using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using EndlessHunt.UI.Common;

namespace EndlessHunt.UI.Screens
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIScreen))]
    public sealed class HomeScreenPresenter : MonoBehaviour
    {
        public const string ScreenId = "Home";
        public const string GameplaySceneName = "GameScene";
        public const string SettingsPopupId = "Settings";

        [Header("Core")]
        [SerializeField] private UIScreen screen;
        [SerializeField] private GameProfileUIAdapter profileAdapter;
        [SerializeField] private UIScreenManager screenManager;
        [SerializeField] private UIPopupManager popupManager;
        [SerializeField] private UITabController tabController;
        [SerializeField] private UIToast toast;

        [Header("Profile")]
        [SerializeField] private Text playerLevelText;
        [SerializeField] private UICurrencyDisplay coinDisplay;
        [SerializeField] private UICurrencyDisplay gemDisplay;
        [SerializeField] private UICurrencyDisplay energyDisplay;

        [Header("Stage")]
        [SerializeField] private UIStageCarousel stageCarousel;
        [SerializeField] private Text stageNumberText;
        [SerializeField] private Text stageNameText;
        [SerializeField] private Text bestScoreText;

        [Header("Primary Action")]
        [SerializeField] private Button playButton;
        [SerializeField] private UIButton playButtonView;
        [SerializeField] private Text energyCostText;
        [SerializeField] private Text insufficientEnergyText;
        [SerializeField] private Button settingsButton;

        [Header("Prepared Navigation")]
        [SerializeField] private UITabButton homeTab;
        [SerializeField] private UITabButton equipmentTab;
        [SerializeField] private UITabButton talentTab;
        [SerializeField] private UITabButton shopTab;

        private bool _listenersBound;
        private bool _carouselInitialized;
        private int _displayedStageIndex;

        public UIScreen Screen => screen;
        public UIStageCarousel StageCarousel => stageCarousel;
        public Text StageNumberView => stageNumberText;
        public Text StageNameView => stageNameText;
        public Text BestScoreView => bestScoreText;
        public Button PlayButton => playButton;
        public Button SettingsButton => settingsButton;
        public UICurrencyDisplay CoinDisplay => coinDisplay;
        public UICurrencyDisplay GemDisplay => gemDisplay;
        public UICurrencyDisplay EnergyDisplay => energyDisplay;
        public UITabButton[] BottomTabs => new[] { homeTab, equipmentTab, talentTab, shopTab };
        public bool IsConfigured =>
            screen != null &&
            playerLevelText != null &&
            coinDisplay != null &&
            gemDisplay != null &&
            energyDisplay != null &&
            stageCarousel != null &&
            stageNumberText != null &&
            stageNameText != null &&
            bestScoreText != null &&
            playButton != null &&
            playButtonView != null &&
            energyCostText != null &&
            insufficientEnergyText != null &&
            settingsButton != null &&
            tabController != null &&
            homeTab != null &&
            equipmentTab != null &&
            talentTab != null &&
            shopTab != null;

        public void Configure(
            UIScreen targetScreen,
            Text levelText,
            UICurrencyDisplay coins,
            UICurrencyDisplay gems,
            UICurrencyDisplay energy,
            UIStageCarousel carousel,
            Text stageNumber,
            Text stageName,
            Text bestScore,
            Button startButton,
            UIButton startButtonView,
            Text energyCost,
            Text energyWarning,
            Button settings,
            UITabController tabs,
            UITabButton home,
            UITabButton equipment,
            UITabButton talent,
            UITabButton shop)
        {
            screen = targetScreen;
            playerLevelText = levelText;
            coinDisplay = coins;
            gemDisplay = gems;
            energyDisplay = energy;
            stageCarousel = carousel;
            stageNumberText = stageNumber;
            stageNameText = stageName;
            bestScoreText = bestScore;
            playButton = startButton;
            playButtonView = startButtonView;
            energyCostText = energyCost;
            insufficientEnergyText = energyWarning;
            settingsButton = settings;
            tabController = tabs;
            homeTab = home;
            equipmentTab = equipment;
            talentTab = talent;
            shopTab = shop;
        }

        public void BindServices(
            GameProfileUIAdapter adapter,
            UIScreenManager screens,
            UIPopupManager popups,
            UIToast feedbackToast)
        {
            profileAdapter = adapter;
            screenManager = screens;
            popupManager = popups;
            toast = feedbackToast;
        }

        private void Awake()
        {
            ResolveServices();
            BindListeners();
            if (screenManager != null && screen != null) screenManager.Register(screen);
            if (tabController != null) tabController.Configure(screenManager);
            BindCurrencyDisplays();
            InitializeCarousel();
            RefreshView();
        }

        private void OnEnable()
        {
            if (Application.isPlaying && _listenersBound) RefreshView();
        }

        private void OnDestroy()
        {
            if (!_listenersBound) return;
            stageCarousel.SelectionChanged -= HandleStageSelectionChanged;
            playButton.onClick.RemoveListener(StartGame);
            settingsButton.onClick.RemoveListener(ShowSettingsFeedback);
            equipmentTab.Clicked -= ShowPreparedScreenFeedback;
            talentTab.Clicked -= ShowPreparedScreenFeedback;
            shopTab.Clicked -= ShowPreparedScreenFeedback;
        }

        public void RefreshView()
        {
            if (profileAdapter == null) return;
            profileAdapter.Refresh(true);
            playerLevelText.text = "LV. " + profileAdapter.GetPlayerLevel();
            int unlocked = Mathf.Clamp(profileAdapter.GetHighestStage(), 1, GameCatalog.MapNames.Length);
            stageCarousel.SetUnlockedCount(unlocked);
            RefreshStage();

            int energy = profileAdapter.GetEnergy();
            int cost = profileAdapter.GetBattleEnergyCost();
            bool stageUnlocked = _displayedStageIndex < unlocked;
            bool canPlay = stageUnlocked && energy >= cost;
            energyCostText.text = cost + " ENERJİ";
            insufficientEnergyText.text = stageUnlocked ? "ENERJİ YETERSİZ" : "BÖLÜM KİLİTLİ";
            insufficientEnergyText.gameObject.SetActive(!canPlay);
            playButtonView.SetLabel(stageUnlocked
                ? canPlay ? "SAVAŞA BAŞLA" : "ENERJİ BEKLENİYOR"
                : "BÖLÜM KİLİTLİ");
            playButtonView.SetInteractable(canPlay);
        }

        public static string FormatBestScore(int score)
        {
            return "EN İYİ SKOR  " + Mathf.Max(0, score).ToString("N0");
        }

        private void ResolveServices()
        {
            if (profileAdapter == null) profileAdapter = GetComponentInParent<GameProfileUIAdapter>();
            if (screenManager == null) screenManager = GetComponentInParent<UIScreenManager>();
            if (popupManager == null) popupManager = GetComponentInParent<UIPopupManager>();
            if (screen == null) screen = GetComponent<UIScreen>();
        }

        private void BindListeners()
        {
            if (_listenersBound || !IsConfigured) return;
            stageCarousel.SelectionChanged += HandleStageSelectionChanged;
            playButton.onClick.AddListener(StartGame);
            settingsButton.onClick.AddListener(ShowSettingsFeedback);
            equipmentTab.Clicked += ShowPreparedScreenFeedback;
            talentTab.Clicked += ShowPreparedScreenFeedback;
            shopTab.Clicked += ShowPreparedScreenFeedback;
            _listenersBound = true;
        }

        private void BindCurrencyDisplays()
        {
            coinDisplay?.Bind(profileAdapter);
            gemDisplay?.Bind(profileAdapter);
            energyDisplay?.Bind(profileAdapter);
        }

        private void InitializeCarousel()
        {
            if (_carouselInitialized || stageCarousel == null || profileAdapter == null) return;
            _displayedStageIndex = Mathf.Clamp(
                profileAdapter.GetSelectedStage(), 0, GameCatalog.MapNames.Length - 1);
            int unlocked = Mathf.Clamp(profileAdapter.GetHighestStage(), 1, GameCatalog.MapNames.Length);
            stageCarousel.Initialize(_displayedStageIndex, unlocked);
            _carouselInitialized = true;
        }

        private void RefreshStage()
        {
            int selected = Mathf.Clamp(_displayedStageIndex, 0, GameCatalog.MapNames.Length - 1);
            stageNumberText.text = "BÖLÜM " + (selected + 1);
            stageNameText.text = profileAdapter.GetStageName(selected).ToUpperInvariant();
            bestScoreText.text = FormatBestScore(profileAdapter.GetBestScore());
        }

        private void HandleStageSelectionChanged(int index)
        {
            _displayedStageIndex = Mathf.Clamp(index, 0, GameCatalog.MapNames.Length - 1);
            if (_displayedStageIndex < profileAdapter.GetHighestStage())
                profileAdapter.SetSelectedStage(_displayedStageIndex);
            RefreshView();
        }

        private void StartGame()
        {
            if (_displayedStageIndex >= profileAdapter.GetHighestStage())
            {
                ShowToast("BÖLÜM HENÜZ KİLİTLİ");
                RefreshView();
                return;
            }

            profileAdapter.SetSelectedStage(_displayedStageIndex);
            if (!profileAdapter.TrySpendBattleEnergy())
            {
                ShowToast("YETERLİ ENERJİ YOK");
                RefreshView();
                return;
            }

            profileAdapter.Refresh(true);
            SceneManager.LoadScene(GameplaySceneName);
        }

        private void ShowSettingsFeedback()
        {
            if (popupManager != null && popupManager.OpenPopup(SettingsPopupId)) return;
            ShowToast("AYARLAR BAĞLANTISI HAZIR");
        }

        private void ShowPreparedScreenFeedback(UITabButton tab)
        {
            if (tabController != null) tabController.SelectTab("Home");
            string label = tab != null ? tab.TabId.ToUpperInvariant() : "EKRAN";
            ShowToast(label + " SONRAKİ AŞAMADA");
        }

        private void ShowToast(string message)
        {
            if (toast != null) toast.Show(message);
        }
    }
}
