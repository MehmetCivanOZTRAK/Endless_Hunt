using UnityEngine;
using UnityEngine.EventSystems;

namespace EndlessHunt.UI
{
    [DisallowMultipleComponent]
    public sealed class UIRoot : MonoBehaviour
    {
        [SerializeField] private UITheme theme;
        [SerializeField] private SafeAreaController safeArea;
        [SerializeField] private Transform screenLayer;
        [SerializeField] private Transform popupLayer;
        [SerializeField] private Transform overlayLayer;
        [SerializeField] private Transform toastLayer;
        [SerializeField] private UIScreenManager screenManager;
        [SerializeField] private UIPopupManager popupManager;
        [SerializeField] private UIBackHandler backHandler;
        [SerializeField] private GameProfileUIAdapter profileAdapter;
        [SerializeField] private bool ensureEventSystem = true;

        public UITheme Theme => theme;
        public SafeAreaController SafeArea => safeArea;
        public Transform ScreenLayer => screenLayer;
        public Transform PopupLayer => popupLayer;
        public Transform OverlayLayer => overlayLayer;
        public Transform ToastLayer => toastLayer;
        public UIScreenManager ScreenManager => screenManager;
        public UIPopupManager PopupManager => popupManager;
        public GameProfileUIAdapter ProfileAdapter => profileAdapter;

        private void Awake()
        {
            if (ensureEventSystem && EventSystem.current == null)
                Debug.LogError("Assign an EventSystem in the UI scene before playing.", this);
        }

        public void Configure(UITheme uiTheme, SafeAreaController safeAreaController, Transform screens,
            Transform popups, Transform overlays, Transform toasts, UIScreenManager screensManager,
            UIPopupManager popupsManager, UIBackHandler uiBackHandler, GameProfileUIAdapter adapter)
        {
            theme = uiTheme;
            safeArea = safeAreaController;
            screenLayer = screens;
            popupLayer = popups;
            overlayLayer = overlays;
            toastLayer = toasts;
            screenManager = screensManager;
            popupManager = popupsManager;
            backHandler = uiBackHandler;
            profileAdapter = adapter;
        }
    }
}
