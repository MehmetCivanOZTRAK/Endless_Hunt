using UnityEngine;
using UnityEngine.UI;

namespace EndlessHunt.UI.Common
{
    [DisallowMultipleComponent]
    public sealed class UIEquipmentSlot : MonoBehaviour
    {
        [SerializeField] private string slotId;
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private Text levelText;
        [SerializeField] private GameObject emptyIndicator;
        [SerializeField] private Image selectedIndicator;

        public string SlotId => slotId;
        public Button Button => button;

        public void Configure(string id, Button targetButton, Image itemIcon, Text level,
            GameObject empty, Image selection)
        {
            slotId = id;
            button = targetButton;
            icon = itemIcon;
            levelText = level;
            emptyIndicator = empty;
            selectedIndicator = selection;
        }

        public void SetPresentation(Sprite sprite, int level, bool selected = false)
        {
            bool hasItem = sprite != null;
            if (icon != null) { icon.sprite = sprite; icon.enabled = hasItem; }
            if (levelText != null)
            {
                levelText.gameObject.SetActive(hasItem);
                levelText.text = $"LV.{Mathf.Max(1, level)}";
            }
            if (emptyIndicator != null) emptyIndicator.SetActive(!hasItem);
            if (selectedIndicator != null) selectedIndicator.enabled = selected;
        }
    }
}
