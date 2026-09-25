using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EndlessHunt;
using EndlessHunt.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object=UnityEngine.Object;

[InitializeOnLoad]
public static class EndlessHuntDemoVerification
{
    const string Key="EH.DemoVerification";
    static readonly List<string> Checks=new();
    static int _step,_errors;
    static double _next;
    static bool _resume;
    static int _coins,_gems,_energy;
    static float _health;
    static EndlessHuntDemoVerification(){if(!SessionState.GetBool(Key,false))return;EditorApplication.playModeStateChanged+=Changed;Application.logMessageReceived+=Log;}
    public static void Run(){SessionState.SetBool(Key,true);EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");EditorApplication.isPlaying=true;}
    public static void ApplyAndRun(){EndlessHuntDemoRevision.Apply();Run();}
    static void Changed(PlayModeStateChange state)
    {
        if(state==PlayModeStateChange.EnteredPlayMode){EditorApplication.isPaused=false;_next=EditorApplication.timeSinceStartup+4;EditorApplication.update+=Tick;}
        if(state==PlayModeStateChange.ExitingPlayMode){SessionState.SetString(Key+".report",string.Join("\n",Checks));SessionState.SetInt(Key+".errors",_errors);}
        if(state==PlayModeStateChange.EnteredEditMode){SessionState.SetBool(Key,false);EditorApplication.update-=Tick;Directory.CreateDirectory("Logs");File.WriteAllText("Logs/demo-verification.txt",SessionState.GetString(Key+".report","")+"\nERRORS: "+SessionState.GetInt(Key+".errors",0));EditorApplication.delayCall+=Finish;}
    }
    static void Finish(){int errors=SessionState.GetInt(Key+".errors",0);if(errors==0)try{EndlessHuntAndroidScriptCheck.Run();}catch(Exception e){File.WriteAllText("Logs/android-script-check.txt","FAILED: "+e);errors++;}EditorApplication.Exit(errors==0?0:1);}
    static void Check(bool ok,string message){Checks.Add((ok?"PASS ":"FAIL ")+message);if(!ok)_errors++;}
    static void Call(object o,string method)=>o.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)?.Invoke(o,null);
    static void Set(object o,string field,object value)=>o.GetType().GetField(field,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(o,value);
    static void CloseDialogs(DemoMenu m){foreach(Transform child in m.overlayRoot){child.gameObject.SetActive(false);Object.Destroy(child.gameObject);}}
    static void Tick()
    {
        if(_resume){_resume=false;EditorApplication.isPaused=false;_next=EditorApplication.timeSinceStartup+3;return;}
        if(EditorApplication.timeSinceStartup<_next)return;_next=EditorApplication.timeSinceStartup+1;
        try
        {
            var m=Object.FindFirstObjectByType<SurvivorMenu>();var demo=Object.FindFirstObjectByType<DemoMenu>();var g=Object.FindFirstObjectByType<SurvivalGame>();
            if(_step>=22){ProjectileCheck(g);return;}
            switch(_step++)
            {
                case 0:
                    Check(m&&demo&&m.homePage.activeSelf,"Home opens with preserved user menu");
                    Check(DemoTheme.Current.font.name=="MYRIADPRO-BOLD SDF","User supplied Myriad SDF typeface loaded");
                    Check(Object.FindObjectsByType<Text>(FindObjectsSortMode.None).All(t=>!t.enabled),"Every active legacy label renders through Myriad TMP");
                    Check(DemoTheme.Current.slots.All(s=>s&&AssetDatabase.GetAssetPath(s).Contains("DEMO_Fantasy_RPG")),"RPG rarity slots resolve to supplied asset package");
                    Capture("01-home",540,960);Capture("01-home-tall",540,1200);Capture("01-home-tablet",768,1024);Capture("01-home-notch",540,1200,true);
                    var swipe=m.stageArt.GetComponentInParent<StageSwipe>();GameProfile.SelectedStage=0;
                    var drag=new PointerEventData(EventSystem.current){position=new Vector2(Screen.width*.8f,Screen.height*.5f)};swipe.OnBeginDrag(drag);drag.position=new Vector2(Screen.width*.2f,Screen.height*.5f);swipe.OnDrag(drag);swipe.OnEndDrag(drag);
                    Check(GameProfile.SelectedStage==1&&!m.playButton.interactable,"Finger swipe selects locked chapter");
                    Check(!m.previousStageButton.gameObject.activeSelf&&!m.nextStageButton.gameObject.activeSelf,"No arrow navigation");
                    demo.Weekly();break;
                case 1:Capture("02-weekly",540,960);CloseDialogs(demo);demo.Rewards(50,0,5);break;
                case 2:Capture("03-rewards",540,960);CloseDialogs(demo);demo.Exit();break;
                case 3:Capture("04-exit",540,960);CloseDialogs(demo);demo.Settings();break;
                case 4:Capture("05-settings",540,960);CloseDialogs(demo);demo.Profile();break;
                case 5:Capture("06-profile",540,960);CloseDialogs(demo);demo.Shop(0);_gems=GameProfile.Gems;break;
                case 6:
                    Capture("07-shop-gems",540,960);
                    var buy=Object.FindObjectsByType<Button>(FindObjectsSortMode.None).First(b=>b.gameObject.activeInHierarchy&&b.name=="TEST SATIN AL");buy.onClick.Invoke();
                    Check(GameProfile.Gems>_gems,"Test diamond pack grants currency");CloseDialogs(demo);demo.Shop(2);_energy=GameProfile.Energy;break;
                case 7:
                    Object.FindObjectsByType<Button>(FindObjectsSortMode.None).First(b=>b.gameObject.activeInHierarchy&&b.name=="TEST SATIN AL").onClick.Invoke();Check(GameProfile.Energy>_energy,"Test energy packs can exceed regeneration cap");CloseDialogs(demo);m.upgradeTab.onClick.Invoke();break;
                case 8:
                    _coins=GameProfile.Coins;LocalPlayerStore.Current.talents.Clear();Check(!LocalPlayerStore.Unlock(3),"Skill prerequisite blocks skipped node");Check(LocalPlayerStore.Unlock(0)&&LocalPlayerStore.Unlock(3),"Tap unlock follows branch");Check(GameProfile.Coins==_coins,"Talent tree does not charge coins");Call(demo,"RefreshTree");Capture("08-talent-tree",540,960);
                    var tree=m.upgradePage.GetComponentInChildren<ScrollRect>();Check(tree.content.childCount>LocalPlayerStore.TalentCount,"72 talent nodes and branch connections present");tree.verticalNormalizedPosition=0;break;
                case 9:Capture("09-talent-depth",540,960);m.equipmentTab.onClick.Invoke();break;
                case 10:
                    Capture("10-equipment",540,960);demo.Leaderboard();break;
                case 11:
                    Capture("11-leaderboard",540,960);CloseDialogs(demo);
                    var targets=new[]{m.gemText,m.coinText,m.energyText};for(int i=0;i<targets.Length;i++)
                    {
                        var r=targets[i].rectTransform;var data=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(r.rect.center))};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);var button=hits.Count>0?hits[0].gameObject.GetComponentInParent<Button>():null;
                        Check(button&&button.name=="Tap target","Top resource "+i+" has working touch target");if(button)button.onClick.Invoke();Check((int)typeof(DemoMenu).GetField("_category",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(demo)==i,"Top resource "+i+" opens matching shop category");
                    }
                    int oldIndex=LocalPlayerStore.Data.active;int oldBalance=GameProfile.Gems;string oldId=LocalPlayerStore.Current.id;
                    if(LocalPlayerStore.Data.hunters.Count>=8)LocalPlayerStore.Data.hunters.RemoveAt(7);
                    Check(LocalPlayerStore.Create("Test Avcı"),"Local registration creates independent profile");GameProfile.Gems=777;LocalPlayerStore.RecordRun(1250,6);LocalPlayerStore.Save();LocalPlayerStore.Load();Check(LocalPlayerStore.Current.name=="Test Avcı"&&LocalPlayerStore.Current.bestScore==1250,"JSON save survives reload with score");LocalPlayerStore.Select(oldIndex);Check(GameProfile.Gems==oldBalance&&LocalPlayerStore.Current.id==oldId,"Switching profiles restores separate wallet");
                    LocalPlayerStore.Current.lastRewardDay="";Check(LocalPlayerStore.ClaimDaily(out _,out _,out _),"Daily reward can be claimed once");Check(!LocalPlayerStore.ClaimDaily(out _,out _,out _),"Duplicate daily claim rejected");GameProfile.SelectedStage=0;SceneManager.LoadScene("GameScene");_next+=4;break;
                case 12:
                    Check(g&&g.mapChunks.Length==35,"Existing 35-chunk endless world preserved");Check(g.goblins.Length==3&&g.goblins.All(s=>s.run.Length==24&&s.attack.Length==16&&s.hurt.Length==8&&s.death.Length==6),"All three goblins have direction/run/attack/hurt/death frames");
                    Check(g.movementJoystick is FloatingJoystick,"Floating joystick assigned");
                    var stick=g.movementJoystick;var down=new PointerEventData(EventSystem.current){position=new Vector2(Screen.width*.68f,Screen.height*.30f)};stick.OnPointerDown(down);down.position+=new Vector2(90,60);stick.OnDrag(down);Check(stick.Direction.sqrMagnitude>.1f,"Joystick moves from touched origin");stick.OnPointerUp(down);Check(stick.Direction==Vector2.zero,"Touch release clears movement");
                    foreach(var e in g.enemyPool)e.gameObject.SetActive(false);g.Enemies.Clear();g.enabled=false;
                    for(int i=0;i<3;i++){var enemy=g.enemyPool[i];enemy.SetSkin(g.goblins[i]);enemy.Activate(g.player.transform.position+new Vector3((i-1)*3,4,0),null,500,0,1,false);g.Enemies.Add(enemy);}
                    for(int i=0;i<15;i++)g.experiencePool[i].Activate(g.player.transform.position+new Vector3((i%5-2)*.7f,-3-i/5*.5f,0),1);
                    Capture("12-battle",540,960);
                    Check(g.experiencePool[0].spriteRenderer.sprite&&g.experiencePool[0].spriteRenderer.sortingOrder==6,"Crystal drop sprite renders above world");
                    for(int i=0;i<3;i++)Check(Mathf.Abs(g.enemyPool[i].transform.localScale.y*g.goblins[i].visibleHeight-1.8f)<.03f,"Goblin "+i+" normalized to character height");
                    _health=g.player.HealthNormalized;Set(g.player,"_hurtLock",0f);g.player.Damage(10);_next=EditorApplication.timeSinceStartup+.19;break;
                case 13:Check(g.player.HealthNormalized<_health&&g.feedback.delayedHealth.value>g.feedback.health.value,"Health damage has delayed trailing bar");Check(g.experiencePool[0].spriteRenderer.bounds.size.y>.3f,"Crystal pop reaches readable world size");Capture("13-damage",540,960);break;
                case 14:Check(Mathf.Abs(g.feedback.health.value-g.feedback.delayedHealth.value)<.02f,"Health trail catches up");g.AddExperience(8);break;
                case 15:
                    Check(g.levelUpPanel.activeSelf&&Time.timeScale==0,"Crystal reward pauses for choice");Check(g.choiceTexts[0].text.Contains(RunArsenal.Names[0]),"First reward offers earliest weapon");Check(g.choiceTexts.Skip(1).All(t=>!RunArsenal.Names.Any(n=>t.text.Contains(n))),"Reward includes stat alternatives");Capture("14-weapon-choice",540,960);g.choiceButtons[0].onClick.Invoke();Check(g.arsenal.ranks[0]==1&&Time.timeScale==1,"Selecting reward enables auto weapon and resumes");g.pauseButton.onClick.Invoke();break;
                case 16:Capture("15-pause",540,960);g.pauseMenuButton.onClick.Invoke();break;
                case 17:Capture("16-battle-exit",540,960);g.exitCancelButton.onClick.Invoke();g.resumeButton.onClick.Invoke();
                    Check(g.arsenal.NextOffer(2)==0,"Advanced weapons gated at low level");for(int i=0;i<11;i++)g.arsenal.Grant(i);Check(g.arsenal.icons.Length==11&&g.arsenal.icons.All(s=>s),"All gear_left_11 through 21 icons wired");Check(RunArsenal.Cooldowns.Distinct().Count()==11&&RunArsenal.Cooldowns.Min()>1,"Eleven distinct cooldowns exceed one second");_next+=2;break;
                case 18:
                    Capture("17-projectiles",540,960);Check(Object.FindObjectsByType<HuntProjectile>(FindObjectsSortMode.None).Length>0,"Auto-projectiles are launched");Set(g,"_time",29f);g.enabled=true;break;
                case 19:Capture("18-wave-warning",540,960);Set(g,"_time",120f);g.enabled=true;break;
                case 20:
                    Check(g.WaveNumber>=2,"Wave counter advances");g.GameOver();break;
                case 21:Capture("19-results",540,960);Check(LocalPlayerStore.Current.runs>0,"Run result recorded in local leaderboard");g.gameOverPanel.SetActive(false);Time.timeScale=1;g.enabled=false;g.arsenal.enabled=false;g.player.GetComponent<MeleeWeapon>().enabled=false;break;
            }
        }
        catch(Exception e){_errors++;Checks.Add("EXCEPTION "+e);EditorApplication.isPlaying=false;}
    }
    static float Health(SurvivalEnemy e)=>(float)typeof(SurvivalEnemy).GetField("_health",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(e);
    static void ProjectileCheck(SurvivalGame game)
    {
        int id=(_step-22)/2;bool prepare=(_step-22)%2==0;_step++;
        if(id>=11){EditorApplication.isPlaying=false;return;}
        if(prepare)
        {
            foreach(var p in Object.FindObjectsByType<HuntProjectile>(FindObjectsInactive.Include,FindObjectsSortMode.None))p.gameObject.SetActive(false);
            foreach(var e in game.enemyPool)e.gameObject.SetActive(false);game.Enemies.Clear();
            for(int i=0;i<4;i++){var e=game.enemyPool[i];e.SetSkin(game.goblins[i%3]);e.Activate(game.player.transform.position+new Vector3(2+i,0),null,10000,0,1,false);game.Enemies.Add(e);}
            if(id==7){for(int i=0;i<11;i++)game.arsenal.ranks[i]=0;game.arsenal.ranks[7]=1;var clocks=(float[])typeof(RunArsenal).GetField("_clocks",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game.arsenal);clocks[7]=0;game.arsenal.enabled=true;}
            else {game.arsenal.enabled=false;var p=Object.FindObjectsByType<HuntProjectile>(FindObjectsInactive.Include,FindObjectsSortMode.None)[0];p.Launch(game.player.transform.position,game.enemyPool[0],id,game.arsenal.icons[id],10);}
            _next=EditorApplication.timeSinceStartup+(id==4?.8:id==5?2.6:2.1);return;
        }
        game.arsenal.enabled=false;
        int hit=game.enemyPool.Take(4).Count(e=>Health(e)<9999);
        int[] expected={1,3,3,2,1,1,2,3,4,3,3};
        Check(hit>=expected[id],RunArsenal.Names[id]+" behavior hits "+hit+" targets (expected >="+expected[id]+")");
        if(id==4){float slow=(float)typeof(SurvivalEnemy).GetField("_slowTime",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(game.enemyPool[0]);Check(slow>0,"Frost applies timed slow");}
        if(id==5)Check(Health(game.enemyPool[0])<9989,"Poison deals damage after initial impact");
        if(id==0||id==4||id==5)Check(hit==1,"Single-target projectile does not splash");
    }
    public static void Capture(string name,int width,int height,bool notch=false)
    {
        var camera=Camera.main;var canvas=Object.FindFirstObjectByType<Canvas>();float size=camera.orthographicSize;
        if(Object.FindFirstObjectByType<SurvivalGame>())camera.orthographicSize=5.4f*height/width;
        var target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);var image=new Texture2D(width,height,TextureFormat.RGBA32,false);
        var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;var mode=canvas.renderMode;
        camera.targetTexture=target;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
        var safe=canvas.GetComponentInChildren<SafeAreaController>();if(notch&&safe)safe.ApplySafeArea(new Rect(0,height*.04f,width,height*.90f),new Vector2(width,height));
        Canvas.ForceUpdateCanvases();var materialWorld=Object.FindFirstObjectByType<InfiniteMap>();if(materialWorld)materialWorld.Refresh();camera.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();Directory.CreateDirectory("Logs/DemoPreviews");File.WriteAllBytes("Logs/DemoPreviews/"+name+".png",image.EncodeToPNG());
        camera.targetTexture=oldTarget;camera.orthographicSize=size;RenderTexture.active=oldActive;canvas.renderMode=mode;if(safe)safe.Refresh(true);if(materialWorld)materialWorld.Refresh();Object.DestroyImmediate(image);Object.DestroyImmediate(target);
    }
    static void Log(string message,string trace,LogType type)
    {
        if(type!=LogType.Error&&type!=LogType.Exception&&type!=LogType.Assert)return;
        if(trace.Contains("UnityEditor.Search.SearchDatabase")){Checks.Add("EDITOR DIAGNOSTIC: copied-project search index; resume Play Mode.");_resume=true;return;}
        _errors++;Checks.Add("RUNTIME ERROR "+message+"\n"+trace);
    }
}
