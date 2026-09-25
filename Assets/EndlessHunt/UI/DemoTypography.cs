using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace EndlessHunt.UI
{
    // Keeps serialized legacy label references valid while rendering the supplied SDF typeface.
    [DefaultExecutionOrder(1000)]
    public sealed class DemoTypography : MonoBehaviour
    {
        public Text source;
        public TextMeshProUGUI label;
        string _last;Color _color;
#if UNITY_EDITOR
        public static void Apply(GameObject root)
        {
            var theme=DemoTheme.Current;if(!theme)return;
            foreach(var t in root.GetComponentsInChildren<Text>(true))
            {
                if(t.GetComponent<DemoTypography>())continue;
                var bridge=t.gameObject.AddComponent<DemoTypography>();bridge.source=t;
                var go=new GameObject("Myriad SDF",typeof(RectTransform),typeof(TextMeshProUGUI));
                var r=(RectTransform)go.transform;r.SetParent(t.transform,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;
                bridge.label=go.GetComponent<TextMeshProUGUI>();var l=bridge.label;l.font=theme.font;l.fontSize=t.fontSize;
                l.enableAutoSizing=t.resizeTextForBestFit;l.fontSizeMin=t.resizeTextMinSize;l.fontSizeMax=t.fontSize;
                l.alignment=(TextAlignmentOptions)((int[])new[]{257,258,260,513,514,516,1025,1026,1028})[(int)t.alignment];
                l.raycastTarget=false;l.richText=false;l.fontSharedMaterial=t.color.a>0&&t.color.grayscale<.5f?theme.ink:theme.outlined;l.extraPadding=true;
                foreach(var effect in t.GetComponents<BaseMeshEffect>()) effect.enabled=false;
                t.enabled=false;bridge.Sync();
            }
            // Existing TMP labels keep the font and material chosen in the editor.
        }
#endif
        void LateUpdate()=>Sync();
        void Sync(){if(!source||!label)return;if(_last!=source.text){_last=source.text;label.text=_last;}if(_color!=source.color){_color=source.color;label.color=_color;}source.enabled=false;}
    }
}
