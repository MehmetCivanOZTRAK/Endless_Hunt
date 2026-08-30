using System;
using UnityEngine;

namespace EndlessHunt.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class UIScreen : MonoBehaviour
    {
        [SerializeField] private string screenId;
        [SerializeField] private bool keepActiveWhenHidden;
        [SerializeField] private CanvasGroup canvasGroup;

        public string ScreenId => screenId;
        public bool IsVisible { get; private set; }
        public event Action<UIScreen, bool> VisibilityChanged;

        private void Reset() => canvasGroup = GetComponent<CanvasGroup>();

        public void Configure(string id, bool keepActive = false)
        {
            screenId = id;
            keepActiveWhenHidden = keepActive;
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        }

        internal void SetVisible(bool visible)
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (visible && !gameObject.activeSelf) gameObject.SetActive(true);
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
            IsVisible = visible;
            if (!visible && !keepActiveWhenHidden) gameObject.SetActive(false);
            VisibilityChanged?.Invoke(this, visible);
        }
    }
}
