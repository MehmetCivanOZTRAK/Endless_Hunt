using System;
using System.IO;
using System.Linq;
using System.Text;
using EndlessHunt;
using EndlessHunt.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

public static class EndlessHuntDemoRevision
{
    const string R="Assets/EndlessHunt/Resources/";
    static Sprite Ui(string path)=>EndlessHuntSceneFactory.Ui(path+".png");
    static Sprite Icon(string name)=>Ui("Icons/128px/"+name+"_icon_128px");
    static T Asset<T>(string path) where T:ScriptableObject {var a=AssetDatabase.LoadAssetAtPath<T>(path);if(!a){a=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(a,path);}return a;}
    [MenuItem("Endless Hunt/Apply Demo UI Revision (preserve top bar)")]
    public static void Apply()
    {
        Directory.CreateDirectory(R);AssetDatabase.Refresh();Theme();Goblins();Menu();
        foreach(string name in new[]{"GameScene","GameScene2"})Battle(name);
        AssetDatabase.SaveAssets();EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        Debug.Log("Demo revision saved. User top-bar layout and art verified unchanged.");
    }
    static void Theme()
    {
        var t=Asset<DemoTheme>(R+"DemoTheme.asset");
        t.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/myriad-pro/MYRIADPRO-BOLD SDF.asset");
        if(!t.font)throw new Exception("MYRIADPRO-BOLD SDF missing");
        t.font.atlasPopulationMode=AtlasPopulationMode.Dynamic;
        t.font.TryAddCharacters("ÇĞİÖŞÜçğıöşü•–0123456789%",out string missing);
        if(!string.IsNullOrEmpty(missing))Debug.LogWarning("Myriad glyphs not present: "+missing);
        t.outlined=Material("Myriad Outline",t.font.material,true);t.ink=Material("Myriad Ink",t.font.material,false);
        t.creamModal=Ui("Modals/modalSimple_v2");t.darkModal=Ui("Modals/darkModalSimple_dark");t.notebook=Ui("Modals/modalSimple_v2");
        t.ribbon=Ui("Headers/headerBow_cyan");t.notebookHeader=Ui("Headers/notebook_pink");t.darkHeader=Ui("Headers/header_black");
        t.rewardCard=Ui("Cards/card_black");t.rewardAvailable=Ui("Cards/card_available");t.rewardWide=Ui("Containers/containerReward_cream");
        t.cyanButton=Ui("Buttons/button_cyan");t.pinkButton=Ui("Buttons/button_pink");t.greenButton=Ui("Buttons/button_green");t.darkButton=Ui("Buttons/button_black");t.closeButton=Ui("Buttons/buttonExit_black");
        string[] tabs={"store","equipment","adventure","skills","shop"};t.tabsDefault=tabs.Select(s=>Ui("Buttons/"+s+"_1ButtonDefault")).ToArray();t.tabsPressed=tabs.Select(s=>Ui("Buttons/"+s+"ButtonPressed")).ToArray();
        string slot="Assets/DEMO_Fantasy_RPG_UI_doboui/Item_slots/";
        t.slots=new[]{"gray","green","sky"}.Select(s=>EndlessHuntSceneFactory.Sprite(slot+"itemSlot1_"+s+".png")).ToArray();
        t.selectedSlots=new[]{"gray","green","sky"}.Select(s=>EndlessHuntSceneFactory.Sprite(slot+"itemSlotSelected1_"+s+".png")).ToArray();
        t.gem=Icon("gem");t.coin=Icon("coin");t.energy=Icon("energy");t.options=Icon("options");t.settings=Icon("settings");t.mail=Icon("mail");t.skills=Icon("skills");t.sword=Icon("sword");t.chest=Icon("chestRuby");t.tick=Icon("tickV1");t.lockIcon=Icon("warningCircle");
        t.avatars=new[]{Icon("adventure"),Icon("armor"),Icon("chestSkull"),Icon("skills"),Icon("sword")};
        t.barBase=Ui("ProgressBars/progressBarBase_black");t.healthFill=Ui("ProgressBars/progressBar_red");t.levelFill=Ui("ProgressBars/progressBar_green");t.rewardFill=Ui("ProgressBars/progressBar_cyan");
        EditorUtility.SetDirty(t);EditorUtility.SetDirty(t.font);AssetDatabase.SaveAssets();
    }
    static Material Material(string name,Material source,bool outline)
    {
        string path=R+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(source);AssetDatabase.CreateAsset(m,path);}else m.CopyPropertiesFromMaterial(source);
        m.SetColor("_FaceColor",Color.white);m.SetFloat("_FaceDilate",outline?.16f:0);m.SetFloat("_OutlineWidth",outline?.22f:0);m.SetColor("_OutlineColor",new Color(.09f,.07f,.12f));m.EnableKeyword("UNDERLAY_ON");m.SetColor("_UnderlayColor",outline?new Color(.05f,.04f,.08f,.95f):new Color(0,0,0,0));m.SetFloat("_UnderlayOffsetX",0);m.SetFloat("_UnderlayOffsetY",outline?-.65f:0);m.SetFloat("_UnderlayDilate",.10f);m.SetFloat("_UnderlaySoftness",.04f);EditorUtility.SetDirty(m);return m;
    }
    [Serializable] class Manifest {public GoblinManifest[] goblins;}
    [Serializable] class GoblinManifest {public string kind;public Frame[] frames;}
    [Serializable] class Frame {public string name;public int x,y;public float height;}
    static void Goblins()
    {
        var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText("Assets/EndlessHunt/Art/Goblins/manifest.json"));
        foreach(var g in manifest.goblins)
        {
            string name=g.kind.Replace(" ",""),path="Assets/EndlessHunt/Art/Goblins/"+name+".png";
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;importer.spritePixelsPerUnit=100;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.maxTextureSize=2048;importer.filterMode=FilterMode.Bilinear;importer.textureCompression=TextureImporterCompression.Compressed;
            #pragma warning disable CS0618
            importer.spritesheet=g.frames.Select(f=>new SpriteMetaData{name=f.name,rect=new Rect(f.x,f.y,192,192),alignment=(int)SpriteAlignment.Center,pivot=new Vector2(.5f,.5f)}).ToArray();
            #pragma warning restore CS0618
            importer.SaveAndReimport();
            var sprites=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(s=>s.name);
            var set=Asset<GoblinSet>(R+name+".asset");set.title=g.kind;set.visibleHeight=g.frames.First(f=>f.name=="Running_Front_0").height;
            set.run=g.frames.Where(f=>f.name.StartsWith("Running_")).Select(f=>sprites[f.name]).ToArray();set.attack=g.frames.Where(f=>f.name.StartsWith("Attacking_")).Select(f=>sprites[f.name]).ToArray();set.hurt=g.frames.Where(f=>f.name.StartsWith("Hurt_")).Select(f=>sprites[f.name]).ToArray();set.death=g.frames.Where(f=>f.name.StartsWith("Dying_")).Select(f=>sprites[f.name]).ToArray();EditorUtility.SetDirty(set);
        }
    }
    static string Fingerprint(Transform root)
    {
        var b=new StringBuilder();foreach(var r in root.GetComponentsInChildren<RectTransform>(true)){var i=r.GetComponent<Image>();b.AppendLine(r.name+"|"+r.anchorMin+"|"+r.anchorMax+"|"+r.offsetMin+"|"+r.offsetMax+"|"+r.localScale+"|"+(i?AssetDatabase.GetAssetPath(i.sprite):""));}return b.ToString();
    }
    static void Menu()
    {
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");var m=Object.FindFirstObjectByType<SurvivorMenu>();
        var top=m.energyText.transform.parent;string before=Fingerprint(top);var root=m.homePage.transform.root.gameObject;
        if(PrefabUtility.IsPartOfPrefabInstance(root))PrefabUtility.UnpackPrefabInstance(root,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
        var demo=m.GetComponent<DemoMenu>();if(!demo)demo=m.gameObject.AddComponent<DemoMenu>();demo.menu=m;demo.topBar=top;demo.avatarImage=top.GetComponentsInChildren<Image>(true).FirstOrDefault(i=>i.name=="Avatar");
        var overlay=m.homePage.transform.parent.parent.Find("Demo Dialog Layer");if(!overlay){var go=new GameObject("Demo Dialog Layer",typeof(RectTransform));overlay=go.transform;overlay.SetParent(m.homePage.transform.parent.parent,false);DemoUI.Stretch((RectTransform)overlay);}demo.overlayRoot=overlay;
        m.previousStageButton.gameObject.SetActive(false);m.nextStageButton.gameObject.SetActive(false);
        var screens=(RectTransform)m.homePage.transform.parent;screens.offsetMax=new Vector2(screens.offsetMax.x,-210);
        // Navigation artwork, child states, sizing and button transitions are authored by the user.
        string after=Fingerprint(top);if(before!=after)throw new Exception("Protected top bar changed");
        Directory.CreateDirectory("Logs");File.WriteAllText("Logs/topbar-preservation.txt","PASS: geometry and sprite paths identical before/after scene migration\n"+before);
        DemoTypography.Apply(root);
        PrefabUtility.SaveAsPrefabAssetAndConnect(root,"Assets/EndlessHunt/UI/Screens/UI_MobileMenu.prefab",InteractionMode.AutomatedAction);
        EditorSceneManager.SaveScene(scene);
    }
    static void Battle(string name)
    {
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");var game=Object.FindFirstObjectByType<SurvivalGame>();
        var old=game.timerText.GetComponentInParent<Canvas>();if(old)Object.DestroyImmediate(old.gameObject);
        EndlessHuntGameSceneBuilder.BuildHud(game);
        var canvas=game.timerText.GetComponentInParent<Canvas>();var safe=game.timerText.transform.parent.parent;
        var t=DemoTheme.Current;game.goblins=new[]{"MaleGoblin","FemaleGoblin","ChiefGoblin"}.Select(s=>AssetDatabase.LoadAssetAtPath<GoblinSet>(R+s+".asset")).ToArray();
        game.enemySprites=new[]{game.goblins[0].run[0]};game.enemyWalkFrames=Array.Empty<Sprite>();game.enemyVisibleHeight=game.goblins[0].visibleHeight;
        foreach(var gem in game.experiencePool){gem.spriteRenderer.sprite=t.gem;gem.spriteRenderer.color=Color.white;gem.spriteRenderer.sortingOrder=6;}
        var arsenal=game.GetComponent<RunArsenal>();if(!arsenal)arsenal=game.gameObject.AddComponent<RunArsenal>();game.arsenal=arsenal;
        arsenal.icons=Enumerable.Range(11,11).Select(n=>EndlessHuntSceneFactory.Sprite(EndlessHuntSceneFactory.LayerThumbnails+"gear_left/gear_left_c_"+n+".png")).ToArray();
        arsenal.visibleIconHeights=arsenal.icons.Select(s=>VisibleHeight(s)).ToArray();
        var mat=AssetDatabase.LoadAssetAtPath<Material>(R+"Projectile Trail.mat");if(!mat){mat=new Material(Shader.Find("Sprites/Default"));AssetDatabase.CreateAsset(mat,R+"Projectile Trail.mat");}arsenal.trailMaterial=mat;
        Object.DestroyImmediate(game.movementJoystick.gameObject);
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Joystick Pack/Prefabs/Floating Joystick.prefab");var joy=(GameObject)PrefabUtility.InstantiatePrefab(prefab,safe);joy.name="Floating Touch Joystick";var rect=joy.GetComponent<RectTransform>();DemoUI.Stretch(rect);rect.SetAsFirstSibling();game.movementJoystick=joy.GetComponent<Joystick>();
        // Joystick's transparent hit surface remains behind all actionable HUD elements and dialogs.
        var demo=canvas.gameObject.AddComponent<DemoBattle>();demo.game=game;demo.safeArea=safe;
        game.xpBar.fillRect.GetComponent<Image>().sprite=t.levelFill;
        var pause=(RectTransform)game.pausePanel.transform;Compact(pause,t.creamModal,750,690);game.resumeButton.GetComponent<Image>().sprite=t.cyanButton;game.pauseMenuButton.GetComponent<Image>().sprite=t.pinkButton;
        var pauseCard=pause.GetChild(0);EndlessHuntSceneFactory.Image("Pause Header",pauseCard,t.darkHeader,new Vector2(0,1),Vector2.one,new Vector2(0,-100),Vector2.zero).transform.SetAsFirstSibling();
        var pauseTitle=pauseCard.GetComponentsInChildren<Text>().First(x=>x.name=="Title");pauseTitle.rectTransform.anchorMin=new Vector2(.06f,.85f);pauseTitle.rectTransform.anchorMax=new Vector2(.94f,.985f);pauseTitle.color=Color.white;
        foreach(var label in game.resumeButton.GetComponentsInChildren<Text>())label.color=Color.white;foreach(var label in game.pauseMenuButton.GetComponentsInChildren<Text>())label.color=Color.white;
        var exit=game.exitConfirmPanel.GetComponent<Image>();exit.sprite=null;exit.color=new Color(.02f,.04f,.07f,.78f);
        var exitCard=exit.transform.GetChild(0) as RectTransform;exitCard.anchorMin=exitCard.anchorMax=exitCard.pivot=new Vector2(.5f,.5f);exitCard.anchoredPosition=Vector2.zero;exitCard.sizeDelta=new Vector2(730,510);
        exitCard.GetComponent<Image>().sprite=t.creamModal;exitCard.GetChild(0).GetComponent<Image>().sprite=t.darkHeader;game.exitConfirmButton.GetComponent<Image>().sprite=t.pinkButton;game.exitCancelButton.GetComponent<Image>().sprite=t.cyanButton;
        Compact((RectTransform)game.gameOverPanel.transform,t.darkModal,850,1050);
        game.pauseButton.GetComponent<RectTransform>().anchoredPosition=new Vector2(-65,-72);game.pauseButton.GetComponent<RectTransform>().sizeDelta=new Vector2(80,80);
        game.pauseButton.GetComponentInChildren<Text>().text="II";
        game.scoreText.rectTransform.anchorMax=new Vector2(.82f,.94f);
        game.levelUpTitle.transform.parent.GetComponent<Image>().sprite=Ui("Headers/headerBow_yellow");
        DemoTypography.Apply(canvas.gameObject);
        EditorSceneManager.SaveScene(scene);
    }
    static void Compact(RectTransform overlay,Sprite sprite,float width,float height)
    {
        var children=overlay.Cast<Transform>().ToArray();var i=overlay.GetComponent<Image>();i.sprite=null;i.color=new Color(.02f,.04f,.07f,.78f);
        var card=EndlessHuntSceneFactory.Image("Demo Modal Card",overlay,sprite,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(-width/2,-height/2),new Vector2(width/2,height/2));
        foreach(var c in children)c.SetParent(card.transform,false);
        foreach(var text in card.GetComponentsInChildren<Text>())text.color=sprite==DemoTheme.Current.creamModal?DemoUI.Ink:Color.white;
    }
    static float VisibleHeight(Sprite sprite)
    {
        var tex=new Texture2D(2,2);tex.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(sprite)));var pixels=tex.GetPixels32();int low=tex.height,high=0;
        for(int y=0;y<tex.height;y++)for(int x=0;x<tex.width;x++)if(pixels[y*tex.width+x].a>30){low=Math.Min(low,y);high=Math.Max(high,y);}
        Object.DestroyImmediate(tex);return Math.Max(1,high-low+1)/sprite.pixelsPerUnit;
    }
}
