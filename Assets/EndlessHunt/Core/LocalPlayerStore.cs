using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace EndlessHunt
{
    [Serializable] public sealed class LocalHunter
    {
        public string id="legacy", name="AVCI", lastRewardDay="";
        public int avatar, rewardDay, bestScore, bestWave, runs;
        public List<int> talents=new();
    }
    [Serializable] public sealed class LocalSave
    {
        public int version=1, active, frameRate=60;
        public float volume=1;
        public bool vibration=true, damageNumbers=true, messagesRead;
        public List<LocalHunter> hunters=new(){new LocalHunter()};
    }
    public static class LocalPlayerStore
    {
        static LocalSave _data;
        public static string SavePath=>Path.Combine(Application.persistentDataPath,"hunters-v1.json");
        public static LocalSave Data { get { if(_data==null) Load(); return _data; } }
        public static LocalHunter Current=>Data.hunters[Mathf.Clamp(Data.active,0,Data.hunters.Count-1)];
        public static string WalletPrefix=>Current.id=="legacy"?"EH3_":"EH4_"+Current.id+"_";
        public static void Load()
        {
            _data=Read(SavePath)??Read(SavePath+".bak")??new LocalSave();
            if(_data.hunters==null||_data.hunters.Count==0) _data.hunters=new(){new LocalHunter()};
            _data.active=Mathf.Clamp(_data.active,0,_data.hunters.Count-1);
            foreach(var h in _data.hunters) {h.talents??=new(); if(string.IsNullOrWhiteSpace(h.name)) h.name="AVCI";}
            ApplySettings();
        }
        static LocalSave Read(string path) { try { return File.Exists(path)?JsonUtility.FromJson<LocalSave>(File.ReadAllText(path)):null; } catch(Exception e) {Debug.LogWarning("Yerel kayıt okunamadı: "+e.Message);return null;} }
        public static void Save()
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            File.WriteAllText(SavePath+".tmp",JsonUtility.ToJson(Data,true));
            if(File.Exists(SavePath)) File.Replace(SavePath+".tmp",SavePath,SavePath+".bak");
            else File.Move(SavePath+".tmp",SavePath);
        }
        public static bool Create(string name)
        {
            name=(name??"").Trim(); if(name.Length<2||name.Length>16||Data.hunters.Count>=8) return false;
            Data.hunters.Add(new LocalHunter{id=Guid.NewGuid().ToString("N"),name=name});Data.active=Data.hunters.Count-1;Save();return true;
        }
        public static void Select(int index) {if(index<0||index>=Data.hunters.Count)return;Data.active=index;Save();}
        public static void RecordRun(int score,int wave) {var h=Current;h.runs++;h.bestScore=Mathf.Max(h.bestScore,score);h.bestWave=Mathf.Max(h.bestWave,wave);Save();}
        public static List<LocalHunter> Ranking()=>Data.hunters.OrderByDescending(h=>h.bestScore).ThenByDescending(h=>h.bestWave).ThenBy(h=>h.name,StringComparer.Ordinal).ToList();
        public static bool RewardReady=>Current.lastRewardDay!=DateTime.UtcNow.ToString("yyyy-MM-dd");
        public static bool ClaimDaily(out int coins,out int gems,out int energy)
        {
            coins=gems=energy=0; if(!RewardReady)return false;
            int day=Current.rewardDay%7; coins=day==6?1000:50*(day+1);gems=day==6?300:day%2==1?50:0;energy=day<6&&day%2==0?5:0;
            GameProfile.Coins+=coins;GameProfile.Gems+=gems;GameProfile.AddEnergy(energy);
            Current.lastRewardDay=DateTime.UtcNow.ToString("yyyy-MM-dd");Current.rewardDay++;Save();return true;
        }
        public const int TalentCount=72;
        public static readonly string[] TalentNames={"DAYANIKLILIK","KESKİN DARBE","ÇEVİKLİK","MIKNATIS","ZIRH","YENİLENME"};
        public static readonly string[] TalentDetails={"Azami can +10","Hasar +%4","Hareket hızı +%2","Toplama alanı +%5","Alınan hasar -%2","Her 10 saniyede +1 can"};
        // Three independent branches; every node depends on the preceding node in its branch.
        public static bool CanUnlock(int id)=>id>=0&&id<TalentCount&&!Current.talents.Contains(id)&&(id<3||Current.talents.Contains(id-3));
        public static bool Unlock(int id) {if(!CanUnlock(id))return false;Current.talents.Add(id);Save();return true;}
        public static int Talent(int kind)=>Current.talents.Count(i=>i%6==kind);
        public static void ApplySettings() {AudioListener.volume=_data.volume;Application.targetFrameRate=_data.frameRate;}
    }
}
