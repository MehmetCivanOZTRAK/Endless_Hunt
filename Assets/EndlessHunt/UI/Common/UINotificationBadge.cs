using UnityEngine;
using UnityEngine.UI;

namespace EndlessHunt.UI.Common
{
    public enum UIBadgeMode { Hidden, Exclamation, New, Claim, Upgrade, Count, Custom }

    [DisallowMultipleComponent]
    public sealed class UINotificationBadge : MonoBehaviour
    {
        [SerializeField] private GameObject displayRoot;
        [SerializeField] private Text label;

        public UIBadgeMode Mode { get; private set; }
        public bool IsVisible => displayRoot != null && displayRoot.activeSelf;

        public void Configure(GameObject root, Text badgeLabel)
        {
            displayRoot = root;
            label = badgeLabel;
            Hide();
        }

        public void Show(UIBadgeMode mode, int count = 0, string customText = null)
        {
            Mode = mode;
            if (mode == UIBadgeMode.Hidden) { Hide(); return; }
            if (displayRoot != null) displayRoot.SetActive(true);
            if (label == null) return;
            label.text = mode switch
            {
                UIBadgeMode.Exclamation => "!",
                UIBadgeMode.New => "NEW",
                UIBadgeMode.Claim => "CLAIM",
                UIBadgeMode.Upgrade => "UPGRADE",
                UIBadgeMode.Count => Mathf.Max(0, count).ToString(),
                UIBadgeMode.Custom => customText ?? string.Empty,
                _ => string.Empty
            };
        }

        public void SetCount(int count)
        {
            if (count <= 0) Hide();
            else Show(UIBadgeMode.Count, count);
        }

        public void Hide()
        {
            Mode = UIBadgeMode.Hidden;
            if (displayRoot != null) displayRoot.SetActive(false);
        }
    }
}
