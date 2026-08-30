using System;
using System.Collections.Generic;
using EndlessHunt;
using EndlessHunt.UI;
using EndlessHunt.UI.Common;
using EndlessHunt.UI.Screens;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class EndlessHuntMainMenuBuilder
{
    public const string HomePrefabPath = "Assets/EndlessHunt/UI/Screens/Home/UI_HomeScreen.prefab";
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string MainMenuToastName = "Main Menu Toast";
    private const string MainMenuSettingsName = "Main Menu Settings Popup";
    private const string MapAssetFolder =
        "Assets/Layer Lab/2D Art Maker/AMCasual Character/Demo/Map/";

    private static readonly string[] StageDecorPaths =
    {
        "tent.png", "building2.png", "car1.png", "tree3.png", "campfire.png"
    };

    private readonly struct CurrencyViews
    {
        public readonly UICurrencyDisplay Coins;
        public readonly UICurrencyDisplay Gems;
        public readonly UICurrencyDisplay Energy;

        public CurrencyViews(UICurrencyDisplay coins, UICurrencyDisplay gems, UICurrencyDisplay energy)
        {
            Coins = coins;
            Gems = gems;
            Energy = energy;
        }
    }

    private readonly struct StageViews
    {
        public readonly UIStageCarousel Carousel;
        public readonly Text Number;
        public readonly Text Name;
        public readonly Text Score;

        public StageViews(UIStageCarousel carousel, Text number, Text name, Text score)
        {
            Carousel = carousel;
            Number = number;
            Name = name;
            Score = score;
        }
    }

    private readonly struct NavigationViews
    {
        public readonly UITabController Controller;
        public readonly UITabButton Home;
        public readonly UITabButton Equipment;
        public readonly UITabButton Talent;
        public readonly UITabButton Shop;

        public NavigationViews(UITabController controller, UITabButton home, UITabButton equipment,
            UITabButton talent, UITabButton shop)
        {
            Controller = controller;
            Home = home;
            Equipment = equipment;
            Talent = talent;
            Shop = shop;
        }
    }

    [MenuItem("Endless Hunt/UI/Build And Integrate Main Menu")]
    public static void BuildAndIntegrate()
    {
        EndlessHuntUICoreBuilder.BuildAssets();
        BuildAssets();
        IntegrateMainMenuScene();
        Debug.Log("Endless Hunt Main Menu prefab built and integrated without deleting legacy menu systems.");
    }

    public static void BuildAssets()
    {
        EnsureFolder("Assets/EndlessHunt/UI/Screens");
        EnsureFolder("Assets/EndlessHunt/UI/Screens/Home");

        UITheme theme = AssetDatabase.LoadAssetAtPath<UITheme>(EndlessHuntUIPrefabFactory.ThemePath);
        if (theme == null) throw new MissingReferenceException("EndlessHuntUITheme.asset is missing.");

        BuildHomePrefab(theme);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("UI_HomeScreen prefab refreshed from UI Core and Vector UI Pack assets.");
    }

    public static void EnsureAssets()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(HomePrefabPath) == null)
        {
            BuildAssets();
            return;
        }

        Debug.Log("Preserved existing UI_HomeScreen prefab.");
    }

    public static void IntegrateMainMenuScene()
    {
        Scene scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
        GameObject uiRootPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            EndlessHuntUIPrefabFactory.PrefabPath("UIRoot"));
        GameObject homePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HomePrefabPath);
        GameObject toastPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            EndlessHuntUIPrefabFactory.PrefabPath("UI_Toast"));
        GameObject settingsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            EndlessHuntUIPrefabFactory.PrefabPath("UI_PopupFrame"));
        if (uiRootPrefab == null || homePrefab == null || toastPrefab == null || settingsPrefab == null)
            throw new MissingReferenceException("Main Menu integration prefabs are missing.");

        UIRoot[] existingRoots = UnityEngine.Object.FindObjectsByType<UIRoot>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (existingRoots.Length > 1)
            throw new InvalidOperationException("MainMenu contains more than one UIRoot.");

        UIRoot uiRoot = existingRoots.Length == 1
            ? existingRoots[0]
            : ((GameObject)PrefabUtility.InstantiatePrefab(uiRootPrefab)).GetComponent<UIRoot>();
        uiRoot.gameObject.name = "UIRoot";
        uiRoot.gameObject.SetActive(true);

        foreach (HomeScreenPresenter existing in UnityEngine.Object.FindObjectsByType<HomeScreenPresenter>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
            UnityEngine.Object.DestroyImmediate(existing.gameObject);
        foreach (UIToast existing in uiRoot.ToastLayer.GetComponentsInChildren<UIToast>(true))
            if (existing.gameObject.name == MainMenuToastName)
                UnityEngine.Object.DestroyImmediate(existing.gameObject);
        foreach (UIPopup existing in uiRoot.PopupLayer.GetComponentsInChildren<UIPopup>(true))
            if (existing.gameObject.name == MainMenuSettingsName)
                UnityEngine.Object.DestroyImmediate(existing.gameObject);

        GameObject homeInstance = (GameObject)PrefabUtility.InstantiatePrefab(homePrefab, uiRoot.ScreenLayer);
        homeInstance.name = "HomeScreen";
        Stretch(homeInstance.GetComponent<RectTransform>());
        HomeScreenPresenter presenter = homeInstance.GetComponent<HomeScreenPresenter>();

        GameObject toastInstance = (GameObject)PrefabUtility.InstantiatePrefab(toastPrefab, uiRoot.ToastLayer);
        toastInstance.name = MainMenuToastName;
        RectTransform toastRect = toastInstance.GetComponent<RectTransform>();
        toastRect.anchorMin = new Vector2(.15f, .47f);
        toastRect.anchorMax = new Vector2(.85f, .53f);
        toastRect.offsetMin = Vector2.zero;
        toastRect.offsetMax = Vector2.zero;
        UIToast toast = toastInstance.GetComponent<UIToast>();
        toast.HideImmediate();

        GameObject settingsInstance = (GameObject)PrefabUtility.InstantiatePrefab(
            settingsPrefab, uiRoot.PopupLayer);
        settingsInstance.name = MainMenuSettingsName;
        Stretch(settingsInstance.GetComponent<RectTransform>());
        UIPopup settingsPopup = settingsInstance.GetComponent<UIPopup>();
        UIPopupFrame settingsFrame = settingsInstance.GetComponent<UIPopupFrame>();
        RectTransform settingsCard = settingsInstance.transform.Find("Popup Card") as RectTransform;
        settingsPopup.Configure(HomeScreenPresenter.SettingsPopupId, settingsCard, true);
        settingsFrame.SetTitle("AYARLAR");
        settingsFrame.SetActionsVisible(false, false);
        EndlessHuntUIPrefabFactory.Text(
            "Settings Message", settingsFrame.ContentRoot,
            "AYARLAR SONRAKİ AŞAMADA", 30, Color.white,
            TextAnchor.MiddleCenter, Vector2.zero, Vector2.one,
            new Vector2(28f, 20f), new Vector2(-28f, -20f), FontStyle.Bold);
        settingsInstance.SetActive(false);

        presenter.BindServices(
            uiRoot.ProfileAdapter, uiRoot.ScreenManager, uiRoot.PopupManager, toast);
        uiRoot.ScreenManager.Configure(uiRoot.ScreenLayer, presenter.Screen);
        uiRoot.PopupManager.Configure(uiRoot.PopupLayer);
        PrefabUtility.RecordPrefabInstancePropertyModifications(presenter);
        PrefabUtility.RecordPrefabInstancePropertyModifications(uiRoot.ScreenManager);
        PrefabUtility.RecordPrefabInstancePropertyModifications(uiRoot.PopupManager);
        PrefabUtility.RecordPrefabInstancePropertyModifications(settingsPopup);

        foreach (SurvivorMenu legacyMenu in UnityEngine.Object.FindObjectsByType<SurvivorMenu>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
            legacyMenu.enabled = false;

        Canvas rootCanvas = uiRoot.GetComponent<Canvas>();
        foreach (Canvas canvas in UnityEngine.Object.FindObjectsByType<Canvas>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (canvas == rootCanvas) continue;
            canvas.gameObject.SetActive(false);
        }

        EventSystem[] eventSystems = UnityEngine.Object.FindObjectsByType<EventSystem>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (eventSystems.Length == 0)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        else if (eventSystems.Length > 1)
            throw new InvalidOperationException("MainMenu contains duplicate EventSystems.");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("MainMenu scene now uses one active UIRoot and preserves the disabled legacy menu.");
    }

    private static void BuildHomePrefab(UITheme theme)
    {
        RectTransform root = EndlessHuntUIPrefabFactory.Root(
            "UI_HomeScreen",
            new Vector2(1080f, 1920f),
            typeof(Image),
            typeof(CanvasGroup),
            typeof(UIScreen),
            typeof(HomeScreenPresenter));
        Image background = root.GetComponent<Image>();
        background.sprite = EndlessHuntUIPrefabFactory.Ui("Panels/panel_blue.png");
        background.color = theme.background;
        background.raycastTarget = true;
        EndlessHuntUIPrefabFactory.ApplySliced(background);

        UIScreen screen = root.GetComponent<UIScreen>();
        screen.Configure(HomeScreenPresenter.ScreenId, true);

        RectTransform content = EndlessHuntUIPrefabFactory.Rect(
            "Home Layout", root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        VerticalLayoutGroup vertical = content.gameObject.AddComponent<VerticalLayoutGroup>();
        vertical.padding = new RectOffset(18, 18, 10, 10);
        vertical.spacing = theme.smallSpacing;
        vertical.childAlignment = TextAnchor.UpperCenter;
        vertical.childControlWidth = true;
        vertical.childControlHeight = true;
        vertical.childForceExpandWidth = true;
        vertical.childForceExpandHeight = false;

        Text levelText;
        CurrencyViews currencies = BuildTopBar(content, theme, out levelText);
        Button settingsButton;
        UIStageCarousel carousel = BuildStageCarousel(content, theme, out settingsButton);
        StageViews stage = BuildStageInfo(content, theme, carousel);
        Button playButton;
        UIButton playButtonView;
        Text energyCost;
        Text energyWarning;
        BuildPlayArea(content, theme, out playButton, out playButtonView, out energyCost, out energyWarning);
        NavigationViews navigation = BuildNavigation(content, theme);

        HomeScreenPresenter presenter = root.GetComponent<HomeScreenPresenter>();
        presenter.Configure(
            screen,
            levelText,
            currencies.Coins,
            currencies.Gems,
            currencies.Energy,
            stage.Carousel,
            stage.Number,
            stage.Name,
            stage.Score,
            playButton,
            playButtonView,
            energyCost,
            energyWarning,
            settingsButton,
            navigation.Controller,
            navigation.Home,
            navigation.Equipment,
            navigation.Talent,
            navigation.Shop);

        GameObject saved = PrefabUtility.SaveAsPrefabAsset(root.gameObject, HomePrefabPath);
        UnityEngine.Object.DestroyImmediate(root.gameObject);
        if (saved == null) throw new UnityException("Failed to save " + HomePrefabPath);
    }

    private static CurrencyViews BuildTopBar(Transform parent, UITheme theme, out Text levelText)
    {
        RectTransform top = Section("Top Bar", parent, 116f);
        Image topBackground = top.gameObject.AddComponent<Image>();
        topBackground.sprite = EndlessHuntUIPrefabFactory.Ui("Modals/topBarEmpty.png");
        topBackground.color = Color.white;
        topBackground.raycastTarget = false;

        HorizontalLayoutGroup row = top.gameObject.AddComponent<HorizontalLayoutGroup>();
        row.padding = new RectOffset(8, 8, 6, 6);
        row.spacing = 6f;
        row.childAlignment = TextAnchor.MiddleCenter;
        row.childControlWidth = true;
        row.childControlHeight = true;
        row.childForceExpandWidth = false;
        row.childForceExpandHeight = true;

        RectTransform profile = EndlessHuntUIPrefabFactory.Root(
            "Player Level", new Vector2(220f, 100f), typeof(Image), typeof(LayoutElement));
        profile.SetParent(top, false);
        Image profileBackground = profile.GetComponent<Image>();
        profileBackground.sprite = EndlessHuntUIPrefabFactory.Ui("Cards/cardShop_yellow.png");
        profileBackground.raycastTarget = false;
        EndlessHuntUIPrefabFactory.ApplySliced(profileBackground);
        LayoutElement profileLayout = profile.GetComponent<LayoutElement>();
        profileLayout.minWidth = 210f;
        profileLayout.preferredWidth = 220f;

        Image avatar = EndlessHuntUIPrefabFactory.Image(
            "Avatar", profile,
            EndlessHuntUIPrefabFactory.Ui("Icons/128px/adventure_icon_128px.png"),
            new Vector2(.03f, .08f), new Vector2(.36f, .92f),
            Vector2.zero, Vector2.zero, Color.white);
        avatar.preserveAspect = true;
        levelText = EndlessHuntUIPrefabFactory.Text(
            "Level", profile, "LV. 1", theme.bodySize, theme.textPrimary,
            TextAnchor.MiddleLeft, new Vector2(.38f, .12f), new Vector2(.98f, .88f),
            Vector2.zero, Vector2.zero, FontStyle.Bold);

        UICurrencyDisplay energy = BuildCurrency(top, theme, UICurrencyType.Energy,
            "Energy", "Icons/128px/energy_icon_128px.png", 264f);
        UICurrencyDisplay gems = BuildCurrency(top, theme, UICurrencyType.Gems,
            "Gems", "Icons/128px/gem_icon_128px.png", 240f);
        UICurrencyDisplay coins = BuildCurrency(top, theme, UICurrencyType.Coins,
            "Coins", "Icons/128px/coin_icon_128px.png", 240f);
        return new CurrencyViews(coins, gems, energy);
    }

    private static UICurrencyDisplay BuildCurrency(
        Transform parent, UITheme theme, UICurrencyType type, string name,
        string iconPath, float preferredWidth)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            EndlessHuntUIPrefabFactory.PrefabPath("UI_CurrencyDisplay"));
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        instance.name = name;

        LayoutElement layout = instance.GetComponent<LayoutElement>();
        layout.minWidth = preferredWidth - 12f;
        layout.preferredWidth = preferredWidth;
        layout.flexibleWidth = 0f;
        layout.minHeight = 76f;
        layout.preferredHeight = 96f;
        Image background = instance.GetComponent<Image>();
        background.sprite = EndlessHuntUIPrefabFactory.Ui("Labels/label_backgroundV2.png");
        background.color = Color.white;

        HorizontalLayoutGroup row = instance.GetComponent<HorizontalLayoutGroup>();
        row.padding = new RectOffset(8, 10, 7, 7);
        row.spacing = 5f;

        Image icon = instance.transform.Find("Icon").GetComponent<Image>();
        icon.sprite = EndlessHuntUIPrefabFactory.Ui(iconPath);
        icon.GetComponent<LayoutElement>().preferredWidth = 44f;
        Text value = instance.transform.Find("Value").GetComponent<Text>();
        value.fontSize = theme.bodySize - 2;
        value.GetComponent<LayoutElement>().preferredWidth = type == UICurrencyType.Energy ? 112f : 104f;
        Text timer = instance.transform.Find("Timer").GetComponent<Text>();
        timer.gameObject.SetActive(false);

        UICurrencyDisplay display = instance.GetComponent<UICurrencyDisplay>();
        display.Configure(type, null, icon, value, timer);
        return display;
    }

    private static UIStageCarousel BuildStageCarousel(
        Transform parent, UITheme theme, out Button settingsButton)
    {
        RectTransform area = Section("Map Stage Carousel", parent, 860f, 1f);
        Image areaFrame = area.gameObject.AddComponent<Image>();
        areaFrame.sprite = EndlessHuntUIPrefabFactory.Ui("Containers/cardContainer_black.png");
        areaFrame.raycastTarget = false;
        EndlessHuntUIPrefabFactory.ApplySliced(areaFrame);

        RectTransform scrollRoot = EndlessHuntUIPrefabFactory.Rect(
            "MapCarousel", area, Vector2.zero, Vector2.one,
            new Vector2(8f, 8f), new Vector2(-8f, -8f));
        ScrollRect scrollRect = scrollRoot.gameObject.AddComponent<ScrollRect>();
        RectTransform viewport = EndlessHuntUIPrefabFactory.Rect(
            "Viewport", scrollRoot, Vector2.zero, Vector2.one,
            new Vector2(46f, 10f), new Vector2(-46f, -10f));
        Image viewportGraphic = viewport.gameObject.AddComponent<Image>();
        viewportGraphic.color = new Color(1f, 1f, 1f, .001f);
        viewportGraphic.raycastTarget = true;
        viewport.gameObject.AddComponent<RectMask2D>();

        RectTransform content = EndlessHuntUIPrefabFactory.Rect(
            "Content", viewport, new Vector2(0f, 0f), new Vector2(0f, 1f),
            Vector2.zero, Vector2.zero);
        content.pivot = new Vector2(0f, .5f);
        content.anchoredPosition = Vector2.zero;
        HorizontalLayoutGroup layout = content.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(92, 92, 12, 12);
        layout.spacing = 24f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;
        ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        var cards = new List<UIStageCardView>(GameCatalog.MapNames.Length);
        for (int i = 0; i < GameCatalog.MapNames.Length; i++)
            cards.Add(BuildStageCard(content, theme, i));

        scrollRect.content = content;
        scrollRect.viewport = viewport;
        scrollRect.horizontal = true;
        scrollRect.vertical = false;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.inertia = false;
        scrollRect.scrollSensitivity = 0f;

        UIStageCarousel carousel = scrollRoot.gameObject.AddComponent<UIStageCarousel>();
        carousel.Configure(scrollRect, content, cards.ToArray());

        settingsButton = EndlessHuntSceneFactory.Button(
            "Settings", area,
            EndlessHuntUIPrefabFactory.Ui("Buttons/buttonSquare_blue.png"),
            string.Empty, theme.captionSize,
            new Vector2(.88f, .865f), new Vector2(.975f, .975f),
            Vector2.zero, Vector2.zero);
        Image settingsIcon = EndlessHuntUIPrefabFactory.Image(
            "Icon", settingsButton.transform,
            EndlessHuntUIPrefabFactory.Ui("Icons/128px/settings_icon_128px.png"),
            new Vector2(.16f, .16f), new Vector2(.84f, .84f),
            Vector2.zero, Vector2.zero, Color.white);
        settingsIcon.preserveAspect = true;
        return carousel;
    }

    private static UIStageCardView BuildStageCard(Transform parent, UITheme theme, int index)
    {
        RectTransform slot = EndlessHuntUIPrefabFactory.Root(
            "StageCard_" + (index + 1), new Vector2(720f, 820f),
            typeof(LayoutElement), typeof(UIStageCardView));
        slot.SetParent(parent, false);
        LayoutElement slotLayout = slot.GetComponent<LayoutElement>();
        slotLayout.minWidth = 720f;
        slotLayout.preferredWidth = 720f;
        slotLayout.flexibleWidth = 0f;
        slotLayout.minHeight = 680f;

        RectTransform visual = EndlessHuntUIPrefabFactory.Rect(
            "Stage Visual", slot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        CanvasGroup group = visual.gameObject.AddComponent<CanvasGroup>();
        Image frame = visual.gameObject.AddComponent<Image>();
        frame.sprite = EndlessHuntUIPrefabFactory.Ui("Containers/cardContainer_cream.png");
        frame.raycastTarget = false;
        EndlessHuntUIPrefabFactory.ApplySliced(frame);

        Color ground = index < GameCatalog.MapGround.Length
            ? GameCatalog.MapGround[index]
            : theme.panel;
        ground = Color.Lerp(ground, Color.white, .12f);
        ground.a = 1f;
        Image map = EndlessHuntUIPrefabFactory.Image(
            "Map Artwork", visual,
            EndlessHuntSceneFactory.Sprite(MapAssetFolder + "background.png"),
            new Vector2(.035f, .035f), new Vector2(.965f, .965f),
            Vector2.zero, Vector2.zero, ground);
        map.type = Image.Type.Simple;
        map.raycastTarget = false;

        Image chapterHeader = EndlessHuntUIPrefabFactory.Image(
            "Chapter Badge", visual,
            EndlessHuntUIPrefabFactory.Ui("Headers/header_yellow.png"),
            new Vector2(.25f, .84f), new Vector2(.75f, .975f),
            Vector2.zero, Vector2.zero, Color.white);
        EndlessHuntUIPrefabFactory.Text(
            "Chapter", chapterHeader.transform, "BÖLÜM " + (index + 1), theme.bodySize,
            theme.textPrimary, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one,
            new Vector2(20f, 6f), new Vector2(-20f, -6f), FontStyle.Bold);

        string primaryPath = StageDecorPaths[index % StageDecorPaths.Length];
        Image primary = EndlessHuntUIPrefabFactory.Image(
            "Map Landmark", visual, EndlessHuntSceneFactory.Sprite(MapAssetFolder + primaryPath),
            new Vector2(.2f, .19f), new Vector2(.8f, .76f),
            Vector2.zero, Vector2.zero, Color.white);
        primary.preserveAspect = true;
        primary.raycastTarget = false;

        string secondaryPath = index switch
        {
            0 => "campfire.png",
            1 => "object4.png",
            2 => "building2.png",
            3 => "object5.png",
            _ => "object6.png"
        };
        Image secondary = EndlessHuntUIPrefabFactory.Image(
            "Map Detail", visual, EndlessHuntSceneFactory.Sprite(MapAssetFolder + secondaryPath),
            new Vector2(.07f, .1f), new Vector2(.34f, .38f),
            Vector2.zero, Vector2.zero, Color.white);
        secondary.preserveAspect = true;
        secondary.raycastTarget = false;

        Image lockedOverlay = EndlessHuntUIPrefabFactory.Image(
            "Locked Overlay", visual,
            EndlessHuntUIPrefabFactory.Ui("Containers/cardContainer_black.png"),
            new Vector2(.035f, .035f), new Vector2(.965f, .965f),
            Vector2.zero, Vector2.zero, new Color(.08f, .09f, .13f, .86f));
        EndlessHuntUIPrefabFactory.ApplySliced(lockedOverlay);
        Image key = EndlessHuntUIPrefabFactory.Image(
            "Lock Icon", lockedOverlay.transform,
            EndlessHuntUIPrefabFactory.Ui("Icons/128px/key_icon_128px.png"),
            new Vector2(.38f, .46f), new Vector2(.62f, .7f),
            Vector2.zero, Vector2.zero, Color.white);
        key.preserveAspect = true;
        Text lockText = EndlessHuntUIPrefabFactory.Text(
            "Locked Label", lockedOverlay.transform, "KİLİTLİ", theme.titleSize,
            theme.warning, TextAnchor.MiddleCenter,
            new Vector2(.12f, .3f), new Vector2(.88f, .47f),
            Vector2.zero, Vector2.zero, FontStyle.Bold);

        UIStageCardView view = slot.GetComponent<UIStageCardView>();
        view.Configure(index, visual, group, frame, lockedOverlay.gameObject, lockText);
        view.SetState(index == 0, index == 0);
        return view;
    }

    private static StageViews BuildStageInfo(
        Transform parent, UITheme theme, UIStageCarousel carousel)
    {
        RectTransform stage = Section("Stage Info", parent, 174f);
        Image background = stage.gameObject.AddComponent<Image>();
        background.sprite = EndlessHuntUIPrefabFactory.Ui("Modals/modalSimple_v2.png");
        background.color = Color.white;
        background.raycastTarget = false;
        EndlessHuntUIPrefabFactory.ApplySliced(background);

        Text number = EndlessHuntUIPrefabFactory.Text(
            "Stage Number", stage, "BÖLÜM 1", theme.captionSize, theme.accent,
            TextAnchor.MiddleCenter, new Vector2(.08f, .69f), new Vector2(.92f, .96f),
            Vector2.zero, Vector2.zero, FontStyle.Bold);
        Text name = EndlessHuntUIPrefabFactory.Text(
            "Stage Name", stage, "YEŞİL KAMP", theme.titleSize - 4,
            new Color(.12f, .09f, .16f, 1f), TextAnchor.MiddleCenter,
            new Vector2(.08f, .28f), new Vector2(.92f, .72f),
            Vector2.zero, Vector2.zero, FontStyle.Bold);
        Text score = EndlessHuntUIPrefabFactory.Text(
            "Best Score", stage, "EN İYİ SKOR  0", theme.bodySize, theme.secondary,
            TextAnchor.MiddleCenter, new Vector2(.08f, .04f), new Vector2(.92f, .31f),
            Vector2.zero, Vector2.zero, FontStyle.Bold);
        return new StageViews(carousel, number, name, score);
    }

    private static void BuildPlayArea(
        Transform parent,
        UITheme theme,
        out Button playButton,
        out UIButton playButtonView,
        out Text energyCost,
        out Text energyWarning)
    {
        RectTransform area = Section("Play Area", parent, 150f);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            EndlessHuntUIPrefabFactory.PrefabPath("UI_Button"));
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, area);
        instance.name = "Start Battle";
        RectTransform rect = instance.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(.24f, .06f);
        rect.anchorMax = new Vector2(.76f, .96f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        LayoutElement layout = instance.GetComponent<LayoutElement>();
        layout.ignoreLayout = true;
        layout.minHeight = 112f;
        layout.preferredHeight = 132f;

        playButton = instance.GetComponent<Button>();
        playButtonView = instance.GetComponent<UIButton>();
        playButtonView.SetLabel("SAVAŞA BAŞLA");
        Text label = instance.transform.Find("Label").GetComponent<Text>();
        label.fontSize = theme.bodySize + 4;
        label.rectTransform.anchorMin = new Vector2(0f, .28f);
        label.rectTransform.anchorMax = Vector2.one;

        Image energyIcon = EndlessHuntUIPrefabFactory.Image(
            "Energy Icon", instance.transform,
            EndlessHuntUIPrefabFactory.Ui("Icons/64px/energy_icon_64px.png"),
            new Vector2(.35f, .04f), new Vector2(.43f, .3f),
            Vector2.zero, Vector2.zero, Color.white);
        energyIcon.preserveAspect = true;
        energyCost = EndlessHuntUIPrefabFactory.Text(
            "Energy Cost", instance.transform, "5 ENERJİ", theme.captionSize,
            theme.textPrimary, TextAnchor.MiddleLeft,
            new Vector2(.43f, .02f), new Vector2(.68f, .31f),
            Vector2.zero, Vector2.zero, FontStyle.Bold);
        energyWarning = EndlessHuntUIPrefabFactory.Text(
            "Energy Warning", area, "ENERJİ YETERSİZ", theme.captionSize,
            theme.danger, TextAnchor.MiddleCenter,
            new Vector2(.77f, .18f), new Vector2(.99f, .82f),
            Vector2.zero, Vector2.zero, FontStyle.Bold);
        energyWarning.gameObject.SetActive(false);
    }

    private static NavigationViews BuildNavigation(Transform parent, UITheme theme)
    {
        RectTransform navigation = Section("Bottom Navigation", parent, 142f);
        Image background = navigation.gameObject.AddComponent<Image>();
        background.sprite = EndlessHuntUIPrefabFactory.Ui("Containers/ButtonContainer.png");
        background.color = Color.white;
        background.raycastTarget = false;
        EndlessHuntUIPrefabFactory.ApplySliced(background);

        HorizontalLayoutGroup group = navigation.gameObject.AddComponent<HorizontalLayoutGroup>();
        group.padding = new RectOffset(8, 8, 7, 7);
        group.spacing = 5f;
        group.childAlignment = TextAnchor.MiddleCenter;
        group.childControlWidth = true;
        group.childControlHeight = true;
        group.childForceExpandWidth = false;
        group.childForceExpandHeight = true;

        UITabController controller = navigation.gameObject.AddComponent<UITabController>();
        UITabButton home = BuildTab(
            navigation, theme, "Home", HomeScreenPresenter.ScreenId, "ANA SAYFA",
            "Icons/128px/adventure_icon_128px.png");
        UITabButton equipment = BuildTab(
            navigation, theme, "Equipment", "Equipment", "EKİPMAN",
            "Icons/128px/armor_icon_128px.png");
        UITabButton talent = BuildTab(
            navigation, theme, "Talent", "Talent", "YETENEK",
            "Icons/128px/skills_icon_128px.png");
        UITabButton shop = BuildTab(
            navigation, theme, "Shop", "Shop", "MAĞAZA",
            "Icons/128px/shop_icon_128px.png");

        controller.Register(home);
        controller.Register(equipment);
        controller.Register(talent);
        controller.Register(shop);
        return new NavigationViews(controller, home, equipment, talent, shop);
    }

    private static UITabButton BuildTab(
        Transform parent,
        UITheme theme,
        string id,
        string targetScreen,
        string labelText,
        string iconPath)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            EndlessHuntUIPrefabFactory.PrefabPath("UI_TabButton"));
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        instance.name = id + " Tab";
        LayoutElement layout = instance.GetComponent<LayoutElement>();
        layout.minWidth = 210f;
        layout.preferredWidth = 220f;
        layout.flexibleWidth = 0f;
        layout.minHeight = 108f;
        layout.preferredHeight = 126f;
        Image background = instance.GetComponent<Image>();
        Button button = instance.GetComponent<Button>();
        Image selection = instance.transform.Find("Selected").GetComponent<Image>();
        selection.gameObject.SetActive(false);
        UINotificationBadge badge = instance.GetComponentInChildren<UINotificationBadge>(true);
        badge.Hide();
        Text label = instance.transform.Find("Label").GetComponent<Text>();
        label.text = labelText;
        label.fontSize = theme.captionSize - 1;
        label.rectTransform.anchorMin = new Vector2(.04f, .02f);
        label.rectTransform.anchorMax = new Vector2(.96f, .31f);
        label.rectTransform.offsetMin = Vector2.zero;
        label.rectTransform.offsetMax = Vector2.zero;
        Image icon = EndlessHuntUIPrefabFactory.Image(
            "Navigation Icon", instance.transform,
            EndlessHuntUIPrefabFactory.Ui(iconPath),
            new Vector2(.31f, .27f), new Vector2(.69f, .91f),
            Vector2.zero, Vector2.zero, Color.white);
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        Sprite inactive = EndlessHuntUIPrefabFactory.Ui("Buttons/segment_black.png");
        Sprite active = EndlessHuntUIPrefabFactory.Ui("Buttons/segment_yellow.png");
        background.sprite = inactive;
        UITabButton tab = instance.GetComponent<UITabButton>();
        tab.Configure(id, targetScreen, button, background, null, badge,
            active, inactive, theme.buttonDisabled);
        tab.ConfigureExpansion(layout, 220f, 330f);
        return tab;
    }

    private static RectTransform Section(
        string name,
        Transform parent,
        float preferredHeight,
        float flexibleHeight = 0f)
    {
        RectTransform section = EndlessHuntUIPrefabFactory.Root(
            name, new Vector2(1044f, preferredHeight), typeof(LayoutElement));
        section.SetParent(parent, false);
        LayoutElement layout = section.GetComponent<LayoutElement>();
        layout.minHeight = preferredHeight;
        layout.preferredHeight = preferredHeight;
        layout.flexibleHeight = flexibleHeight;
        return section;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int slash = path.LastIndexOf('/');
        string parent = path.Substring(0, slash);
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, path.Substring(slash + 1));
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }
}
