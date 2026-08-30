using System;
using UnityEngine;

namespace EndlessHunt.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class UIPopup : MonoBehaviour
    {
        [SerializeField] private string popupId = "Popup";
        [SerializeField] private bool closeOnBack = true;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform animatedContent;
        [SerializeField] private UIAnimationController animationController;

        public string PopupId => popupId;
        public bool CloseOnBack => closeOnBack;
        public bool IsOpen { get; private set; }
        public event Action<UIPopup> CloseRequested;

        private void Reset()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            animationController = GetComponent<UIAnimationController>();
            animatedContent = transform as RectTransform;
        }

        public void Configure(string id, RectTransform content, bool allowBack = true)
        {
            popupId = id;
            animatedContent = content;
            closeOnBack = allowBack;
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (animationController == null) animationController = GetComponent<UIAnimationController>();
        }

        public void RequestClose() => CloseRequested?.Invoke(this);

        internal void Show(bool animated)
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (animationController == null) animationController = GetComponent<UIAnimationController>();
            if (animatedContent == null) animatedContent = transform as RectTransform;
            gameObject.SetActive(true);
            IsOpen = true;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            if (animated && Application.isPlaying && animationController != null)
            {
                canvasGroup.alpha = 0f;
                animatedContent.localScale = Vector3.one * .92f;
                animationController.FadeScale(canvasGroup, animatedContent, true);
            }
            else
            {
                canvasGroup.alpha = 1f;
                animatedContent.localScale = Vector3.one;
            }
        }

        internal void Hide(bool animated)
        {
            if (!IsOpen && !gameObject.activeSelf) return;
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            IsOpen = false;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            if (animated && Application.isPlaying && animationController != null)
                animationController.FadeScale(canvasGroup, animatedContent, false, completed: Deactivate);
            else
                Deactivate();
        }

        private void Deactivate() => gameObject.SetActive(false);
    }
}
