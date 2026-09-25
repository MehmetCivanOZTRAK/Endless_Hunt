using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace EndlessHunt.UI
{
    // Editor-only layout recipes. Player assemblies only consume serialized scene/prefab references.
    public static class DemoUI
    {
        public static readonly Color Ink=new(.19f,.16f,.24f);
        public static RectTransform Rect(string name,Transform parent,float x,float y,float w,float h)
        {
            var go=new GameObject(name,typeof(RectTransform));var r=(RectTransform)go.transform;r.SetParent(parent,false);r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.pivot=new Vector2(.5f,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;
        }
        public static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
        public static Image Art(string name,Transform parent,Sprite sprite,float x,float y,float w,float h,Color? color=null)
        {var r=Rect(name,parent,x,y,w,h);var i=r.gameObject.AddComponent<Image>();i.sprite=sprite;i.color=color??Color.white;i.raycastTarget=false;if(sprite&&sprite.border.sqrMagnitude>0)i.type=Image.Type.Sliced;return i;}
        public static TextMeshProUGUI Label(Transform p,string value,float x,float y,float w,float h,int size=34,bool ink=false)
        {var r=Rect(value,p,x,y,w,h);var l=r.gameObject.AddComponent<TextMeshProUGUI>();l.font=DemoTheme.Current.font;l.fontSharedMaterial=ink?DemoTheme.Current.ink:DemoTheme.Current.outlined;l.color=ink?Ink:Color.white;l.text=value;l.fontSize=size;l.enableAutoSizing=true;l.fontSizeMin=size*.72f;l.fontSizeMax=size;l.alignment=TextAlignmentOptions.Center;l.raycastTarget=false;l.extraPadding=true;return l;}
        public static Button Button(Transform p,string text,float x,float y,float w,float h,Action action,Sprite sprite=null)
        {var i=Art(text,p,sprite?sprite:DemoTheme.Current.cyanButton,x,y,w,h);i.raycastTarget=true;var b=i.gameObject.AddComponent<Button>();b.targetGraphic=i;b.navigation=new Navigation{mode=Navigation.Mode.None};if(text.Length>0)Label(i.transform,text,0,5,w-24,h-14,32);if(action!=null)b.onClick.AddListener(()=>action());return b;}
        public static RectTransform Scroll(Transform p,string name,float top,float bottom,out ScrollRect scroll)
        {
            var view=Rect(name,p,0,top,0,0);Stretch(view);view.offsetMin=new Vector2(20,bottom);view.offsetMax=new Vector2(-20,-top);
            var image=view.gameObject.AddComponent<Image>();image.color=Color.clear;view.gameObject.AddComponent<RectMask2D>();
            scroll=view.gameObject.AddComponent<ScrollRect>();scroll.viewport=view;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=40;
            var content=Rect("Content",view,0,0,980,1000);content.anchorMin=new Vector2(0,1);content.anchorMax=Vector2.one;content.sizeDelta=new Vector2(0,1000);scroll.content=content;return content;
        }
        public static RectTransform Modal(Transform parent,string title,float w,float h,Sprite body=null,Sprite header=null)
        {
            var t=DemoTheme.Current;var overlay=Art(title+" Overlay",parent,null,0,0,0,0,new Color(.02f,.04f,.07f,.78f));Stretch(overlay.rectTransform);overlay.raycastTarget=true;
            var card=Art("Dialog",overlay.transform,body?body:t.creamModal,0,0,w,h).rectTransform;
            card.anchorMin=card.anchorMax=card.pivot=new Vector2(.5f,.5f);card.anchoredPosition=Vector2.zero;
            bool notebook=header==t.notebookHeader,ribbon=header==t.ribbon;
            Art("Header",card,header?header:t.darkHeader,0,notebook?-56:ribbon?-28:-5,ribbon?w+80:w,notebook?150:ribbon?140:90);Label(card,title,0,notebook?14:5,w-48,60,36);
            card.gameObject.AddComponent<CanvasGroup>();
            return card;
        }
        public static void Close(RectTransform card){if(card)UnityEngine.Object.Destroy(card.parent.gameObject);}
        public static void CloseButton(RectTransform card){Button(card,"",0,card.sizeDelta.y+28,76,76,()=>Close(card),DemoTheme.Current.closeButton);}
    }
}
