using UnityEngine;
using UnityEngine.Events;

namespace EndlessHunt.UI
{
    [DisallowMultipleComponent]
    public sealed class UIBackHandler : MonoBehaviour
    {
        [SerializeField] private UIScreenManager screenManager;
        [SerializeField] private UIPopupManager popupManager;
        [SerializeField] private bool battleContext;
        [SerializeField] private UnityEvent onBattleBackRequested = new();
        [SerializeField] private UnityEvent onHomeBackRequested = new();

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) HandleBack();
        }

        public void Configure(UIScreenManager screens, UIPopupManager popups)
        {
            screenManager = screens;
            popupManager = popups;
        }

        public void SetBattleContext(bool value) => battleContext = value;

        public void HandleBack()
        {
            if (popupManager != null && popupManager.TryCloseTopOnBack(Application.isPlaying)) return;
            if (battleContext)
            {
                onBattleBackRequested.Invoke();
                return;
            }
            if (screenManager != null && screenManager.GoBack()) return;
            onHomeBackRequested.Invoke();
        }
    }
}
