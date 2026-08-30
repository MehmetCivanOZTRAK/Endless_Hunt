using System;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessHunt.UI.Common
{
    public enum UITabVisualState { Active, Inactive, Disabled, Notification }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class UITabButton : MonoBehaviour
    {
        [SerializeField] private string tabId;
        [SerializeField] private string targetScreenId;
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private Image selectedIndicator;
        [SerializeField] private UINotificationBadge notificationBadge;
        [SerializeField] private Sprite activeSprite;
        [SerializeField] private Sprite inactiveSprite;
        [SerializeField] private Sprite disabledSprite;
        [SerializeField] private LayoutElement layoutElement;
        [SerializeField] private float inactiveWidth;
        [SerializeField] private float activeWidth;

        public string TabId => tabId;
        public string TargetScreenId => targetScreenId;
        public bool IsInteractable => button != null && button.interactable;
        public float CurrentPreferredWidth => layoutElement != null ? layoutElement.preferredWidth : 0f;
        public float InactivePreferredWidth => inactiveWidth;
        public float ActivePreferredWidth => activeWidth;
        public UITabVisualState VisualState { get; private set; }
        public event Action<UITabButton> Clicked;

        private void Awake()
        {
            if (button == null) button = GetComponent<Button>();
            button.onClick.AddListener(NotifyClicked);
            ApplyState(false);
        }

        public void ConfigureExpansion(LayoutElement targetLayout, float compactWidth, float expandedWidth)
        {
            layoutElement = targetLayout;
            inactiveWidth = Mathf.Max(0f, compactWidth);
            activeWidth = Mathf.Max(inactiveWidth, expandedWidth);
            ApplyState(VisualState == UITabVisualState.Active);
        }

        public void Configure(string id, string screenId, Button targetButton, Image targetBackground,
            Image selection, UINotificationBadge badge, Sprite active, Sprite inactive, Sprite disabled)
        {
            tabId = id;
            targetScreenId = screenId;
            button = targetButton;
            background = targetBackground;
            selectedIndicator = selection;
            notificationBadge = badge;
            activeSprite = active;
            inactiveSprite = inactive;
            disabledSprite = disabled;
            ApplyState(false);
        }

        public void SetActive(bool value) => ApplyState(value);

        public void SetInteractable(bool value)
        {
            if (button != null) button.interactable = value;
            ApplyState(VisualState == UITabVisualState.Active);
        }

        public void SetNotification(UIBadgeMode mode, int count = 0, string customText = null)
        {
            if (notificationBadge == null) return;
            notificationBadge.Show(mode, count, customText);
            if (mode != UIBadgeMode.Hidden && VisualState == UITabVisualState.Inactive)
                VisualState = UITabVisualState.Notification;
        }

        private void ApplyState(bool active)
        {
            bool interactable = button == null || button.interactable;
            VisualState = !interactable ? UITabVisualState.Disabled :
                active ? UITabVisualState.Active :
                notificationBadge != null && notificationBadge.IsVisible ? UITabVisualState.Notification :
                UITabVisualState.Inactive;
            if (selectedIndicator != null) selectedIndicator.enabled = VisualState == UITabVisualState.Active;
            if (layoutElement != null && inactiveWidth > 0f)
                layoutElement.preferredWidth = VisualState == UITabVisualState.Active ? activeWidth : inactiveWidth;
            if (background == null) return;
            Sprite sprite = VisualState switch
            {
                UITabVisualState.Active => activeSprite,
                UITabVisualState.Disabled => disabledSprite != null ? disabledSprite : inactiveSprite,
                _ => inactiveSprite
            };
            if (sprite != null) background.sprite = sprite;
            background.color = VisualState == UITabVisualState.Disabled ? new Color(.45f, .45f, .48f, 1f) : Color.white;
        }

        private void NotifyClicked() => Clicked?.Invoke(this);
    }
}
