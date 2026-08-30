using UnityEngine;
using UnityEngine.UI;

namespace EndlessHunt.UI.Common
{
    [DisallowMultipleComponent]
    public sealed class UIRewardItem : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Text title;
        [SerializeField] private Text amountText;

        public void Configure(Image rewardIcon, Text titleText, Text amount)
        {
            icon = rewardIcon;
            title = titleText;
            amountText = amount;
        }

        public void SetPresentation(Sprite sprite, string titleValue, string amount)
        {
            if (icon != null) { icon.sprite = sprite; icon.enabled = sprite != null; }
            if (title != null) title.text = titleValue ?? string.Empty;
            if (amountText != null) amountText.text = amount ?? string.Empty;
        }
    }
}
