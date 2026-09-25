using UnityEngine;
using UnityEngine.UI;

namespace EndlessHunt.UI.Common
{
    [DisallowMultipleComponent]
    public sealed class UIPopupFrame : MonoBehaviour
    {
        [SerializeField] private UIPopup popup;
        [SerializeField] private Text titleText;
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private RectTransform footerRoot;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button primaryAction;
        [SerializeField] private Button secondaryAction;

        public RectTransform ContentRoot => contentRoot;
        public RectTransform FooterRoot => footerRoot;
        public Button PrimaryAction => primaryAction;
        public Button SecondaryAction => secondaryAction;

        private void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(RequestClose);
        }

        public void Configure(UIPopup owner, Text title, RectTransform content, RectTransform footer,
            Button close, Button primary, Button secondary)
        {
            popup = owner;
            titleText = title;
            contentRoot = content;
            footerRoot = footer;
            closeButton = close;
            primaryAction = primary;
            secondaryAction = secondary;
        }

        public void SetTitle(string value)
        {
            if (titleText != null) titleText.text = value ?? string.Empty;
        }

        public void SetActionsVisible(bool primaryVisible, bool secondaryVisible)
        {
            if (primaryAction != null) primaryAction.gameObject.SetActive(primaryVisible);
            if (secondaryAction != null) secondaryAction.gameObject.SetActive(secondaryVisible);
        }

        public void RequestClose()
        {
            if (popup != null) popup.RequestClose();
        }
    }
}
