using UnityEngine;

namespace EndlessHunt.UI
{
    [CreateAssetMenu(fileName = "EndlessHuntUITheme", menuName = "Endless Hunt/UI/UI Theme")]
    public sealed class UITheme : ScriptableObject
    {
        [Header("Colors")]
        public Color primary = new(.96f, .49f, .12f, 1f);
        public Color secondary = new(.22f, .48f, .92f, 1f);
        public Color background = new(.075f, .085f, .13f, 1f);
        public Color panel = new(.14f, .16f, .23f, 1f);
        public Color accent = new(1f, .82f, .2f, 1f);
        public Color textPrimary = Color.white;
        public Color textSecondary = new(.78f, .84f, .94f, 1f);
        public Color disabled = new(.38f, .4f, .48f, 1f);
        public Color success = new(.2f, .82f, .45f, 1f);
        public Color warning = new(1f, .68f, .15f, 1f);
        public Color danger = new(.92f, .22f, .25f, 1f);

        [Header("Spacing And Sizing")]
        [Min(0f)] public float smallSpacing = 8f;
        [Min(0f)] public float commonSpacing = 16f;
        [Min(0f)] public float largeSpacing = 24f;
        [Min(44f)] public float buttonHeight = 88f;
        [Min(16f)] public float iconSize = 64f;

        [Header("Typography")]
        [Min(1)] public int titleSize = 42;
        [Min(1)] public int bodySize = 24;
        [Min(1)] public int captionSize = 18;

        [Header("Vector UI Sprites")]
        public Sprite buttonNormal;
        public Sprite buttonPressed;
        public Sprite buttonSelected;
        public Sprite buttonDisabled;
        public Sprite panelSprite;
        public Sprite modalSprite;
        public Sprite progressBackground;
        public Sprite progressFill;
        public Sprite itemSlot;
        public Sprite tabInactive;
        public Sprite tabActive;
        public Sprite badgeSprite;
        public Sprite toastSprite;
    }
}
