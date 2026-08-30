using Spine.Unity;
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

        [Header("Core")]
        [SerializeField] private UIScreen screen;
        [SerializeField] private GameProfileUIAdapter profileAdapter;
        [SerializeField] private UIScreenManager screenManager;
        [SerializeField] private UITabController tabController;
        [SerializeField] private UIToast toast;

        [Header("Profile")]
        [SerializeField] private Text playerLevelText;
        [SerializeField] private UICurrencyDisplay coinDisplay;
        [SerializeField] private UICurrencyDisplay gemDisplay;
        [SerializeField] private UICurrencyDisplay energyDisplay;

        [Header("Character")]
        [SerializeField] private SkeletonGraphic characterPreview;
        [SerializeField] private WeaponDefinition[] weapons;

        [Header("Stage")]
        [SerializeField] private Text stageNumberText;
        [SerializeField] private Text stageNameText;
        [SerializeField] private Text bestScoreText;
        [SerializeField] private Button previousStageButton;
        [SerializeField] private Button nextStageButton;

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

        public UIScreen Screen => screen;
        public SkeletonGraphic CharacterPreview => characterPreview;
        public Button PlayButton => playButton;
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
            characterPreview != null &&
            stageNumberText != null &&
            stageNameText != null &&
            bestScoreText != null &&
            previousStageButton != null &&
            nextStageButton != null &&
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
            SkeletonGraphic preview,
            WeaponDefinition[] availableWeapons,
            Text stageNumber,
            Text stageName,
            Text bestScore,
            Button previousStage,
            Button nextStage,
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
            characterPreview = preview;
            weapons = availableWeapons;
            stageNumberText = stageNumber;
            stageNameText = stageName;
            bestScoreText = bestScore;
            previousStageButton = previousStage;
            nextStageButton = nextStage;
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
            UIToast feedbackToast)
        {
            profileAdapter = adapter;
            screenManager = screens;
            toast = feedbackToast;
        }

        private void Awake()
        {
            ResolveServices();
            BindListeners();
            if (screenManager != null && screen != null) screenManager.Register(screen);
            if (tabController != null) tabController.Configure(screenManager);
            BindCurrencyDisplays();
            RefreshCharacter();
            RefreshView();
        }

        private void OnEnable()
        {
            if (Application.isPlaying && _listenersBound) RefreshView();
        }

        private void OnDestroy()
        {
            if (!_listenersBound) return;
            previousStageButton.onClick.RemoveListener(PreviousStage);
            nextStageButton.onClick.RemoveListener(NextStage);
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
            RefreshStage();

            int energy = profileAdapter.GetEnergy();
            int cost = profileAdapter.GetBattleEnergyCost();
            bool canPlay = energy >= cost;
            energyCostText.text = cost + " ENERJİ";
            insufficientEnergyText.gameObject.SetActive(!canPlay);
            playButtonView.SetLabel(canPlay ? "SAVAŞA BAŞLA" : "ENERJİ BEKLENİYOR");
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
            if (screen == null) screen = GetComponent<UIScreen>();
        }

        private void BindListeners()
        {
            if (_listenersBound || !IsConfigured) return;
            previousStageButton.onClick.AddListener(PreviousStage);
            nextStageButton.onClick.AddListener(NextStage);
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

        private void RefreshCharacter()
        {
            CharacterSkin.Apply(characterPreview, weapons);
            SkeletonAnimation animation = characterPreview != null ? characterPreview.Animation as SkeletonAnimation : null;
            if (animation != null && animation.AnimationState != null)
                animation.AnimationState.SetAnimation(0, "Idle", true);
        }

        private void RefreshStage()
        {
            int selected = Mathf.Clamp(profileAdapter.GetSelectedStage(), 0, GameCatalog.MapNames.Length - 1);
            stageNumberText.text = "BÖLÜM " + (selected + 1);
            stageNameText.text = profileAdapter.GetStageName(selected).ToUpperInvariant();
            bestScoreText.text = FormatBestScore(profileAdapter.GetBestScore());

            int unlocked = Mathf.Clamp(profileAdapter.GetHighestStage(), 1, GameCatalog.MapNames.Length);
            previousStageButton.interactable = unlocked > 1;
            nextStageButton.interactable = unlocked > 1;
        }

        private void PreviousStage() => CycleStage(-1);
        private void NextStage() => CycleStage(1);

        private void CycleStage(int delta)
        {
            int count = Mathf.Clamp(profileAdapter.GetHighestStage(), 1, GameCatalog.MapNames.Length);
            int next = (profileAdapter.GetSelectedStage() + delta + count) % count;
            profileAdapter.SetSelectedStage(next);
            RefreshStage();
        }

        private void StartGame()
        {
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
