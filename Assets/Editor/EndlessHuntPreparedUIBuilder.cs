using System;
using System.IO;
using System.Linq;
using EndlessHunt;
using EndlessHunt.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using static EndlessHunt.UI.DemoUI;
using Object=UnityEngine.Object;

// Run explicitly, once. Existing prepared pages are never rebuilt by this migration.
public static class EndlessHuntPreparedUIBuilder
{
    static DemoTheme T=>DemoTheme.Current;
    const string Prefabs="Assets/EndlessHunt/UI/Screens/";
    [MenuItem("Endless Hunt/Migrate Runtime UI To Prepared Scenes")]
    public static void Run()
    {
        foreach(var scene in Enumerable.Range(0,UnityEngine.SceneManagement.SceneManager.sceneCount).Select(UnityEngine.SceneManagement.SceneManager.GetSceneAt))
            if(scene.isDirty)throw new InvalidOperationException("Save open scene edits before running the UI migration.");
        BakeMenu(); BakeBattle("GameScene"); BakeBattle("GameScene2");
        AssetDatabase.SaveAssets();EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        File.WriteAllText("Logs/prepared-ui-bake.txt","PASS: menu/dialog/HUD layouts serialized into scenes and prefabs; two material maps and enemy projectile pools assigned.");
    }
    static Button Icon(Transform p,Sprite sprite,string name,float x,float y,Action unused=null)
    {var b=Button(p,"",x,y,94,94,null,T.darkButton);b.name=name;Art("Icon",b.transform,sprite,0,15,62,62).preserveAspect=true;return b;}
    static Button Hit(Transform p)
    {
        var i=Art("Tap target",p,null,0,0,0,0,Color.clear);Stretch(i.rectTransform);i.raycastTarget=true;
        i.rectTransform.offsetMin=new Vector2(-32,-20);i.rectTransform.offsetMax=new Vector2(24,20);
        var b=i.gameObject.AddComponent<Button>();b.targetGraphic=i;return b;
    }
    static PreparedDialog Dialog(Transform p,string title,float w,float h,Sprite body=null,Sprite header=null,bool close=true)
    {
        var card=Modal(p,title,w,h,body,header);var d=card.parent.gameObject.AddComponent<PreparedDialog>();d.card=card;d.group=card.GetComponent<CanvasGroup>();
        d.closeButtons=close?new[]{Button(card,"",0,h+28,76,76,null,T.closeButton)}:Array.Empty<Button>();
        d.gameObject.SetActive(false);return d;
    }
    static void BakeMenu()
    {
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");var m=Object.FindFirstObjectByType<SurvivorMenu>();var d=m.GetComponent<DemoMenu>();
        if(d.shopPage){Debug.Log("Prepared menu exists; authored layout retained.");return;}
        var root=m.transform.root.gameObject;
        foreach(var page in new[]{m.homePage,m.equipmentPage,m.upgradePage,m.storePage})if(!page.GetComponent<CanvasGroup>())page.AddComponent<CanvasGroup>();
        d.optionsButton=Icon(m.homePage.transform,T.options,"Options Button",-442,70);
        d.leaderboardButton=Icon(m.homePage.transform,T.skills,"Leaderboard Button",442,70);
        d.weeklyButton=Icon(m.homePage.transform,T.chest,"Weekly Reward Button",-442,210);
        d.nameButton=Hit(m.playerNameText.transform);d.avatarButton=Hit(d.avatarImage.transform);
        d.energyButton=Hit(m.energyText.transform);d.goldButton=Hit(m.coinText.transform);d.gemButton=Hit(m.gemText.transform);
        m.previousStageButton.gameObject.SetActive(false);m.nextStageButton.gameObject.SetActive(false);
        BakeShop(d);BakeTree(d);BakeDialogs(d);
        DemoTypography.Apply(root);
        PrefabUtility.SaveAsPrefabAsset(root,Prefabs+"UI_MobileMenu.prefab");
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
    }
    static void BakeShop(DemoMenu d)
    {
        var p=Rect("Currency Shop",d.menu.storePage.transform.parent,0,0,0,0);var src=(RectTransform)d.menu.storePage.transform;
        p.anchorMin=src.anchorMin;p.anchorMax=src.anchorMax;p.offsetMin=src.offsetMin;p.offsetMax=src.offsetMax;
        d.shopPage=p.gameObject;Stretch(Art("Background",p,null,0,0,0,0,new Color(.10f,.16f,.20f)).rectTransform);
        Label(p,"MAĞAZA",0,70,800,75,52);Label(p,"TEST MAĞAZASI • GERÇEK ÖDEME ALINMAZ",0,148,940,44,24);
        string[] names={"ELMAS","ALTIN","ENERJİ"};d.categoryButtons=new Button[3];d.shopCategories=new GameObject[3];d.packs=new PreparedPack[18];
        int[][] counts={new[]{80,500,1200,2500,6500,14000},new[]{500,1500,5000,12000,30000,80000},new[]{5,10,20,40,80,160}};
        string[] sizes={"CEP PAKETİ","KÜÇÜK KESE","AVCI PAKETİ","BÜYÜK SANDIK","DEV SANDIK","KRAL SANDIĞI"};
        for(int cat=0;cat<3;cat++)
        {
            d.categoryButtons[cat]=Button(p,names[cat],(cat-1)*310,220,288,86,null);
            var content=Scroll(p,"Packs "+names[cat],332,20,out var scroll);content.sizeDelta=new Vector2(0,1110);d.shopCategories[cat]=scroll.gameObject;
            for(int i=0;i<6;i++)
            {
                float x=i%2==0?-238:238,y=i/2*365;var card=Art("Pack "+i,content,T.creamModal,x,y,450,345);
                Label(card.transform,sizes[i],0,20,406,52,30,true);Sprite icon=cat==0?T.gem:cat==1?T.coin:T.energy;int copies=1+i/2;
                for(int j=0;j<copies;j++)Art("Currency",card.transform,icon,(j-(copies-1)*.5f)*60,87+Mathf.Abs(j-(copies-1)*.5f)*12,116,116).preserveAspect=true;
                Label(card.transform,counts[cat][i].ToString("N0"),0,211,380,44,40,true);
                d.packs[cat*6+i]=new PreparedPack{category=cat,amount=counts[cat][i],buy=Button(card.transform,"TEST SATIN AL",0,264,355,62,null)};
            }
            scroll.gameObject.SetActive(cat==0);
        }
        p.gameObject.SetActive(false);
    }
    static void BakeTree(DemoMenu d)
    {
        var page=d.menu.upgradePage.transform;foreach(Transform c in page)c.gameObject.SetActive(false);
        Stretch(Art("Tree Background",page,null,0,0,0,0,new Color(.11f,.16f,.20f)).rectTransform);
        Label(page,"YETENEK AĞACI",0,75,920,70,48);Label(page,"Önceki düğümü aç • Yeteneğe dokun",0,151,920,48,27);
        var content=Scroll(page,"Talent Tree",228,20,out var scroll);content.sizeDelta=new Vector2(0,24*265+35);
        d.talentButtons=new Button[LocalPlayerStore.TalentCount];d.talentFrames=new Image[d.talentButtons.Length];d.talentLabels=new TMP_Text[d.talentButtons.Length];
        Sprite[] icons={T.energy,T.sword,T.skills,T.gem,T.settings,T.chest};
        for(int i=0;i<d.talentButtons.Length;i++)
        {
            float x=(i%3-1)*312,y=i/3*265+24;
            if(i>=3)Art("Branch",content,null,x,y-110,12,120,new Color(.27f,.45f,.48f)).transform.SetAsFirstSibling();
            var b=Button(content,"",x,y,154,154,null,T.slots[0]);b.name="Talent "+i;d.talentButtons[i]=b;d.talentFrames[i]=b.GetComponent<Image>();
            Art("Talent Icon",b.transform,icons[i%6],0,30,86,86).preserveAspect=true;
            Art("Node Caption",content,null,x,y+165,286,65,new Color(.11f,.16f,.20f));d.talentLabels[i]=Label(content,LocalPlayerStore.TalentDetails[i%6],x,y+158,290,80,27);
        }
    }
    static void BakeDialogs(DemoMenu d)
    {
        var p=d.overlayRoot;
        d.weeklyDialog=Dialog(p,"HAFTALIK ÖDÜL",760,920,T.notebook,T.notebookHeader);var card=d.weeklyDialog.card;d.rewardDays=new PreparedDay[7];
        for(int i=0;i<7;i++)
        {
            float x=i==6?0:(i%3-1)*224,y=i==6?662:132+i/3*253,w=i==6?662:204;
            var b=Button(card,"",x,y,w,220,null,i==6?T.rewardWide:T.rewardCard);b.name="Day "+(i+1);
            Label(b.transform,"GÜN "+(i+1),0,12,w-20,36,27);
            Art("Reward",b.transform,i==6?T.chest:T.coin,i==6?-56:-38,62,88,88).preserveAspect=true;
            Art("Bonus",b.transform,i==6?T.gem:i%2==0?T.energy:T.gem,i==6?56:45,78,66,66).preserveAspect=true;
            var label=Label(b.transform,"ÖDÜL",0,158,w-16,55,23,i==6);var claimed=Art("Claimed",b.transform,T.tick,43,100,64,64).gameObject;claimed.SetActive(false);
            d.rewardDays[i]=new PreparedDay{button=b,frame=b.GetComponent<Image>(),label=label,claimed=claimed};
        }
        d.rewardsDialog=Dialog(p,"ÖDÜLLER",780,360,T.darkModal,T.ribbon);card=d.rewardsDialog.card;
        var row=Rect("Reward Items",card,0,124,630,180);var layout=row.gameObject.AddComponent<HorizontalLayoutGroup>();layout.childAlignment=TextAnchor.UpperCenter;layout.childControlWidth=layout.childControlHeight=layout.childForceExpandWidth=layout.childForceExpandHeight=false;layout.spacing=20;
        d.rewardGroups=new GameObject[3];d.rewardAmounts=new TMP_Text[3];Sprite[] currencies={T.coin,T.gem,T.energy};
        for(int i=0;i<3;i++){var item=Rect("Reward "+i,row,0,0,185,180);Art("Icon",item,currencies[i],0,0,120,120).preserveAspect=true;d.rewardGroups[i]=item.gameObject;d.rewardAmounts[i]=Label(item,"x0",0,140,185,48,33);}
        d.optionsDialog=Dialog(p,"SEÇENEKLER",720,675);card=d.optionsDialog.card;string[] names={"AYARLAR","MESAJLAR","PROFİL","HAFTALIK ÖDÜL","OYUNDAN ÇIK"};d.optionActions=new Button[5];
        for(int i=0;i<5;i++)d.optionActions[i]=Button(card,names[i],0,126+i*102,570,80,null);
        d.settingsDialog=Dialog(p,"AYARLAR",740,745);card=d.settingsDialog.card;Label(card,"SES SEVİYESİ",0,125,620,55,32,true);
        var bar=Art("Volume",card,T.barBase,0,195,550,55);d.volumeSlider=bar.gameObject.AddComponent<Slider>();
        var fill=Art("Fill",bar.transform,T.levelFill,0,6,535,42);Stretch(fill.rectTransform);d.volumeSlider.fillRect=fill.rectTransform;
        var handle=Art("Handle",bar.transform,T.cyanButton,0,0,40,60);d.volumeSlider.handleRect=handle.rectTransform;d.volumeSlider.targetGraphic=handle;handle.raycastTarget=true;bar.raycastTarget=true;
        d.vibrationButton=Button(card,"",0,295,570,86,null);d.vibrationLabel=Label(d.vibrationButton.transform,"TİTREŞİM",0,8,540,62,30);
        d.damageNumbersButton=Button(card,"",0,401,570,86,null);d.damageNumbersLabel=Label(d.damageNumbersButton.transform,"HASAR SAYILARI",0,8,540,62,30);
        d.frameRateButton=Button(card,"",0,507,570,86,null);d.frameRateLabel=Label(d.frameRateButton.transform,"AKICILIK",0,8,540,62,30);
        Label(card,"Dikey ekran • Dokunduğun yerde joystick\nAyarlar bu cihazda saklanır",0,620,650,75,25,true);
        BakeProfile(d);
        d.leaderboardDialog=Dialog(p,"LİDERLİK TABLOSU",850,1040);card=d.leaderboardDialog.card;Label(card,"BU CİHAZDAKİ AVCILAR",0,117,780,55,30,true);
        d.rankingContent=Scroll(card,"Scores",205,70,out _);d.rankingContent.sizeDelta=new Vector2(0,8*98);d.rankingRows=new GameObject[8];d.rankingLabels=new TMP_Text[8];
        for(int i=0;i<8;i++){var r=Art("Rank "+(i+1),d.rankingContent,T.darkButton,0,i*98,755,84);d.rankingRows[i]=r.gameObject;d.rankingLabels[i]=Label(r.transform,"AVCI",0,6,720,68,30);}
        Label(card,"En iyi skor • Eşitlikte en yüksek dalga",0,965,770,42,24,true);
        d.exitDialog=Dialog(p,"OYUNDAN ÇIK",730,510,close:false);card=d.exitDialog.card;Label(card,"Oyundan ayrılmak\nistediğine emin misin?",0,161,620,154,40,true);
        d.quitButton=Button(card,"ÇIK",-159,365,280,85,null,T.pinkButton);d.exitDialog.closeButtons=new[]{Button(card,"VAZGEÇ",159,365,280,85,null)};
        d.messageDialog=Dialog(p,"BİLGİ",780,650);card=d.messageDialog.card;d.messageTitle=card.GetComponentsInChildren<TMP_Text>(true).First();d.messageBody=Label(card,"",0,134,675,435,34,true);
    }
    static void BakeProfile(DemoMenu d)
    {
        d.profileDialog=Dialog(d.overlayRoot,"AVCI PROFİLİ",790,1030);var card=d.profileDialog.card;
        var art=Art("Name Input",card,T.slots[0],0,126,620,95);art.raycastTarget=true;d.nameInput=art.gameObject.AddComponent<TMP_InputField>();
        var text=Label(art.transform,"",0,10,570,70,37);d.nameInput.textViewport=text.rectTransform;d.nameInput.textComponent=text;d.nameInput.characterLimit=16;d.nameInput.lineType=TMP_InputField.LineType.SingleLine;
        Label(card,"İsim: 2–16 karakter",0,225,650,42,25,true);d.avatarButtons=new Button[T.avatars.Length];
        for(int i=0;i<d.avatarButtons.Length;i++){var b=Button(card,"",(i-(T.avatars.Length-1)*.5f)*130,286,114,114,null);d.avatarButtons[i]=b;Art("Portrait",b.transform,T.avatars[i],0,8,98,98).preserveAspect=true;}
        d.saveProfileButton=Button(card,"KAYDET",-160,435,300,82,null);d.newProfileButton=Button(card,"YENİ KAYIT",160,435,300,82,null,T.greenButton);
        Label(card,"BU CİHAZDAKİ PROFİLLER",0,548,670,52,30,true);d.profileContent=Scroll(card,"Profiles",615,34,out _);d.profileContent.sizeDelta=new Vector2(0,8*88);
        d.profileButtons=new Button[8];d.profileLabels=new TMP_Text[8];
        for(int i=0;i<8;i++){d.profileButtons[i]=Button(d.profileContent,"AVCI",0,i*88,650,75,null);d.profileLabels[i]=d.profileButtons[i].GetComponentInChildren<TMP_Text>();}
    }
    static void BakeBattle(string name)
    {
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");var g=Object.FindFirstObjectByType<SurvivalGame>();var d=g.timerText.GetComponentInParent<Canvas>().GetComponent<DemoBattle>();
        var tuning=AssetDatabase.LoadAssetAtPath<SurvivalBalance>("Assets/EndlessHunt/Resources/SurvivalBalance.asset");
        if(!tuning){tuning=ScriptableObject.CreateInstance<SurvivalBalance>();AssetDatabase.CreateAsset(tuning,"Assets/EndlessHunt/Resources/SurvivalBalance.asset");}
        g.balance=tuning;
        tuning.maximumPopulation=Mathf.Min(tuning.maximumPopulation,g.enemyPool.Length);EditorUtility.SetDirty(tuning);
        if(!g.materialWorld)
        {
            foreach(var chunk in g.mapChunks)if(chunk)Object.DestroyImmediate(chunk.gameObject);
            g.worldRoot.name="Infinite Material World";g.mapChunks=Array.Empty<Transform>();g.mapChunkRenderers=Array.Empty<SpriteRenderer>();
            var quad=GameObject.CreatePrimitive(PrimitiveType.Quad);quad.name="World Surface - Material UV";Object.DestroyImmediate(quad.GetComponent<Collider>());quad.transform.SetParent(g.worldRoot,false);
            g.materialWorld=quad.AddComponent<InfiniteMap>();g.materialWorld.viewCamera=g.gameplayCamera;
            g.materialWorld.maps=new[]{AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Map1.mat"),AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Map2.mat")};
            foreach(var mat in g.materialWorld.maps)
            {
                var ti=AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(mat.mainTexture)) as TextureImporter;
                if(ti&&ti.wrapMode!=TextureWrapMode.Repeat){ti.wrapMode=TextureWrapMode.Repeat;ti.SaveAndReimport();}
                Debug.Log("World material: "+mat.name+" / "+mat.shader.name);
            }
            quad.GetComponent<MeshRenderer>().sortingOrder=-30;g.materialWorld.SelectMap(g.forcedMapIndex>=0?g.forcedMapIndex:0);
        }
        if(g.enemyProjectiles==null||g.enemyProjectiles.Length==0)
        {
            var pool=new GameObject("Enemy Stone Projectiles (Prepared Pool)");pool.transform.SetParent(g.transform,false);g.enemyProjectiles=new EnemyProjectile[24];
            var sprite=EndlessHuntSceneFactory.Sprite(EndlessHuntSceneFactory.LayerThumbnails+"gear_left/gear_left_c_19.png");
            float height=VisibleHeight(sprite);var material=g.arsenal.trailMaterial;
            for(int i=0;i<24;i++)
            {
                var go=new GameObject("Stone "+i);go.transform.SetParent(pool.transform,false);var shot=go.AddComponent<EnemyProjectile>();
                var art=new GameObject("gear_left_c_19",typeof(SpriteRenderer));art.transform.SetParent(go.transform,false);art.transform.localScale=Vector3.one*(.6f/height);
                shot.artwork=art.GetComponent<SpriteRenderer>();shot.artwork.sprite=sprite;shot.artwork.sortingOrder=18;
                shot.trail=go.AddComponent<TrailRenderer>();shot.trail.sharedMaterial=material;shot.trail.time=.18f;shot.trail.startWidth=.14f;shot.trail.endWidth=0;shot.trail.sortingOrder=17;shot.trail.minVertexDistance=.06f;
                shot.trail.startColor=new Color(1,.48f,.14f,.8f);shot.trail.endColor=new Color(.45f,.16f,.06f,0);shot.trail.emitting=false;
                var aim=new GameObject("Aim Warning",typeof(LineRenderer));aim.transform.SetParent(go.transform,false);shot.aimLine=aim.GetComponent<LineRenderer>();
                shot.aimLine.sharedMaterial=material;shot.aimLine.useWorldSpace=true;shot.aimLine.positionCount=2;shot.aimLine.widthMultiplier=.06f;shot.aimLine.startColor=shot.aimLine.endColor=new Color(1,.25f,.08f,.6f);shot.aimLine.sortingOrder=5;shot.aimLine.enabled=false;
                go.SetActive(false);g.enemyProjectiles[i]=shot;
            }
        }
        if(!d.rewardBar)
        {
            var hud=Rect("Reward HUD (Prepared)",d.safeArea,0,0,0,0);Stretch(hud);hud.SetSiblingIndex(g.levelUpPanel.transform.GetSiblingIndex());
            var bar=Art("Crystal Reward Bar",hud,T.barBase,0,235,970,44);d.rewardBar=bar.gameObject.AddComponent<Slider>();d.rewardBar.interactable=false;
            var fill=Art("Fill",bar.transform,T.rewardFill,0,5,950,34);Stretch(fill.rectTransform);fill.rectTransform.offsetMin=new Vector2(8,7);fill.rectTransform.offsetMax=new Vector2(-8,-7);d.rewardBar.fillRect=fill.rectTransform;
            d.rewardText=Label(hud,"YETENEK 1 • 0 / 12 ELMAS",0,280,900,40,26);
            d.weaponIcons=new Image[11];d.weaponRanks=new TMP_Text[11];
            for(int i=0;i<11;i++){var slot=Art("Run Weapon "+i,hud,T.slots[0],(i-5)*80,335,72,72,new Color(.45f,.5f,.56f));d.weaponIcons[i]=Art("Icon",slot.transform,g.arsenal.icons[i],0,8,52,52);d.weaponIcons[i].preserveAspect=true;d.weaponRanks[i]=Label(slot.transform,"",0,50,68,28,20);}
            var banner=Art("Wave Notice",hud,T.ribbon,0,453,820,120);d.notice=banner.gameObject.AddComponent<CanvasGroup>();d.notice.alpha=0;d.waveNotice=Label(banner.transform,"DALGA 1",0,12,720,85,36);
        }
        var canvas=g.timerText.GetComponentInParent<Canvas>();
        foreach(var motion in canvas.GetComponentsInChildren<PopupMotion>(true))if(!motion.GetComponent<CanvasGroup>())motion.gameObject.AddComponent<CanvasGroup>();
        DemoTypography.Apply(canvas.gameObject);
        if(name=="GameScene")PrefabUtility.SaveAsPrefabAsset(canvas.gameObject,Prefabs+"UI_BattleHUD.prefab");
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
    }
    static float VisibleHeight(Sprite sprite)
    {
        var texture=new Texture2D(2,2);texture.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(sprite)));
        var pixels=texture.GetPixels32();int min=texture.height,max=0;
        for(int y=0;y<texture.height;y++)for(int x=0;x<texture.width;x++)if(pixels[y*texture.width+x].a>20){min=Mathf.Min(min,y);max=Mathf.Max(max,y);}
        Object.DestroyImmediate(texture);return Mathf.Max(.01f,(max-min+1)/sprite.pixelsPerUnit);
    }
}
