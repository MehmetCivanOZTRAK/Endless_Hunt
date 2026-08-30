using System;
using System.Collections.Generic;
using EndlessHunt;
using EndlessHunt.UI;
using EndlessHunt.UI.Common;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class EndlessHuntUICoreValidation
{
    private static readonly string[] PrefabNames =
    {
        "UIRoot",
        "UI_Button",
        "UI_TabButton",
        "UI_CurrencyDisplay",
        "UI_ProgressBar",
        "UI_ItemCard",
        "UI_EquipmentSlot",
        "UI_TalentNode",
        "UI_RewardItem",
        "UI_NotificationBadge",
        "UI_PopupFrame",
        "UI_Toast"
    };

    [MenuItem("Endless Hunt/Validate UI Core")]
    public static void Run()
    {
        var errors = new List<string>();

        ValidateTheme(errors);
        ValidatePrefabs(errors);
        ValidateCommonPrefabReferences(errors);
        ValidateRoot(errors);
        ValidateSafeAreaMath(errors);
        ValidateNavigationAndBack(errors);
        ValidateTabs(errors);
        ValidateCurrency(errors);

        if (errors.Count > 0)
        {
            string separator = Environment.NewLine + "- ";
            throw new BuildFailedException(
                "UI Core validation failed:" + Environment.NewLine + "- " + string.Join(separator, errors));
        }

        Debug.Log(
            "UI CORE VALIDATION PASSED: theme, 12 prefabs, 1080x1920/2340/2160 safe areas, " +
            "popup-first back navigation, screen history, tabs, coins/gems/energy formatting.");
    }

    private static void ValidateTheme(List<string> errors)
    {
        UITheme theme = AssetDatabase.LoadAssetAtPath<UITheme>(
            EndlessHuntUIPrefabFactory.ThemePath);

        if (theme == null)
        {
            errors.Add("Theme asset is missing.");
            return;
        }

        if (theme.buttonNormal == null ||
            theme.panelSprite == null ||
            theme.modalSprite == null ||
            theme.progressFill == null ||
            theme.itemSlot == null)
        {
            errors.Add("Theme is missing one or more required Vector UI sprite references.");
        }

        if (theme.buttonHeight <= 0f || theme.iconSize <= 0f)
        {
            errors.Add("Theme sizing tokens must be positive.");
        }
    }

    private static void ValidatePrefabs(List<string> errors)
    {
        foreach (string prefabName in PrefabNames)
        {
            string path = EndlessHuntUIPrefabFactory.PrefabFolder + "/" + prefabName + ".prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
            {
                errors.Add("Missing prefab: " + path);
                continue;
            }

            foreach (Transform child in prefab.GetComponentsInChildren<Transform>(true))
            {
                int missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject);
                if (missingCount > 0)
                    errors.Add(prefabName + " contains " + missingCount + " missing script(s) on " + child.name + ".");
            }

            if (prefab.GetComponentInChildren<SurvivalGame>(true) != null ||
                prefab.GetComponentInChildren<SurvivorMenu>(true) != null)
            {
                errors.Add(prefabName + " must not contain gameplay or legacy menu controller components.");
            }
        }
    }

    private static void ValidateCommonPrefabReferences(List<string> errors)
    {
        ValidateReferences<UIButton>(
            "UI_Button", errors,
            "theme", "button", "background", "selectedIndicator", "label", "animationController");
        ValidateReferences<UITabButton>(
            "UI_TabButton", errors,
            "button", "background", "selectedIndicator", "notificationBadge",
            "activeSprite", "inactiveSprite", "disabledSprite");
        ValidateReferences<UICurrencyDisplay>(
            "UI_CurrencyDisplay", errors,
            "icon", "valueText", "timerText");
        ValidateReferences<UIProgressBar>(
            "UI_ProgressBar", errors,
            "slider", "valueText");
        ValidateReferences<UIItemCard>(
            "UI_ItemCard", errors,
            "button", "icon", "title", "subtitle", "selectedIndicator", "disabledOverlay");
        ValidateReferences<UIEquipmentSlot>(
            "UI_EquipmentSlot", errors,
            "button", "icon", "levelText", "emptyIndicator", "selectedIndicator");
        ValidateReferences<UITalentNode>(
            "UI_TalentNode", errors,
            "button", "icon", "title", "levelText", "costText", "lockedOverlay");
        ValidateReferences<UIRewardItem>(
            "UI_RewardItem", errors,
            "icon", "title", "amountText");
        ValidateReferences<UINotificationBadge>(
            "UI_NotificationBadge", errors,
            "displayRoot", "label");
        ValidateReferences<UIPopupFrame>(
            "UI_PopupFrame", errors,
            "popup", "titleText", "contentRoot", "footerRoot",
            "closeButton", "primaryAction", "secondaryAction");
        ValidateReferences<UIPopup>(
            "UI_PopupFrame", errors,
            "canvasGroup", "animatedContent", "animationController");
        ValidateReferences<UIToast>(
            "UI_Toast", errors,
            "messageText", "canvasGroup", "animatedRoot", "animationController");
    }

    private static void ValidateReferences<T>(
        string prefabName,
        List<string> errors,
        params string[] propertyNames) where T : Component
    {
        string path = EndlessHuntUIPrefabFactory.PrefabFolder + "/" + prefabName + ".prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) return;

        T component = prefab.GetComponentInChildren<T>(true);
        if (component == null)
        {
            errors.Add(prefabName + " is missing required component " + typeof(T).Name + ".");
            return;
        }

        var serialized = new SerializedObject(component);
        foreach (string propertyName in propertyNames)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                errors.Add(prefabName + "." + typeof(T).Name + " has no serialized field " + propertyName + ".");
                continue;
            }

            if (property.propertyType == SerializedPropertyType.ObjectReference &&
                property.objectReferenceValue == null)
            {
                errors.Add(
                    prefabName + "." + typeof(T).Name + " has an unassigned reference: " + propertyName + ".");
            }
        }
    }

    private static void ValidateRoot(List<string> errors)
    {
        GameObject rootPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            EndlessHuntUIPrefabFactory.PrefabFolder + "/UIRoot.prefab");

        if (rootPrefab == null)
            return;

        UIRoot root = rootPrefab.GetComponent<UIRoot>();
        Canvas canvas = rootPrefab.GetComponent<Canvas>();
        CanvasScaler scaler = rootPrefab.GetComponent<CanvasScaler>();

        if (root == null || canvas == null || scaler == null)
        {
            errors.Add("UIRoot must contain UIRoot, Canvas and CanvasScaler components.");
            return;
        }

        if (root.Theme == null ||
            root.SafeArea == null ||
            root.ScreenLayer == null ||
            root.OverlayLayer == null ||
            root.PopupLayer == null ||
            root.ToastLayer == null ||
            root.ScreenManager == null ||
            root.PopupManager == null ||
            root.ProfileAdapter == null)
        {
            errors.Add("UIRoot has an unassigned core reference.");
        }

        if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize ||
            scaler.referenceResolution != new Vector2(1080f, 1920f) ||
            !Approximately(scaler.matchWidthOrHeight, 0.55f))
        {
            errors.Add("UIRoot CanvasScaler must use 1080x1920, Scale With Screen Size and Match 0.55.");
        }

        if (rootPrefab.GetComponentInChildren<EventSystem>(true) != null)
        {
            errors.Add("UIRoot prefab must not serialize a duplicate EventSystem.");
        }
    }

    private static void ValidateSafeAreaMath(List<string> errors)
    {
        AssertSafeArea(
            errors,
            "1080x1920",
            new Rect(0f, 0f, 1080f, 1920f),
            new Vector2(1080f, 1920f),
            Vector2.zero,
            Vector2.one);

        AssertSafeArea(
            errors,
            "1080x2340",
            new Rect(0f, 80f, 1080f, 2180f),
            new Vector2(1080f, 2340f),
            new Vector2(0f, 80f / 2340f),
            new Vector2(1f, 2260f / 2340f));

        AssertSafeArea(
            errors,
            "1080x2160",
            new Rect(24f, 60f, 1032f, 2040f),
            new Vector2(1080f, 2160f),
            new Vector2(24f / 1080f, 60f / 2160f),
            new Vector2(1056f / 1080f, 2100f / 2160f));
    }

    private static void AssertSafeArea(
        List<string> errors,
        string label,
        Rect safeArea,
        Vector2 screenSize,
        Vector2 expectedMin,
        Vector2 expectedMax)
    {
        SafeAreaController.CalculateAnchors(
            safeArea,
            screenSize,
            true,
            true,
            out Vector2 actualMin,
            out Vector2 actualMax);

        if (!Approximately(actualMin.x, expectedMin.x) ||
            !Approximately(actualMin.y, expectedMin.y) ||
            !Approximately(actualMax.x, expectedMax.x) ||
            !Approximately(actualMax.y, expectedMax.y))
        {
            errors.Add(
                label + " safe-area anchors were " + actualMin + " to " + actualMax +
                ", expected " + expectedMin + " to " + expectedMax + ".");
        }
    }

    private static void ValidateNavigationAndBack(List<string> errors)
    {
        var validationRoot = new GameObject("UI Core Navigation Validation");

        try
        {
            UIScreenManager screenManager = validationRoot.AddComponent<UIScreenManager>();
            UIPopupManager popupManager = validationRoot.AddComponent<UIPopupManager>();
            UIBackHandler backHandler = validationRoot.AddComponent<UIBackHandler>();
            backHandler.Configure(screenManager, popupManager);

            UIScreen home = CreateScreen(validationRoot.transform, "Home");
            UIScreen shop = CreateScreen(validationRoot.transform, "Shop");
            screenManager.Register(home);
            screenManager.Register(shop);

            if (!screenManager.Open("Home", true) || !screenManager.Open("Shop"))
            {
                errors.Add("Screen manager could not open registered screens.");
                return;
            }

            UIPopup popup = CreatePopup(validationRoot.transform, "Confirm");
            popupManager.Register(popup);

            if (!popupManager.OpenPopup("Confirm", false))
            {
                errors.Add("Popup manager could not open a registered popup.");
                return;
            }

            backHandler.HandleBack();

            if (popupManager.HasOpenPopup || screenManager.ActiveScreen != shop)
                errors.Add("Back must close the top popup before changing the active screen.");

            backHandler.HandleBack();

            if (screenManager.ActiveScreen != home)
                errors.Add("Back must navigate to the previous screen after popups are closed.");
        }
        catch (Exception exception)
        {
            errors.Add("Navigation/back validation threw " + exception.GetType().Name + ": " + exception.Message);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(validationRoot);
        }
    }

    private static UIScreen CreateScreen(Transform parent, string id)
    {
        var gameObject = new GameObject(id, typeof(RectTransform), typeof(CanvasGroup), typeof(UIScreen));
        gameObject.transform.SetParent(parent, false);
        UIScreen screen = gameObject.GetComponent<UIScreen>();
        screen.Configure(id);
        return screen;
    }

    private static UIPopup CreatePopup(Transform parent, string id)
    {
        var gameObject = new GameObject(
            id,
            typeof(RectTransform),
            typeof(CanvasGroup),
            typeof(UIAnimationController),
            typeof(UIPopup));

        gameObject.transform.SetParent(parent, false);
        UIPopup popup = gameObject.GetComponent<UIPopup>();
        popup.Configure(id, gameObject.GetComponent<RectTransform>(), true);
        return popup;
    }

    private static void ValidateTabs(List<string> errors)
    {
        var validationRoot = new GameObject("UI Core Tab Validation");

        try
        {
            UITabController controller = validationRoot.AddComponent<UITabController>();
            UITabButton first = CreateTab(validationRoot.transform, "First");
            UITabButton second = CreateTab(validationRoot.transform, "Second");

            controller.Register(first);
            controller.Register(second);
            controller.SelectTab(0);

            if (controller.ActiveTab != first || first.VisualState != UITabVisualState.Active)
            {
                errors.Add("Tab controller could not select the first tab.");
                return;
            }

            controller.SelectTab("Second");

            if (controller.ActiveTab != second ||
                second.VisualState != UITabVisualState.Active ||
                first.VisualState == UITabVisualState.Active)
                errors.Add("Tab controller did not update selected/inactive tab states.");
        }
        catch (Exception exception)
        {
            errors.Add("Tab validation threw " + exception.GetType().Name + ": " + exception.Message);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(validationRoot);
        }
    }

    private static UITabButton CreateTab(Transform parent, string id)
    {
        var gameObject = new GameObject(id, typeof(RectTransform), typeof(Image), typeof(Button), typeof(UITabButton));
        gameObject.transform.SetParent(parent, false);
        UITabButton tab = gameObject.GetComponent<UITabButton>();
        tab.Configure(
            id,
            string.Empty,
            gameObject.GetComponent<Button>(),
            gameObject.GetComponent<Image>(),
            null,
            null,
            null,
            null,
            null);
        return tab;
    }

    private static void ValidateCurrency(List<string> errors)
    {
        if (UICurrencyDisplay.FormatValue(UICurrencyType.Energy, 20, GameProfile.MaxEnergy) != "20/20")
            errors.Add("Energy display must use GameProfile.MaxEnergy and format as 20/20.");

        if (UICurrencyDisplay.FormatValue(UICurrencyType.Coins, 12500, 0) != 12500.ToString("N0"))
            errors.Add("Coin display formatting is incorrect.");

        if (UICurrencyDisplay.FormatValue(UICurrencyType.Gems, 80, 0) != "80")
            errors.Add("Gem display formatting is incorrect.");
    }

    private static bool Approximately(float left, float right)
    {
        return Mathf.Abs(left - right) < 0.0001f;
    }
}
