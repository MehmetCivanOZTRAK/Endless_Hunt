using EndlessHunt.UI;
using EndlessHunt.UI.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class EndlessHuntUICoreBuilder
{
    [MenuItem("Endless Hunt/UI/Build Or Refresh UI Core Assets")]
    public static void BuildAssets()
    {
        EnsureFolders();
        UITheme theme = BuildTheme();
        BuildUIRoot(theme);
        BuildButton(theme);
        BuildCurrencyDisplay(theme);
        BuildProgressBar(theme);
        EndlessHuntUICommonPrefabBuilder.Build(theme);
        BuildTabButton(theme);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Endless Hunt UI Core assets refreshed without modifying gameplay scenes.");
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets/EndlessHunt/UI/Core");
        EnsureFolder("Assets/EndlessHunt/UI/Common");
        EnsureFolder("Assets/EndlessHunt/UI/Common/Prefabs");
        EnsureFolder("Assets/EndlessHunt/UI/Theme");
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int slash = path.LastIndexOf('/');
        string parent = path.Substring(0, slash);
        string name = path.Substring(slash + 1);
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }

    private static UITheme BuildTheme()
    {
        UITheme theme = AssetDatabase.LoadAssetAtPath<UITheme>(EndlessHuntUIPrefabFactory.ThemePath);
        if (theme == null)
        {
            theme = ScriptableObject.CreateInstance<UITheme>();
            AssetDatabase.CreateAsset(theme, EndlessHuntUIPrefabFactory.ThemePath);
        }
        theme.buttonNormal = EndlessHuntUIPrefabFactory.Ui("Buttons/buttonAdvanced_blue.png");
        theme.buttonPressed = EndlessHuntUIPrefabFactory.Ui("Buttons/buttonAdvanced_purple.png");
        theme.buttonSelected = EndlessHuntUIPrefabFactory.Ui("Buttons/buttonAdvanced_yellow.png");
        theme.buttonDisabled = EndlessHuntUIPrefabFactory.Ui("Buttons/buttonAdvanced_black.png");
        theme.panelSprite = EndlessHuntUIPrefabFactory.Ui("Panels/panel_black.png");
        theme.modalSprite = EndlessHuntUIPrefabFactory.Ui("Modals/modalSimple_v1.png");
        theme.progressBackground = EndlessHuntUIPrefabFactory.Ui("ProgressBars/progressBarBase_black.png");
        theme.progressFill = EndlessHuntUIPrefabFactory.Ui("ProgressBars/progressBar_cyan.png");
        theme.itemSlot = EndlessHuntUIPrefabFactory.Ui("Item Slots/itemSlotAdvanced_blue.png");
        theme.tabInactive = EndlessHuntUIPrefabFactory.Ui("Buttons/shop_1ButtonDefault.png");
        theme.tabActive = EndlessHuntUIPrefabFactory.Ui("Buttons/shopButtonPressed.png");
        theme.badgeSprite = EndlessHuntUIPrefabFactory.Ui("Labels/labelAdvanced_red.png");
        theme.toastSprite = EndlessHuntUIPrefabFactory.Ui("Labels/labelAdvanced_black.png");
        EditorUtility.SetDirty(theme);
        return theme;
    }

    private static void BuildUIRoot(UITheme theme)
    {
        RectTransform root = EndlessHuntUIPrefabFactory.Root("UIRoot", new Vector2(1080f, 1920f),
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(UIScreenManager),
            typeof(UIPopupManager), typeof(UIBackHandler), typeof(GameProfileUIAdapter), typeof(UIRoot));
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = .55f;

        RectTransform safe = EndlessHuntUIPrefabFactory.Rect("SafeArea", root, Vector2.zero, Vector2.one,
            Vector2.zero, Vector2.zero);
        SafeAreaController safeArea = safe.gameObject.AddComponent<SafeAreaController>();
        EndlessHuntUIPrefabFactory.Stretch(safe);
        RectTransform screenLayer = Layer("ScreenLayer", safe);
        RectTransform overlayLayer = Layer("OverlayLayer", safe);
        RectTransform popupLayer = Layer("PopupLayer", safe);
        RectTransform toastLayer = Layer("ToastLayer", safe);

        UIScreenManager screens = root.GetComponent<UIScreenManager>();
        UIPopupManager popups = root.GetComponent<UIPopupManager>();
        UIBackHandler back = root.GetComponent<UIBackHandler>();
        GameProfileUIAdapter adapter = root.GetComponent<GameProfileUIAdapter>();
        screens.Configure(screenLayer);
        popups.Configure(popupLayer);
        back.Configure(screens, popups);
        root.GetComponent<UIRoot>().Configure(theme, safeArea, screenLayer, popupLayer, overlayLayer,
            toastLayer, screens, popups, back, adapter);
        EndlessHuntUIPrefabFactory.Save(root, "UIRoot");
    }

    private static RectTransform Layer(string name, Transform parent)
    {
        RectTransform layer = EndlessHuntUIPrefabFactory.Rect(name, parent, Vector2.zero, Vector2.one,
            Vector2.zero, Vector2.zero);
        EndlessHuntUIPrefabFactory.Stretch(layer);
        return layer;
    }

    private static void BuildButton(UITheme theme)
    {
        RectTransform root = EndlessHuntUIPrefabFactory.Root("UI_Button", new Vector2(320f, theme.buttonHeight),
            typeof(Image), typeof(Button), typeof(LayoutElement), typeof(UIAnimationController), typeof(UIButton));
        Image background = root.GetComponent<Image>();
        background.sprite = theme.buttonNormal;
        background.raycastTarget = true;
        EndlessHuntUIPrefabFactory.ApplySliced(background);
        Button button = root.GetComponent<Button>();
        button.targetGraphic = background;
        button.transition = Selectable.Transition.None;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        LayoutElement layout = root.GetComponent<LayoutElement>();
        layout.minHeight = theme.buttonHeight;
        layout.preferredHeight = theme.buttonHeight;
        layout.minWidth = 176f;
        Text label = EndlessHuntUIPrefabFactory.Text("Label", root, "BUTTON", theme.bodySize,
            theme.textPrimary, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one,
            new Vector2(theme.commonSpacing, theme.smallSpacing),
            new Vector2(-theme.commonSpacing, -theme.smallSpacing), FontStyle.Bold);
        Image selection = EndlessHuntUIPrefabFactory.AddSelection(root);
        selection.transform.SetAsFirstSibling();
        root.GetComponent<UIButton>().Configure(theme, button, background, label, selection);
        EndlessHuntUIPrefabFactory.Save(root, "UI_Button");
    }

    private static void BuildCurrencyDisplay(UITheme theme)
    {
        RectTransform root = EndlessHuntUIPrefabFactory.Root("UI_CurrencyDisplay", new Vector2(320f, 84f),
            typeof(Image), typeof(LayoutElement), typeof(HorizontalLayoutGroup), typeof(UICurrencyDisplay));
        Image background = root.GetComponent<Image>();
        background.sprite = EndlessHuntUIPrefabFactory.Ui("Labels/label_backgroundV2.png");
        EndlessHuntUIPrefabFactory.ApplySliced(background);
        LayoutElement layout = root.GetComponent<LayoutElement>();
        layout.minHeight = 64f;
        layout.preferredHeight = 84f;
        layout.preferredWidth = 320f;
        HorizontalLayoutGroup group = root.GetComponent<HorizontalLayoutGroup>();
        group.padding = new RectOffset(12, 12, 8, 8);
        group.spacing = theme.smallSpacing;
        group.childAlignment = TextAnchor.MiddleCenter;
        group.childControlWidth = true;
        group.childControlHeight = true;
        group.childForceExpandWidth = false;
        group.childForceExpandHeight = true;

        RectTransform iconRoot = EndlessHuntUIPrefabFactory.Root("Icon", new Vector2(theme.iconSize, theme.iconSize),
            typeof(Image), typeof(LayoutElement));
        iconRoot.SetParent(root, false);
        Image icon = iconRoot.GetComponent<Image>();
        icon.sprite = EndlessHuntUIPrefabFactory.Ui("Icons/128px/coin_icon_128px.png");
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        LayoutElement iconLayout = iconRoot.GetComponent<LayoutElement>();
        iconLayout.preferredWidth = theme.iconSize;
        iconLayout.preferredHeight = theme.iconSize;

        Text value = EndlessHuntUIPrefabFactory.Text("Value", root, "0", theme.bodySize, theme.textPrimary,
            TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, FontStyle.Bold);
        value.gameObject.AddComponent<LayoutElement>().preferredWidth = 120f;
        Text timer = EndlessHuntUIPrefabFactory.Text("Timer", root, string.Empty, theme.captionSize,
            theme.accent, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            FontStyle.Bold);
        timer.gameObject.AddComponent<LayoutElement>().preferredWidth = 92f;
        timer.gameObject.SetActive(false);
        root.GetComponent<UICurrencyDisplay>().Configure(UICurrencyType.Coins, null, icon, value, timer);
        EndlessHuntUIPrefabFactory.Save(root, "UI_CurrencyDisplay");
    }

    private static void BuildProgressBar(UITheme theme)
    {
        RectTransform root = EndlessHuntUIPrefabFactory.Root("UI_ProgressBar", new Vector2(520f, 58f),
            typeof(Image), typeof(Slider), typeof(LayoutElement), typeof(UIProgressBar));
        Image background = root.GetComponent<Image>();
        background.sprite = theme.progressBackground;
        EndlessHuntUIPrefabFactory.ApplySliced(background);
        Slider slider = root.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;
        slider.interactable = false;
        RectTransform fillArea = EndlessHuntUIPrefabFactory.Rect("Fill Area", root, Vector2.zero, Vector2.one,
            new Vector2(10f, 10f), new Vector2(-10f, -10f));
        Image fill = EndlessHuntUIPrefabFactory.Image("Fill", fillArea, theme.progressFill, Vector2.zero,
            Vector2.one, Vector2.zero, Vector2.zero, Color.white);
        EndlessHuntUIPrefabFactory.ApplySliced(fill);
        slider.fillRect = fill.rectTransform;
        slider.targetGraphic = fill;
        Text value = EndlessHuntUIPrefabFactory.Text("Value", root, "0/1", theme.captionSize,
            theme.textPrimary, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one,
            new Vector2(12f, 4f), new Vector2(-12f, -4f), FontStyle.Bold);
        root.GetComponent<UIProgressBar>().Configure(slider, value);
        EndlessHuntUIPrefabFactory.Save(root, "UI_ProgressBar");
    }

    private static void BuildTabButton(UITheme theme)
    {
        RectTransform root = EndlessHuntUIPrefabFactory.Root("UI_TabButton", new Vector2(200f, 132f),
            typeof(Image), typeof(Button), typeof(LayoutElement), typeof(UITabButton));
        Image background = root.GetComponent<Image>();
        background.sprite = theme.tabInactive;
        background.raycastTarget = true;
        Button button = root.GetComponent<Button>();
        button.targetGraphic = background;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        LayoutElement layout = root.GetComponent<LayoutElement>();
        layout.minWidth = 88f;
        layout.minHeight = 88f;
        layout.preferredWidth = 200f;
        layout.preferredHeight = 132f;
        Image selection = EndlessHuntUIPrefabFactory.AddSelection(root);
        selection.transform.SetAsFirstSibling();
        EndlessHuntUIPrefabFactory.Text("Label", root, "TAB", theme.captionSize, theme.textPrimary,
            TextAnchor.LowerCenter, Vector2.zero, Vector2.one, new Vector2(4f, 2f),
            new Vector2(-4f, -4f), FontStyle.Bold);
        UINotificationBadge badge = EndlessHuntUICommonPrefabBuilder.AddBadgeVisual(root, theme);
        root.GetComponent<UITabButton>().Configure("Tab", string.Empty, button, background, selection, badge,
            theme.tabActive, theme.tabInactive, theme.buttonDisabled);
        EndlessHuntUIPrefabFactory.Save(root, "UI_TabButton");
    }
}
