using UnityEngine;
using TMPro;
namespace EndlessHunt.UI
{
    public sealed class DemoTheme : ScriptableObject
    {
        public TMP_FontAsset font;
        public Material outlined, ink;
        public Sprite creamModal,darkModal,notebook,ribbon,notebookHeader,darkHeader;
        public Sprite cyanButton,pinkButton,greenButton,darkButton,closeButton;
        public Sprite[] tabsDefault,tabsPressed,slots,selectedSlots,avatars;
        public Sprite gem,coin,energy,options,settings,mail,skills,sword,chest,tick,lockIcon;
        public Sprite barBase,healthFill,levelFill,rewardFill;
        public Sprite rewardCard,rewardAvailable,rewardWide;
        public static DemoTheme Current=>Resources.Load<DemoTheme>("DemoTheme");
    }
}
