using System;
using System.Collections.Generic;
using System.Linq;
using EndlessHunt;
using EndlessHunt.UI;
using EndlessHunt.UI.Common;
using EndlessHunt.UI.Screens;
using Spine.Unity;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class EndlessHuntMainMenuValidation
{
    private const string ScenePath = "Assets/Scenes/MainMenu.unity";

    [MenuItem("Endless Hunt/UI/Validate Main Menu")]
    public static void Run()
    {
        var errors = new List<string>();
        ValidateHomePrefab(errors);
        ValidateScene(errors);
        ValidatePresentationData(errors);
        ValidatePreparedNavigation(errors);
        ValidateResponsiveSafeAreas(errors);

        if (errors.Count > 0)
        {
            string separator = Environment.NewLine + "- ";
            throw new BuildFailedException(
                "Main Menu validation failed:" + Environment.NewLine + "- " +
                string.Join(separator, errors));
        }

        Debug.Log(
            "MAIN MENU VALIDATION PASSED: prefab wiring, scene integration, character preview, " +
            "real profile/stage/energy presentation, prepared navigation and 1080x1920/2160/2340 safe areas.");
    }

    private static void ValidateHomePrefab(List<string> errors)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            EndlessHuntMainMenuBuilder.HomePrefabPath);
        if (prefab == null)
        {
            errors.Add("UI_HomeScreen.prefab is missing.");
            return;
        }

        foreach (Transform child in prefab.GetComponentsInChildren<Transform>(true))
        {
            int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject);
            if (missing > 0)
                errors.Add("UI_HomeScreen has " + missing + " missing script(s) on " + child.name + ".");
        }

        HomeScreenPresenter presenter = prefab.GetComponent<HomeScreenPresenter>();
        UIScreen screen = prefab.GetComponent<UIScreen>();
        if (presenter == null || screen == null)
        {
            errors.Add("UI_HomeScreen requires HomeScreenPresenter and UIScreen.");
            return;
        }

        if (!presenter.IsConfigured)
            errors.Add("UI_HomeScreen presenter has an unassigned prefab reference.");
        if (screen.ScreenId != HomeScreenPresenter.ScreenId)
            errors.Add("UI_HomeScreen must use the Home screen id.");
        if (presenter.CharacterPreview == null ||
            presenter.CharacterPreview.SkeletonDataAsset == null)
            errors.Add("Equipped character preview is missing its Spine SkeletonDataAsset.");
        if (presenter.PlayButton == null ||
            presenter.PlayButton.GetComponent<UIButton>() == null)
            errors.Add("Primary Play CTA must use the reusable UIButton component.");
        if (presenter.CoinDisplay == null ||
            presenter.CoinDisplay.CurrencyType != UICurrencyType.Coins ||
            presenter.GemDisplay == null ||
            presenter.GemDisplay.CurrencyType != UICurrencyType.Gems ||
            presenter.EnergyDisplay == null ||
            presenter.EnergyDisplay.CurrencyType != UICurrencyType.Energy)
            errors.Add("Top bar currency prefabs are missing or use incorrect currency types.");

        UITabButton[] tabs = presenter.BottomTabs;
        string[] expectedIds = { "Home", "Equipment", "Talent", "Shop" };
        string[] expectedTargets = { "Home", "Equipment", "Talent", "Shop" };
        if (tabs.Length != expectedIds.Length)
            errors.Add("Bottom navigation must contain exactly four prepared tabs.");
        else
        {
            for (int i = 0; i < tabs.Length; i++)
            {
                if (tabs[i] == null ||
                    tabs[i].TabId != expectedIds[i] ||
                    tabs[i].TargetScreenId != expectedTargets[i])
                    errors.Add("Bottom navigation tab " + i + " has incorrect id/target wiring.");
            }
        }

        if (prefab.GetComponentsInChildren<VerticalLayoutGroup>(true).Length == 0 ||
            prefab.GetComponentsInChildren<HorizontalLayoutGroup>(true).Length < 2)
            errors.Add("Home screen must use responsive vertical and horizontal layout groups.");
        if (prefab.GetComponentsInChildren<UIProgressBar>(true).Length != 0)
            errors.Add("Home screen must not show a fabricated XP progress bar.");
    }

    private static void ValidateScene(List<string> errors)
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        UIRoot[] roots = UnityEngine.Object.FindObjectsByType<UIRoot>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        HomeScreenPresenter[] homes = UnityEngine.Object.FindObjectsByType<HomeScreenPresenter>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        SurvivorMenu[] legacyMenus = UnityEngine.Object.FindObjectsByType<SurvivorMenu>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        EventSystem[] events = UnityEngine.Object.FindObjectsByType<EventSystem>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (roots.Length != 1) errors.Add("MainMenu must contain exactly one UIRoot.");
        if (homes.Length != 1) errors.Add("MainMenu must contain exactly one HomeScreen.");
        if (events.Length != 1) errors.Add("MainMenu must contain exactly one EventSystem.");
        if (legacyMenus.Length == 0)
            errors.Add("Legacy SurvivorMenu must remain preserved for later migrations.");
        else if (legacyMenus.Any(menu => menu.enabled))
            errors.Add("Legacy SurvivorMenu must be disabled after Home presentation migration.");

        Canvas[] activeCanvases = UnityEngine.Object.FindObjectsByType<Canvas>(
                FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(canvas => canvas.gameObject.activeInHierarchy)
            .ToArray();
        if (activeCanvases.Length != 1)
            errors.Add("MainMenu must contain exactly one active Canvas.");

        if (roots.Length == 1 && homes.Length == 1)
        {
            UIRoot root = roots[0];
            HomeScreenPresenter home = homes[0];
            if (!home.transform.IsChildOf(root.ScreenLayer))
                errors.Add("HomeScreen must be a child of UIRoot.ScreenLayer.");
            if (root.SafeArea == null ||
                root.PopupLayer == null ||
                root.ToastLayer == null)
                errors.Add("UIRoot SafeArea/PopupLayer/ToastLayer references are missing.");

            var serialized = new SerializedObject(home);
            foreach (string propertyName in new[] { "profileAdapter", "screenManager", "toast" })
            {
                SerializedProperty property = serialized.FindProperty(propertyName);
                if (property == null || property.objectReferenceValue == null)
                    errors.Add("Scene HomeScreen has an unassigned service: " + propertyName + ".");
            }
        }

        if (!scene.IsValid()) errors.Add("MainMenu scene could not be opened.");
    }

    private static void ValidatePresentationData(List<string> errors)
    {
        if (GameProfile.MaxEnergy != 20)
            errors.Add("Energy maximum must remain backed by GameProfile.MaxEnergy.");
        if (GameProfile.BattleEnergyCost != 5)
            errors.Add("Battle energy cost must preserve the existing value of 5.");

        string formatted = HomeScreenPresenter.FormatBestScore(12500);
        if (!formatted.Contains(12500.ToString("N0")) ||
            formatted.Contains(":") ||
            formatted.Contains("m ") ||
            formatted.Contains("s"))
            errors.Add("BestScore must be presented as a numeric score, not survival time.");

        bool gameSceneEnabled = EditorBuildSettings.scenes.Any(scene =>
            scene.enabled &&
            scene.path.EndsWith("/" + HomeScreenPresenter.GameplaySceneName + ".unity",
                StringComparison.Ordinal));
        if (!gameSceneEnabled)
            errors.Add("The existing GameScene navigation target is not enabled in Build Settings.");
    }

    private static void ValidatePreparedNavigation(List<string> errors)
    {
        var root = new GameObject("Main Menu Navigation Validation");
        try
        {
            UIScreenManager screens = root.AddComponent<UIScreenManager>();
            UITabController tabs = root.AddComponent<UITabController>();
            tabs.Configure(screens);

            UIScreen homeScreen = CreateScreen(root.transform, "Home");
            screens.Register(homeScreen);
            screens.Open(homeScreen, false, true);
            UITabButton home = CreateTab(root.transform, "Home", "Home");
            UITabButton future = CreateTab(root.transform, "Equipment", "Equipment");
            tabs.Register(home);
            tabs.Register(future);

            if (!tabs.SelectTab("Home") || tabs.ActiveTab != home)
                errors.Add("Home tab could not become active.");
            if (tabs.SelectTab("Equipment") ||
                tabs.ActiveTab != home ||
                screens.ActiveScreen != homeScreen)
                errors.Add("Prepared future tabs must not leave Home until their screens exist.");
        }
        catch (Exception exception)
        {
            errors.Add("Prepared navigation validation threw " +
                       exception.GetType().Name + ": " + exception.Message);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
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

    private static UITabButton CreateTab(Transform parent, string id, string target)
    {
        var gameObject = new GameObject(id, typeof(RectTransform), typeof(Image), typeof(Button),
            typeof(UITabButton));
        gameObject.transform.SetParent(parent, false);
        UITabButton tab = gameObject.GetComponent<UITabButton>();
        tab.Configure(id, target, gameObject.GetComponent<Button>(), gameObject.GetComponent<Image>(),
            null, null, null, null, null);
        return tab;
    }

    private static void ValidateResponsiveSafeAreas(List<string> errors)
    {
        ValidateSafeArea(errors, new Vector2(1080f, 1920f),
            new Rect(0f, 0f, 1080f, 1920f));
        ValidateSafeArea(errors, new Vector2(1080f, 2160f),
            new Rect(24f, 60f, 1032f, 2040f));
        ValidateSafeArea(errors, new Vector2(1080f, 2340f),
            new Rect(0f, 80f, 1080f, 2180f));
    }

    private static void ValidateSafeArea(List<string> errors, Vector2 size, Rect safeArea)
    {
        SafeAreaController.CalculateAnchors(
            safeArea, size, true, true, out Vector2 min, out Vector2 max);
        Vector2 expectedMin = new(safeArea.xMin / size.x, safeArea.yMin / size.y);
        Vector2 expectedMax = new(safeArea.xMax / size.x, safeArea.yMax / size.y);
        if ((min - expectedMin).sqrMagnitude > .000001f ||
            (max - expectedMax).sqrMagnitude > .000001f)
            errors.Add(size.x + "x" + size.y + " Safe Area anchors are incorrect.");
    }
}
