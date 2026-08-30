using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EndlessHunt.UI.Common
{
    public enum UIButtonVisualState { Normal, Pressed, Disabled, Selected }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class UIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private UITheme theme;
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private Image selectedIndicator;
        [SerializeField] private Text label;
        [SerializeField] private UIAnimationController animationController;
        [SerializeField] private bool selected;

        public UIButtonVisualState VisualState { get; private set; }
        public bool IsInteractable => button != null && button.interactable;

        private void Reset()
        {
            button = GetComponent<Button>();
            background = GetComponent<Image>();
            animationController = GetComponent<UIAnimationController>();
        }

        private void OnEnable() => RefreshVisual();

        public void Configure(UITheme uiTheme, Button targetButton, Image targetBackground, Text targetLabel,
            Image selection = null)
        {
            theme = uiTheme;
            button = targetButton;
            background = targetBackground;
            label = targetLabel;
            selectedIndicator = selection;
            if (animationController == null) animationController = GetComponent<UIAnimationController>();
            RefreshVisual();
        }

        public void SetLabel(string value)
        {
            if (label != null) label.text = value;
        }

        public void SetSelected(bool value)
        {
            selected = value;
            RefreshVisual();
        }

        public void SetInteractable(bool value)
        {
            if (button != null) button.interactable = value;
            RefreshVisual();
        }

        public void RefreshVisual()
        {
            ApplyState(!IsInteractable ? UIButtonVisualState.Disabled :
                selected ? UIButtonVisualState.Selected : UIButtonVisualState.Normal);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!IsInteractable) return;
            ApplyState(UIButtonVisualState.Pressed);
            if (animationController != null) animationController.ButtonPress(transform as RectTransform);
        }

        public void OnPointerUp(PointerEventData eventData) => RefreshVisual();
        public void OnPointerExit(PointerEventData eventData) => RefreshVisual();

        private void ApplyState(UIButtonVisualState state)
        {
            VisualState = state;
            if (selectedIndicator != null) selectedIndicator.enabled = state == UIButtonVisualState.Selected;
            if (background == null || theme == null) return;
            Sprite sprite = state switch
            {
                UIButtonVisualState.Pressed => theme.buttonPressed,
                UIButtonVisualState.Disabled => theme.buttonDisabled,
                UIButtonVisualState.Selected => theme.buttonSelected,
                _ => theme.buttonNormal
            };
            if (sprite != null) background.sprite = sprite;
            background.color = state == UIButtonVisualState.Disabled ? theme.disabled : Color.white;
        }
    }
}
