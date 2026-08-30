using System;
using System.Linq;
using EndlessHunt;
using EndlessHunt.UI;
using EndlessHunt.UI.Common;
using EndlessHunt.UI.Screens;
using Spine.Unity;
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
    private const string StartPrefabPath = "Assets/CharacterPrefabs/Character_20260827_211359.prefab";
    private const string MainMenuToastName = "Main Menu Toast";

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
        public readonly Text Number;
        public readonly Text Name;
        public readonly Text Score;
        public readonly Button Previous;
        public readonly Button Next;

        public StageViews(Text number, Text name, Text score, Button previous, Button next)
        {
            Number = number;
            Name = name;
            Score = score;
            Previous = previous;
            Next = next;
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
        GameObject startPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(StartPrefabPath);
        WeaponDefinition[] weapons = LoadWeapons();
        if (theme == null) throw new MissingReferenceException("EndlessHuntUITheme.asset is missing.");
        if (startPrefab == null) throw new MissingReferenceException("Main Menu character source prefab is missing.");
        if (weapons.Length == 0) throw new MissingReferenceException("Weapon definitions are missing.");

        BuildHomePrefab(theme, startPrefab, weapons);
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
        if (uiRootPrefab == null || homePrefab == null || toastPrefab == null)
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

        presenter.BindServices(uiRoot.ProfileAdapter, uiRoot.ScreenManager, toast);
        uiRoot.ScreenManager.Configure(uiRoot.ScreenLayer, presenter.Screen);
        uiRoot.PopupManager.Configure(uiRoot.PopupLayer);
        PrefabUtility.RecordPrefabInstancePropertyModifications(presenter);
        PrefabUtility.RecordPrefabInstancePropertyModifications(uiRoot.ScreenManager);
        PrefabUtility.RecordPrefabInstancePropertyModifications(uiRoot.PopupManager);

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

    private static void BuildHomePrefab(UITheme theme, GameObject startPrefab, WeaponDefinition[] weapons)
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
        vertical.padding = new RectOffset(18, 18, 16, 16);
        vertical.spacing = theme.smallSpacing;
        vertical.childAlignment = TextAnchor.UpperCenter;
        vertical.childControlWidth = true;
        vertical.childControlHeight = true;
        vertical.childForceExpandWidth = true;
        vertical.childForceExpandHeight = false;

        Text levelText;
        CurrencyViews currencies = BuildTopBar(content, theme, out levelText);
        Button settingsButton;
        SkeletonGraphic preview = BuildHero(content, theme, startPrefab, out settingsButton);
        StageViews stage = BuildStagePanel(content, theme);
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
            preview,
            weapons,
            stage.Number,
            stage.Name,
            stage.Score,
            stage.Previous,
            stage.Next,
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
        RectTransform top = Section("Top Bar", parent, 176f);
        Image topBackground = top.gameObject.AddComponent<Image>();
        topBackground.sprite = EndlessHuntUIPrefabFactory.Ui("Modals/topBarEmpty.png");
        topBackground.color = Color.white;
        topBackground.raycastTarget = false;

        HorizontalLayoutGroup row = top.gameObject.AddComponent<HorizontalLayoutGroup>();
        row.padding = new RectOffset(12, 12, 10, 10);
        row.spacing = 10f;
        row.childAlignment = TextAnchor.MiddleCenter;
        row.childControlWidth = true;
        row.childControlHeight = true;
        row.childForceExpandWidth = false;
        row.childForceExpandHeight = true;

        RectTransform profile = EndlessHuntUIPrefabFactory.Root(
            "Player Level", new Vector2(236f, 150f), typeof(Image), typeof(LayoutElement));
        profile.SetParent(top, false);
        Image profileBackground = profile.GetComponent<Image>();
        profileBackground.sprite = EndlessHuntUIPrefabFactory.Ui("Cards/cardShop_yellow.png");
        profileBackground.raycastTarget = false;
        EndlessHuntUIPrefabFactory.ApplySliced(profileBackground);
        LayoutElement profileLayout = profile.GetComponent<LayoutElement>();
        profileLayout.minWidth = 220f;
        profileLayout.preferredWidth = 236f;

        Image avatar = EndlessHuntUIPrefabFactory.Image(
            "Avatar", profile,
            EndlessHuntUIPrefabFactory.Ui("Icons/128px/adventure_icon_128px.png"),
            new Vector2(.03f, .12f), new Vector2(.39f, .88f),
            Vector2.zero, Vector2.zero, Color.white);
        avatar.preserveAspect = true;
        EndlessHuntUIPrefabFactory.Text(
            "Profile Label", profile, "KAHRAMAN", theme.captionSize, theme.textPrimary,
            TextAnchor.MiddleLeft, new Vector2(.4f, .5f), new Vector2(.98f, .86f),
            Vector2.zero, Vector2.zero, FontStyle.Bold);
        levelText = EndlessHuntUIPrefabFactory.Text(
            "Level", profile, "LV. 1", theme.bodySize, theme.accent,
            TextAnchor.MiddleLeft, new Vector2(.4f, .14f), new Vector2(.98f, .54f),
            Vector2.zero, Vector2.zero, FontStyle.Bold);

        UICurrencyDisplay coins = BuildCurrency(top, theme, UICurrencyType.Coins,
            "Coins", "Labels/coin_label.png", "Icons/128px/coin_icon_128px.png");
        UICurrencyDisplay gems = BuildCurrency(top, theme, UICurrencyType.Gems,
            "Gems", "Labels/gem_label.png", "Icons/128px/gem_icon_128px.png");
        UICurrencyDisplay energy = BuildCurrency(top, theme, UICurrencyType.Energy,
            "Energy", "Labels/energy_label.png", "Icons/128px/energy_icon_128px.png");
        return new CurrencyViews(coins, gems, energy);
    }

    private static UICurrencyDisplay BuildCurrency(
        Transform parent, UITheme theme, UICurrencyType type, string name,
        string backgroundPath, string iconPath)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            EndlessHuntUIPrefabFactory.PrefabPath("UI_CurrencyDisplay"));
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        instance.name = name;

        LayoutElement layout = instance.GetComponent<LayoutElement>();
        layout.minWidth = 230f;
        layout.preferredWidth = 252f;
        layout.flexibleWidth = 1f;
        Image background = instance.GetComponent<Image>();
        background.sprite = EndlessHuntUIPrefabFactory.Ui(backgroundPath);
        background.color = Color.white;

        Image icon = instance.transform.Find("Icon").GetComponent<Image>();
        icon.sprite = EndlessHuntUIPrefabFactory.Ui(iconPath);
        icon.GetComponent<LayoutElement>().preferredWidth = 50f;
        Text value = instance.transform.Find("Value").GetComponent<Text>();
        value.fontSize = theme.bodySize;
        value.GetComponent<LayoutElement>().preferredWidth =
            type == UICurrencyType.Energy ? 82f : 100f;
        Text timer = instance.transform.Find("Timer").GetComponent<Text>();
        timer.fontSize = theme.captionSize - 2;
        timer.GetComponent<LayoutElement>().preferredWidth = 58f;
        timer.gameObject.SetActive(type == UICurrencyType.Energy);

        UICurrencyDisplay display = instance.GetComponent<UICurrencyDisplay>();
        display.Configure(type, null, icon, value, timer);
        return display;
    }

    private static SkeletonGraphic BuildHero(
        Transform parent, UITheme theme, GameObject startPrefab, out Button settingsButton)
    {
        RectTransform hero = Section("Character Area", parent, 760f, 1f);
        Image card = hero.gameObject.AddComponent<Image>();
        card.sprite = EndlessHuntUIPrefabFactory.Ui("Containers/cardContainer_black.png");
        card.color = Color.white;
        card.raycastTarget = false;
        EndlessHuntUIPrefabFactory.ApplySliced(card);

        Image map = EndlessHuntUIPrefabFactory.Image(
            "Current Stage Backdrop", hero,
            EndlessHuntSceneFactory.Sprite(
                "Assets/Layer Lab/2D Art Maker/AMCasual Character/Demo/Map/background.png"),
            new Vector2(.025f, .025f), new Vector2(.975f, .975f),
            Vector2.zero, Vector2.zero, new Color(.7f, .86f, .74f, 1f));
        map.type = Image.Type.Simple;
        map.raycastTarget = false;

        Image atmosphere = EndlessHuntUIPrefabFactory.Image(
            "Atmosphere", hero,
            EndlessHuntUIPrefabFactory.Ui("Effects/effect_yellow.png"),
            new Vector2(.18f, .02f), new Vector2(.82f, .68f),
            Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, .34f));
        atmosphere.raycastTarget = false;

        Image title = EndlessHuntUIPrefabFactory.Image(
            "Game Title", hero,
            EndlessHuntUIPrefabFactory.Ui("Headers/header_blue.png"),
            new Vector2(.22f, .84f), new Vector2(.78f, .985f),
            Vector2.zero, Vector2.zero, Color.white);
        EndlessHuntUIPrefabFactory.Text(
            "Title", title.transform, "ENDLESS HUNT", theme.titleSize, theme.textPrimary,
            TextAnchor.MiddleCenter, Vector2.zero, Vector2.one,
            new Vector2(30f, 8f), new Vector2(-30f, -8f), FontStyle.Bold);

        SkeletonGraphic preview = EndlessHuntSceneFactory.SpineGraphic(
            startPrefab, "Equipped Character Preview", hero,
            new Vector2(.24f, .04f), new Vector2(.76f, .88f),
            Vector2.zero, Vector2.zero, 2.25f);

        settingsButton = EndlessHuntSceneFactory.Button(
            "Settings", hero,
            EndlessHuntUIPrefabFactory.Ui("Buttons/buttonSquare_blue.png"),
            string.Empty, theme.captionSize,
            new Vector2(.865f, .855f), new Vector2(.97f, .975f),
            Vector2.zero, Vector2.zero);
        Image settingsIcon = EndlessHuntUIPrefabFactory.Image(
            "Icon", settingsButton.transform,
            EndlessHuntUIPrefabFactory.Ui("Icons/128px/settings_icon_128px.png"),
            new Vector2(.16f, .16f), new Vector2(.84f, .84f),
            Vector2.zero, Vector2.zero, Color.white);
        settingsIcon.preserveAspect = true;
        return preview;
    }

    private static StageViews BuildStagePanel(Transform parent, UITheme theme)
    {
        RectTransform stage = Section("Stage Area", parent, 188f);
        Image background = stage.gameObject.AddComponent<Image>();
        background.sprite = EndlessHuntUIPrefabFactory.Ui("Modals/modalSimple_v2.png");
        background.color = Color.white;
        background.raycastTarget = false;
        EndlessHuntUIPrefabFactory.ApplySliced(background);

        Button previous = EndlessHuntSceneFactory.Button(
            "Previous Stage", stage,
            EndlessHuntUIPrefabFactory.Ui("Buttons/buttonSquare_blue.png"), "‹", 48,
            new Vector2(.025f, .16f), new Vector2(.14f, .84f),
            Vector2.zero, Vector2.zero);
        Button next = EndlessHuntSceneFactory.Button(
            "Next Stage", stage,
            EndlessHuntUIPrefabFactory.Ui("Buttons/buttonSquare_blue.png"), "›", 48,
            new Vector2(.86f, .16f), new Vector2(.975f, .84f),
            Vector2.zero, Vector2.zero);
        Text number = EndlessHuntUIPrefabFactory.Text(
            "Stage Number", stage, "BÖLÜM 1", theme.captionSize, theme.accent,
            TextAnchor.MiddleCenter, new Vector2(.15f, .67f), new Vector2(.85f, .94f),
            Vector2.zero, Vector2.zero, FontStyle.Bold);
        Text name = EndlessHuntUIPrefabFactory.Text(
            "Stage Name", stage, "YEŞİL KAMP", theme.titleSize - 4,
            new Color(.12f, .09f, .16f, 1f), TextAnchor.MiddleCenter,
            new Vector2(.15f, .27f), new Vector2(.85f, .7f),
            Vector2.zero, Vector2.zero, FontStyle.Bold);
        Text score = EndlessHuntUIPrefabFactory.Text(
            "Best Score", stage, "EN İYİ SKOR  0", theme.bodySize, theme.secondary,
            TextAnchor.MiddleCenter, new Vector2(.15f, .05f), new Vector2(.85f, .3f),
            Vector2.zero, Vector2.zero, FontStyle.Bold);
        return new StageViews(number, name, score, previous, next);
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
        RectTransform navigation = Section("Bottom Navigation", parent, 158f);
        Image background = navigation.gameObject.AddComponent<Image>();
        background.sprite = EndlessHuntUIPrefabFactory.Ui("Containers/ButtonContainer.png");
        background.color = Color.white;
        background.raycastTarget = false;
        EndlessHuntUIPrefabFactory.ApplySliced(background);

        HorizontalLayoutGroup group = navigation.gameObject.AddComponent<HorizontalLayoutGroup>();
        group.padding = new RectOffset(10, 10, 8, 8);
        group.spacing = 7f;
        group.childAlignment = TextAnchor.MiddleCenter;
        group.childControlWidth = true;
        group.childControlHeight = true;
        group.childForceExpandWidth = true;
        group.childForceExpandHeight = true;

        UITabController controller = navigation.gameObject.AddComponent<UITabController>();
        UITabButton home = BuildTab(
            navigation, theme, "Home", HomeScreenPresenter.ScreenId, "ANA SAYFA",
            "Buttons/adventure_1ButtonDefault.png", "Buttons/adventureButtonPressed.png");
        UITabButton equipment = BuildTab(
            navigation, theme, "Equipment", "Equipment", "EKİPMAN",
            "Buttons/equipment_1ButtonDefault.png", "Buttons/equipmentButtonPressed.png");
        UITabButton talent = BuildTab(
            navigation, theme, "Talent", "Talent", "YETENEK",
            "Buttons/skills_1ButtonDefault.png", "Buttons/skillsButtonPressed.png");
        UITabButton shop = BuildTab(
            navigation, theme, "Shop", "Shop", "MAĞAZA",
            "Buttons/shop_1ButtonDefault.png", "Buttons/shopButtonPressed.png");

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
        string inactivePath,
        string activePath)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            EndlessHuntUIPrefabFactory.PrefabPath("UI_TabButton"));
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        instance.name = id + " Tab";
        LayoutElement layout = instance.GetComponent<LayoutElement>();
        layout.minWidth = 150f;
        layout.preferredWidth = 250f;
        layout.flexibleWidth = 1f;
        layout.minHeight = 112f;
        layout.preferredHeight = 138f;
        Image background = instance.GetComponent<Image>();
        Button button = instance.GetComponent<Button>();
        Image selection = instance.transform.Find("Selected").GetComponent<Image>();
        UINotificationBadge badge = instance.GetComponentInChildren<UINotificationBadge>(true);
        badge.Hide();
        Text label = instance.transform.Find("Label").GetComponent<Text>();
        label.text = labelText;
        label.fontSize = theme.captionSize;
        Sprite inactive = EndlessHuntUIPrefabFactory.Ui(inactivePath);
        Sprite active = EndlessHuntUIPrefabFactory.Ui(activePath);
        background.sprite = inactive;
        UITabButton tab = instance.GetComponent<UITabButton>();
        tab.Configure(id, targetScreen, button, background, selection, badge,
            active, inactive, theme.buttonDisabled);
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

    private static WeaponDefinition[] LoadWeapons()
    {
        return AssetDatabase.FindAssets("t:WeaponDefinition", new[] { "Assets/EndlessHunt/Data/Weapons" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<WeaponDefinition>)
            .Where(item => item != null)
            .OrderBy(item => item.id, StringComparer.Ordinal)
            .ToArray();
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
