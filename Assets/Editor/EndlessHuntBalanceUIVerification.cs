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
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object=UnityEngine.Object;
[InitializeOnLoad]
public static class EndlessHuntBalanceUIVerification
{
    const string Key="EH.BalanceUIVerification";
    static readonly List<string> Checks=new();
    static int _step,_errors,_menuCount,_gameCount;
    static double _next,_deadline;
    static bool _resume;
    static EnemyProjectile _shot;
    static SurvivalEnemy _thrower;
    static float _health;
    static EndlessHuntBalanceUIVerification(){if(!SessionState.GetBool(Key,false))return;EditorApplication.playModeStateChanged+=Changed;Application.logMessageReceived+=Log;}
    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");SessionState.SetInt(Key+".menuCount",UiCount());
        SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;
    }
    static int UiCount()=>Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Count(r=>!r.name.Contains("Caret")&&!r.GetComponent<TMP_SubMeshUI>());
    static void Changed(PlayModeStateChange s)
    {
        if(s==PlayModeStateChange.EnteredPlayMode){_next=EditorApplication.timeSinceStartup+4;_deadline=_next+120;EditorApplication.isPaused=false;EditorApplication.update+=Tick;}
        if(s==PlayModeStateChange.ExitingPlayMode){SessionState.SetString(Key+".report",string.Join("\n",Checks));SessionState.SetInt(Key+".errors",_errors);}
        if(s==PlayModeStateChange.EnteredEditMode){SessionState.SetBool(Key,false);EditorApplication.update-=Tick;File.WriteAllText("Logs/balance-ui-verification.txt",SessionState.GetString(Key+".report","")+"\nERRORS: "+SessionState.GetInt(Key+".errors",0));EditorApplication.delayCall+=Finish;}
    }
    static void Finish(){int errors=SessionState.GetInt(Key+".errors",0);if(errors==0)try{EndlessHuntAndroidScriptCheck.Run();}catch(Exception e){File.AppendAllText("Logs/balance-ui-verification.txt","\nANDROID ERROR "+e);errors++;}EditorApplication.Exit(errors==0?0:1);}
    static void Check(bool ok,string message){Checks.Add((ok?"PASS ":"FAIL ")+message);if(!ok)_errors++;}
    static void Set(object target,string field,object value)=>target.GetType().GetField(field,BindingFlags.NonPublic|BindingFlags.Instance).SetValue(target,value);
    static T Get<T>(object target,string field)=>(T)target.GetType().GetField(field,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(target);
    static void Call(object target,string method,params object[] args)=>target.GetType().GetMethod(method,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(target,args);
    static void CloseDialogs(){foreach(var d in Object.FindObjectsByType<PreparedDialog>(FindObjectsInactive.Include,FindObjectsSortMode.None))d.Close();}
    static void Capture(string name)=>EndlessHuntDemoVerification.Capture("balance-"+name,540,1110);
    static void ClearEnemies(SurvivalGame g){foreach(var e in g.enemyPool)e.gameObject.SetActive(false);g.Enemies.Clear();foreach(var p in g.enemyProjectiles)p.gameObject.SetActive(false);}
    static void Tick()
    {
        if(_resume){_resume=false;EditorApplication.isPaused=false;_next=EditorApplication.timeSinceStartup+3;return;}
        if(EditorApplication.timeSinceStartup<_next)return;_next=EditorApplication.timeSinceStartup+.3;
        if(EditorApplication.timeSinceStartup>_deadline){Check(false,"Test stage timed out: "+_step);EditorApplication.isPlaying=false;return;}
        try
        {
            var d=Object.FindFirstObjectByType<DemoMenu>();var g=Object.FindFirstObjectByType<SurvivalGame>();
            switch(_step)
            {
                case 0:
                    _menuCount=SessionState.GetInt(Key+".menuCount",-1);Check(UiCount()==_menuCount,"Menu opens without creating UI objects");
                    Check(d.shopPage&&d.packs.Length==18&&d.talentButtons.Length==72,"18 currency packs and 72 talent nodes serialized");
                    Check(Object.FindObjectsByType<PreparedDialog>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length==8,"All eight dialogs already exist in the scene");
                    d.optionsButton.onClick.Invoke();Check(d.optionsDialog.gameObject.activeSelf,"Prepared options button opens dialog");
                    d.optionActions[0].onClick.Invoke();Check(d.settingsDialog.gameObject.activeSelf&&!d.optionsDialog.gameObject.activeSelf,"Options navigates to prepared settings");
                    d.volumeSlider.value=.65f;Check(Mathf.Abs(LocalPlayerStore.Data.volume-.65f)<.01f,"Settings volume binding works");break;
                case 1:Capture("01-settings");CloseDialogs();d.Weekly();break;
                case 2:Capture("02-weekly");CloseDialogs();d.Shop(0);break;
                case 3:
                    Capture("03-shop");int gems=GameProfile.Gems;d.packs[0].buy.onClick.Invoke();Check(GameProfile.Gems==gems+d.packs[0].amount&&d.rewardsDialog.gameObject.activeSelf,"Prepared purchase grants gems and opens rewards");GameProfile.Gems=gems;break;
                case 4:Capture("04-rewards");CloseDialogs();d.Profile();Check(d.profileButtons.Length==8&&d.nameInput,"Profile field and eight reusable rows serialized");break;
                case 5:Capture("05-profile");CloseDialogs();d.Leaderboard();Check(d.leaderboardDialog.gameObject.activeSelf,"Prepared local leaderboard opens");break;
                case 6:
                    Capture("06-leaderboard");CloseDialogs();d.menu.upgradeTab.onClick.Invoke();
                    Check(d.menu.upgradePage.activeSelf&&d.menu.tabPressedVisuals[3].gameObject.activeSelf,"Authored bottom navigation still switches pages");break;
                case 7:
                    Capture("07-tree");d.Message("TEST","Hazır mesaj penceresi");CloseDialogs();d.Exit();CloseDialogs();
                    Check(UiCount()==_menuCount,"Shop, profile, settings, rewards, leaderboard and message visits create no scripted UI");
                    SceneManager.LoadScene("GameScene");_next+=3;break;
                case 8:
                    Check(g&&g.balance&&g.materialWorld,"Balance and material world assigned in GameScene");
                    Check(g.enemyPool.Length==120&&g.enemyPool.All(e=>e)&&g.experiencePool.All(e=>e),"Original enemy and crystal pools preserved during map migration");
                    Check(g.mapChunks.Length==0&&g.worldRoot.GetComponentsInChildren<MeshRenderer>(true).Length==1,"One material surface replaces 35 chunks");
                    Check(g.materialWorld.maps.Length==2&&g.enemyProjectiles.Length==24,"Both materials and 24 prepared enemy projectiles assigned");
                    g.enabled=false;g.player.GetComponent<MeleeWeapon>().enabled=false;g.arsenal.enabled=false;ClearEnemies(g);_gameCount=UiCount();
                    var tuning=g.balance;
                    Check(tuning.RewardCost(0,0)==12&&Enumerable.Range(0,12).All(i=>tuning.RewardCost(i+1,1)>tuning.RewardCost(i,1)),"Reward requirements increase at every selection");
                    Check(tuning.RewardCost(5,4)>tuning.RewardCost(5,1),"More unlocked weapons add to reward requirements");
                    Check(tuning.playerSpeed>tuning.EnemySpeed(1,1)&&tuning.EnemySpeed(30,1)<=tuning.maximumEnemySpeed,"Early enemies are slower than player and later speeds are bounded");
                    Check(tuning.Damage(1)==8&&tuning.Damage(10)>tuning.Damage(5)&&tuning.Damage(20)>100,"Wave damage grows to one-hit territory");
                    Check(EnemyProjectile.SegmentDistanceSquared(new Vector2(-10,0),new Vector2(10,0),Vector2.zero)==0,"Swept projectile test catches fast crossings");
                    g.gameplayCamera.transform.position=new Vector3(0,0,-10);g.materialWorld.SelectMap(0);Capture("08-map1");
                    g.materialWorld.SelectMap(1);Capture("09-map2");g.materialWorld.SelectMap(0);
                    g.gameplayCamera.transform.position=new Vector3(-1234,987,-10);g.materialWorld.Refresh();
                    var properties=new MaterialPropertyBlock();g.materialWorld.GetComponent<MeshRenderer>().GetPropertyBlock(properties);var uv=properties.GetVector("_MainTex_ST");
                    Check(uv.z>=0&&uv.z<1&&uv.w>=0&&uv.w<1,"UV offsets wrap across negative and distant world coordinates");
                    Check(g.materialWorld.GetComponent<MeshRenderer>().bounds.Contains(new Vector3(-1234,987,5)),"Surface follows camera at distant coordinates");
                    g.gameplayCamera.transform.position=new Vector3(0,0,-10);g.materialWorld.Refresh();
                    for(int i=0;i<150;i++)Call(g,"SpawnFormation");
                    Check(g.Enemies.Count==tuning.Population(1),"Unkilled first-wave enemies stop at population budget");
                    Check(g.Enemies.All(e=>{var p=e.transform.position;return Mathf.Abs(p.x)>g.gameplayCamera.orthographicSize*g.gameplayCamera.aspect||Mathf.Abs(p.y)>g.gameplayCamera.orthographicSize;}),"Every spawn is outside visible camera rectangle");
                    foreach(var e in g.Enemies)e.transform.position=new Vector3(4,0,0);
                    var crowd=new EnemyCrowd();for(int i=0;i<180;i++)crowd.Resolve(g.Enemies,Vector2.zero,tuning.bodyRadius,.02f);
                    float min=float.MaxValue;for(int i=0;i<g.Enemies.Count;i++)for(int j=i+1;j<g.Enemies.Count;j++)min=Mathf.Min(min,Vector2.Distance(g.Enemies[i].transform.position,g.Enemies[j].transform.position));
                    Check(min>.88f,"Overlapping crowd separates into distinct bodies (minimum "+min.ToString("0.00")+")");Capture("10-separated-crowd");
                    ClearEnemies(g);Set(g,"_wave",4);for(int i=0;i<100;i++)Call(g,"SpawnFormation");
                    Check(g.Enemies.Count>0&&g.Enemies.Count<=tuning.Population(4)&&g.Enemies.Count(e=>e.IsRanged)==tuning.RangedPopulation(4),"Wave four stays within population budget with limited throwers ("+g.Enemies.Count+"/"+tuning.Population(4)+")");
                    // A packed spawn perimeter must skip occupied points; let its occupants enter the arena before filling the budget.
                    for(int batch=0;batch<8;batch++)
                    {
                        for(int i=0;i<g.Enemies.Count;i++)g.Enemies[i].transform.position=new Vector3((i%9-4)*1.3f,(i/9-3)*1.3f);
                        for(int i=0;i<60;i++)Call(g,"SpawnFormation");
                    }
                    Check(g.Enemies.Count==tuning.Population(4),"Wave four reaches its budget when spawn space is available");
                    var recycled=g.Enemies[0];float recycledHealth=Get<float>(recycled,"_health");int liveCount=g.Enemies.Count,crystals=g.Crystals;
                    recycled.transform.position=new Vector3(1000,1000);Call(g,"RepositionDistantEnemy");
                    var returned=recycled.transform.position;
                    Check(recycled.Alive&&returned.sqrMagnitude<1000&&(Mathf.Abs(returned.x)>g.gameplayCamera.orthographicSize*g.gameplayCamera.aspect||Mathf.Abs(returned.y)>g.gameplayCamera.orthographicSize)&&Get<float>(recycled,"_health")==recycledHealth&&g.Enemies.Count==liveCount&&g.Crystals==crystals,"Distant enemy returns outside the viewport without respawning, losing health or granting crystals");
                    ClearEnemies(g);_thrower=g.enemyPool[0];_thrower.Activate(new Vector2(-4,0),null,10000,0,1,false);_thrower.SetRanged(false);g.Enemies.Add(_thrower);
                    g.player.transform.position=Vector3.zero;Set(g.player,"_hurtLock",0f);_health=Get<float>(g.player,"_health");_shot=g.enemyProjectiles[0];_shot.Launch(g,_thrower);
                    Check(!_shot.Flying&&_shot.aimLine.enabled&&Get<float>(g.player,"_health")==_health,"Throw starts with visible warning and no instant damage");break;
                case 9:
                    if(_shot.gameObject.activeSelf)return;
                    Check(Get<float>(g.player,"_health")<_health-20,"Enemy stone hit deals substantial damage");
                    g.player.transform.position=Vector3.zero;_health=Get<float>(g.player,"_health");_shot.Launch(g,_thrower);g.player.transform.position=new Vector3(0,3);break;
                case 10:
                    if(!_shot.Flying||_shot.transform.position.x<1.5f)return;
                    Check(Get<float>(g.player,"_health")==_health&&Mathf.Abs(_shot.transform.position.y)<.01f,"Moving aside dodges the locked straight-line throw");
                    _shot.gameObject.SetActive(false);_shot.Launch(g,_thrower);_thrower.gameObject.SetActive(false);break;
                case 11:
                    if(_shot.gameObject.activeSelf)return;
                    Check(true,"Killing/removing a thrower during warning cancels its shot");ClearEnemies(g);g.player.transform.position=Vector3.zero;
                    Set(g,"_time",0f);g.AddExperience(12);Check(g.levelUpPanel.activeSelf&&Time.timeScale==0,"First 12 crystals open one prepared choice panel");
                    Check(Get<int[]>(g,"_shownChoices")[0]==6,"First weapon offer starts at gear_left_c_11");g.choiceButtons[0].onClick.Invoke();
                    Check(g.RewardsTaken==1&&g.RunLevel==2&&g.RewardCost==25&&g.arsenal.ranks[0]==1,"Taking first ability raises next requirement and unlocks weapon");
                    g.AddExperience(100);Check(!g.levelUpPanel.activeSelf&&Time.timeScale==1&&g.RewardValue==100,"Large pickup preserves overflow without immediately pausing again");
                    Set(g,"_time",g.balance.minimumRewardInterval);Call(g,"TryReward");Check(g.levelUpPanel.activeSelf&&g.RewardValue==75,"Queued reward waits for minimum gameplay interval and deducts one cost");
                    g.choiceButtons[1].onClick.Invoke();Check(g.RewardCost>25&&g.RewardsTaken==2,"Stat selection also increases future reward cost");
                    Check(UiCount()==_gameCount,"Battle rewards and HUD add no runtime UI objects");
                    g.pauseButton.onClick.Invoke();Check(g.pausePanel.activeSelf,"Prepared pause dialog works");break;
                case 12:
                    Capture("11-pause");g.resumeButton.onClick.Invoke();Set(g.player,"_hurtLock",0f);g.player.Damage(g.balance.Damage(30));
                    Check(g.player.IsDead,"Late-wave damage can kill current player in one hit");SceneManager.LoadScene("GameScene2");_next+=3;break;
                case 13:
                    Check(g.materialWorld&&g.materialWorld.maps.Length==2&&g.balance&&g.enemyPool.All(e=>e),"Second gameplay scene has complete serialized references");
                    Check(g.timerText.GetComponentInParent<Canvas>().GetComponent<DemoBattle>().rewardBar,"Second scene HUD is prepared too");
                    EditorApplication.isPlaying=false;return;
            }
            _step++;_deadline=EditorApplication.timeSinceStartup+120;
        }
        catch(Exception e){_errors++;Checks.Add("EXCEPTION "+e);EditorApplication.isPlaying=false;}
    }
    static void Log(string message,string trace,LogType type)
    {
        if(type!=LogType.Error&&type!=LogType.Exception&&type!=LogType.Assert)return;
        if(trace.Contains("UnityEditor.Search.SearchDatabase")){Checks.Add("EDITOR DIAGNOSTIC: copied-project search index");_resume=true;return;}
        _errors++;Checks.Add("RUNTIME ERROR "+message+"\n"+trace);
    }
}
