using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EndlessHunt;
using EndlessHunt.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[InitializeOnLoad]
public static class EndlessHuntMobileVerification
{
    private const string Key = "EH.MobileVerification";
    private static int _step;
    private static double _next;
    private static readonly List<string> Checks = new();
    private static int _errors;
    private static float _beforeDamage;
    private static bool _resumeAfterEditorIndexError;
    static EndlessHuntMobileVerification()
    {
        if (!SessionState.GetBool(Key, false)) return;
        EditorApplication.playModeStateChanged += Changed;
        Application.logMessageReceived += Log;
    }
    public static void Run()
    {
        SessionState.SetBool(Key,true);
        // Test data uses the same PlayerPrefs store as the editor; restore the exact original values afterwards.
        var keys = Enum.GetValues(typeof(GearSlot)).Cast<GearSlot>().Select(s => "EH3_BareEquip_"+s)
            .Concat(new[]{"EH3_EquippedWeapon","EH3_SelectedStage","EH3_Coins","EH2_Upgrade_0"});
        foreach(string key in keys)
        {
            SessionState.SetBool("verify.exists."+key,PlayerPrefs.HasKey(key));
            SessionState.SetString("verify.string."+key,PlayerPrefs.GetString(key));
            SessionState.SetInt("verify.int."+key,PlayerPrefs.GetInt(key));
        }
        foreach(GearSlot slot in Enum.GetValues(typeof(GearSlot))) GameProfile.Unequip(slot);
        GameProfile.EquippedWeaponId=GameProfile.StartingWeaponId;
        GameProfile.SelectedStage=0;
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        EditorApplication.isPlaying=true;
    }
    public static void ApplyAndRun() { EndlessHuntMobilePolish.Apply(); Run(); }
    private static void Changed(PlayModeStateChange state)
    {
        if(state==PlayModeStateChange.ExitingPlayMode)
        {
            SessionState.SetString("EH.VerifyReport",string.Join("\n",Checks)); SessionState.SetInt("EH.VerifyErrors",_errors);
        }
        if(state==PlayModeStateChange.EnteredPlayMode) { EditorApplication.isPaused=false; _next=EditorApplication.timeSinceStartup+3; EditorApplication.update+=Tick; }
        if(state!=PlayModeStateChange.EnteredEditMode) return;
        EditorApplication.update-=Tick;
        var keys = Enum.GetValues(typeof(GearSlot)).Cast<GearSlot>().Select(s => "EH3_BareEquip_"+s)
            .Concat(new[]{"EH3_EquippedWeapon","EH3_SelectedStage","EH3_Coins","EH2_Upgrade_0"});
        foreach(string key in keys)
        {
            if(!SessionState.GetBool("verify.exists."+key,false)) PlayerPrefs.DeleteKey(key);
            else if(key.Contains("BareEquip") || key.Contains("EquippedWeapon")) PlayerPrefs.SetString(key,SessionState.GetString("verify.string."+key,""));
            else PlayerPrefs.SetInt(key,SessionState.GetInt("verify.int."+key,0));
        }
        PlayerPrefs.Save(); SessionState.SetBool(Key,false);
        _errors=SessionState.GetInt("EH.VerifyErrors",0);
        Directory.CreateDirectory("Logs"); File.WriteAllText("Logs/mobile-verification.txt",SessionState.GetString("EH.VerifyReport","")+"\nERRORS: "+_errors);
        EditorApplication.Exit(_errors==0 ? 0 : 1);
    }
    private static void Require(bool ok,string message)
    {
        Checks.Add((ok?"PASS ":"FAIL ")+message);
        if(!ok) _errors++;
    }
    private static void Invoke(object target,string name) => target.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic)?.Invoke(target,null);
    private static void Tick()
    {
        if(_resumeAfterEditorIndexError)
        {
            _resumeAfterEditorIndexError=false; EditorApplication.isPaused=false;
            _next=EditorApplication.timeSinceStartup+3; return;
        }
        if(EditorApplication.timeSinceStartup<_next) return;
        _next=EditorApplication.timeSinceStartup+1;
        try
        {
            var menu=UnityEngine.Object.FindFirstObjectByType<SurvivorMenu>();
            var game=UnityEngine.Object.FindFirstObjectByType<SurvivalGame>();
            switch(_step++)
            {
                case 0:
                    Require(menu!=null && menu.homePage.activeSelf,"Menu opens on battle page");
                    Require(GameCatalog.Gear.Length>=230 && menu.equipmentCards.Length==GameCatalog.Gear.Length,"Full clothing catalog wired");
                    Require(menu.weapons.All(w=>GameProfile.Owns(w.id)),"All 40 weapons available");
                    Require(menu.equipmentCards.Where((card,index)=>!string.IsNullOrEmpty(GameCatalog.Gear[index].spineSkin))
                        .All(card=>AssetDatabase.GetAssetPath(card.icon.sprite).Contains("/PartsThumbnail/")),"Every clothing item uses its actual package thumbnail");
                    foreach(var item in GameCatalog.Gear) Require(string.IsNullOrEmpty(item.spineSkin)||menu.equipmentPreview.Skeleton.Data.FindSkin(item.spineSkin)!=null,"Skin exists: "+item.id);
                    Capture("home-9x16",540,960);
                    Capture("home-9x20",540,1200);
                    Capture("home-3x4",768,1024);
                    Capture("home-safe-area",540,1200,true);
                    var swipe=menu.stageArt.GetComponentInParent<StageSwipe>();
                    var drag=new PointerEventData(EventSystem.current){ position=new Vector2(Screen.width*.8f,Screen.height*.5f) };
                    swipe.OnBeginDrag(drag); drag.position=new Vector2(Screen.width*.2f,Screen.height*.5f);
                    swipe.OnDrag(drag); swipe.OnEndDrag(drag);
                    Require(GameProfile.SelectedStage==1,"Touch swipe selects the next chapter"); break;
                case 1:
                    Require(!menu.playButton.interactable && menu.stageLock.activeSelf,"Second chapter locked");
                    Capture("locked-stage",540,960);
                    menu.equipmentTab.onClick.Invoke(); break;
                case 2:
                    Require(menu.equipmentPage.activeSelf && !menu.homePage.activeSelf,"Equipment tab opens real page");
                    Require(menu.equippedSlotIcons.Skip(1).All(i=>!i.enabled),"Bare starting equipment slots");
                    Capture("equipment-bare",540,960);
                    menu.inventoryFilters[2].onClick.Invoke();
                    Require(menu.inventoryWeaponCards.All(c=>!c.button.gameObject.activeSelf),"Category filter hides weapons");
                    var gear=GameCatalog.Gear.First(g=>g.slot==GearSlot.Top);
                    menu.equipmentCards[Array.IndexOf(GameCatalog.Gear,gear)].button.onClick.Invoke();
                    Require(GameProfile.Equipped(GearSlot.Top)==gear.id,"Equip clothing by tapping inventory");
                    menu.inventoryFilters[0].onClick.Invoke(); break;
                case 3:
                    Capture("equipment-equipped",540,960);
                    menu.equippedSlotButtons[1].onClick.Invoke();
                    Require(GameProfile.FindEquipped(GearSlot.Top)==null,"Unequip by tapping equipped slot");
                    menu.upgradeTab.onClick.Invoke(); break;
                case 4:
                    Require(menu.upgradePage.activeSelf,"Talent page connected");
                    GameProfile.Coins=10000; int level=PlayerPrefs.GetInt("EH2_Upgrade_0",1);
                    menu.upgradeRows[0].button.onClick.Invoke();
                    Require(PlayerPrefs.GetInt("EH2_Upgrade_0")==level+1 && GameProfile.Coins==10000-150*level,"Talent increases stat and deducts correct cost");
                    Capture("talents",540,960);
                    GameProfile.SelectedStage=0; SceneManager.LoadScene("GameScene"); _next+=3; break;
                case 5:
                    Require(game!=null && game.mapChunks.Length==35,"Existing endless map preserved");
                    Require(game.enemyWalkFrames.Length==24,"Goblin walk animation has 24 directional frames");
                    Require(game.player.characterView.GetComponent<Renderer>()!=null,"Player renderer present");
                    float heroHeight=game.player.characterView.GetComponent<Renderer>().bounds.size.y;
                    Require(heroHeight>1.5f && heroHeight<2.3f,"Player visible height matches 1.8 world-unit target");
                    if(game.Enemies.Count>0) Require(Mathf.Abs(game.Enemies[0].spriteRenderer.bounds.size.y-heroHeight)<.45f,"Goblin and player have comparable visible size");
                    Capture("battle",540,960);
                    Checks.Add("MEASURE player height="+game.player.characterView.GetComponent<Renderer>().bounds.size.y+" enemy height="+(game.Enemies.Count>0?game.Enemies[0].spriteRenderer.bounds.size.y:0f)+" camera="+game.gameplayCamera.orthographicSize);
                    foreach(var enemy in game.enemyPool) enemy.gameObject.SetActive(false);
                    game.Enemies.Clear(); game.enabled=false;
                    typeof(SurvivalPlayer).GetField("_hurtLock",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(game.player,0f);
                    _beforeDamage=game.player.HealthNormalized; game.player.Damage(10f); _next=EditorApplication.timeSinceStartup+.19; break;
                case 6:
                    Require(game.player.HealthNormalized<_beforeDamage,"Player loses health");
                    Require(game.feedback.delayedHealth.value>game.feedback.health.value,"Damage trail lags health bar");
                    Capture("damage-feedback",540,960); break;
                case 7:
                    Require(Mathf.Abs(game.feedback.health.value-game.feedback.delayedHealth.value)<.02f,"Damage trail catches up");
                    game.AddExperience(8); break;
                case 8:
                    Require(game.levelUpPanel.activeSelf && Time.timeScale==0,"Level choice pauses combat");
                    Require(game.choiceTexts.Select(t=>t.text).Distinct().Count()==3,"Three distinct upgrade choices");
                    Capture("skill-choice",540,960);
                    game.choiceButtons[0].onClick.Invoke();
                    Require(!game.levelUpPanel.activeSelf && Time.timeScale==1,"Choosing skill resumes combat");
                    game.pauseButton.onClick.Invoke(); Require(Time.timeScale==0,"Pause button works");
                    game.resumeButton.onClick.Invoke(); Require(Time.timeScale==1,"Resume button works");
                    EditorApplication.isPlaying=false; break;
            }
        }
        catch(Exception e) { _errors++; Checks.Add("EXCEPTION "+e); EditorApplication.isPlaying=false; }
    }
    private static void Capture(string name,int width,int height,bool notch=false)
    {
        Camera camera=Camera.main;
        float oldSize=camera.orthographicSize;
        if(UnityEngine.Object.FindFirstObjectByType<SurvivalGame>()!=null) camera.orthographicSize=5.4f*height/width;
        Canvas canvas=UnityEngine.Object.FindFirstObjectByType<Canvas>();
        var target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
        var image=new Texture2D(width,height,TextureFormat.RGBA32,false);
        var oldTarget=camera.targetTexture; var oldActive=RenderTexture.active; var mode=canvas.renderMode;
        camera.targetTexture=target; canvas.renderMode=RenderMode.ScreenSpaceCamera; canvas.worldCamera=camera; canvas.planeDistance=1f;
        var safe=canvas.GetComponentInChildren<SafeAreaController>();
        if(notch && safe!=null) safe.ApplySafeArea(new Rect(0,height*.04f,width,height*.90f),new Vector2(width,height));
        Canvas.ForceUpdateCanvases(); camera.Render(); RenderTexture.active=target;
        image.ReadPixels(new Rect(0,0,width,height),0,0); image.Apply();
        Directory.CreateDirectory("Logs/MobilePreviews"); File.WriteAllBytes("Logs/MobilePreviews/"+name+".png",image.EncodeToPNG());
        camera.targetTexture=oldTarget; camera.orthographicSize=oldSize; RenderTexture.active=oldActive; canvas.renderMode=mode;
        if(safe!=null) safe.Refresh(true);
        UnityEngine.Object.DestroyImmediate(image); UnityEngine.Object.DestroyImmediate(target);
    }
    private static void Log(string message,string trace,LogType type)
    {
        if(type!=LogType.Error && type!=LogType.Exception && type!=LogType.Assert) return;
        if(trace.Contains("UnityEditor.Search.SearchDatabase"))
        {
            Checks.Add("EDITOR DIAGNOSTIC: copied-project Search index failed; resuming Play Mode. " + message);
            _resumeAfterEditorIndexError=true; return;
        }
        _errors++; Checks.Add("RUNTIME ERROR "+message+"\n"+trace);
    }
}
