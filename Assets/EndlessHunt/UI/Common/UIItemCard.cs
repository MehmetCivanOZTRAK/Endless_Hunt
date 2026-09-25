using UnityEngine;
using UnityEngine.UI;

namespace EndlessHunt.UI.Common
{
    [DisallowMultipleComponent]
    public sealed class UIItemCard : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private Text title;
        [SerializeField] private Text subtitle;
        [SerializeField] private Image selectedIndicator;
        [SerializeField] private GameObject disabledOverlay;

        public Button Button => button;

        public void Configure(Button targetButton, Image itemIcon, Text titleText, Text subtitleText,
            Image selection, GameObject disabled)
        {
            button = targetButton;
            icon = itemIcon;
            title = titleText;
            subtitle = subtitleText;
            selectedIndicator = selection;
            disabledOverlay = disabled;
        }

        public void SetPresentation(Sprite sprite, string titleValue, string subtitleValue,
            bool selected = false, bool interactable = true)
        {
            if (icon != null) { icon.sprite = sprite; icon.enabled = sprite != null; }
            if (title != null) title.text = titleValue ?? string.Empty;
            if (subtitle != null) subtitle.text = subtitleValue ?? string.Empty;
            if (selectedIndicator != null) selectedIndicator.enabled = selected;
            if (button != null) button.interactable = interactable;
            if (disabledOverlay != null) disabledOverlay.SetActive(!interactable);
        }
    }
}
