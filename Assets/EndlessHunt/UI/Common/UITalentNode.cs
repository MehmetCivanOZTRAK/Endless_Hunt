using UnityEngine;
using UnityEngine.UI;

namespace EndlessHunt.UI.Common
{
    [DisallowMultipleComponent]
    public sealed class UITalentNode : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private Text title;
        [SerializeField] private Text levelText;
        [SerializeField] private Text costText;
        [SerializeField] private GameObject lockedOverlay;

        public Button Button => button;

        public void Configure(Button targetButton, Image nodeIcon, Text titleText, Text level,
            Text cost, GameObject locked)
        {
            button = targetButton;
            icon = nodeIcon;
            title = titleText;
            levelText = level;
            costText = cost;
            lockedOverlay = locked;
        }

        public void SetPresentation(Sprite sprite, string titleValue, int level, string cost,
            bool locked, bool canUpgrade)
        {
            if (icon != null) { icon.sprite = sprite; icon.enabled = sprite != null; }
            if (title != null) title.text = titleValue ?? string.Empty;
            if (levelText != null) levelText.text = $"LV.{Mathf.Max(0, level)}";
            if (costText != null) costText.text = cost ?? string.Empty;
            if (lockedOverlay != null) lockedOverlay.SetActive(locked);
            if (button != null) button.interactable = !locked && canUpgrade;
        }
    }
}
