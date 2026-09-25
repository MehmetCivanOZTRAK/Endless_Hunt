using UnityEngine;
using UnityEngine.UI;

namespace EndlessHunt.UI.Screens
{
    [DisallowMultipleComponent]
    public sealed class UIStageCardView : MonoBehaviour
    {
        [SerializeField] private int stageIndex;
        [SerializeField] private RectTransform visualRoot;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image frame;
        [SerializeField] private GameObject lockedOverlay;
        [SerializeField] private Text lockLabel;

        public int StageIndex => stageIndex;
        public bool IsUnlocked { get; private set; }
        public bool IsCentered { get; private set; }

        public void Configure(int index, RectTransform visual, CanvasGroup group, Image cardFrame,
            GameObject lockRoot, Text lockedText)
        {
            stageIndex = index;
            visualRoot = visual;
            canvasGroup = group;
            frame = cardFrame;
            lockedOverlay = lockRoot;
            lockLabel = lockedText;
        }

        public void SetState(bool centered, bool unlocked)
        {
            IsCentered = centered;
            IsUnlocked = unlocked;
            if (visualRoot != null) visualRoot.localScale = Vector3.one * (centered ? 1f : .9f);
            if (canvasGroup != null) canvasGroup.alpha = centered ? 1f : .62f;
            if (frame != null) frame.color = centered ? Color.white : new Color(.72f, .76f, .84f, 1f);
            if (lockedOverlay != null) lockedOverlay.SetActive(!unlocked);
            if (lockLabel != null) lockLabel.text = unlocked ? string.Empty : "KİLİTLİ";
        }
    }
}
