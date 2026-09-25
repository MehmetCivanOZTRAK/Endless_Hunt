using System;
using System.Collections.Generic;
using UnityEngine;
namespace EndlessHunt
{
    public enum ShotKind { Homing, Pierce, Bounce, Bomb, Frost, Poison, Return, Volley, Chain, Cluster, Meteor }
    public sealed class RunArsenal : MonoBehaviour
    {
        public Sprite[] icons;
        public float[] visibleIconHeights;
        public Material trailMaterial;
        public readonly int[] ranks=new int[11];
        public static readonly string[] Names={"GÜDÜMLÜ TOP","DELİCİ ŞUT","SEKEN KARPUZ","PATLAYICI PAS","BUZLU GAZOZ","ZEHİRLİ HAVUÇ","DÖNEN ÖRDEK","ÜÇLÜ BEYZBOL","YILDIRIM TAŞI","PARÇA BOMBASI","MUZ METEORU"};
        public static readonly string[] Details={"Tek hedefi takip eder","3 düşmanı delip geçer","3 farklı hedef arasında seker","Yay çizerek gider; alanı patlatır","Hedefi 2 sn yavaşlatır","3 sn boyunca zehirler","Atılır ve avcıya döner","3 farklı düşmana aynı anda","4 hedefe elektrik sıçratır","Patlar; çevrede 3 patlama daha","Geniş alana ağır darbe"};
        public static readonly float[] Cooldowns={2.4f,3.6f,4.2f,5.5f,3.2f,4.5f,4.8f,5.2f,6.2f,7.4f,9f};
        public static readonly int[] UnlockLevel={2,3,4,5,6,7,8,9,10,12,14};
        readonly float[] _clocks=new float[11];
        readonly List<HuntProjectile> _pool=new();
        SurvivalGame _game;
        void Start(){_game=GetComponent<SurvivalGame>();for(int i=0;i<40;i++)Create();}
        HuntProjectile Create(){var go=new GameObject("Projectile (Pool)",typeof(SpriteRenderer),typeof(HuntProjectile));go.transform.SetParent(transform);var p=go.GetComponent<HuntProjectile>();p.Initialize(this,trailMaterial);go.SetActive(false);_pool.Add(p);return p;}
        public SurvivalGame Game=>_game;
        public float Cooldown(int id)=>Cooldowns[id]*Mathf.Pow(.93f,Mathf.Max(0,ranks[id]-1));
        public bool Grant(int id){if(id<0||id>=11||ranks[id]>=5)return false;ranks[id]++;_clocks[id]=.4f;return true;}
        public int NextOffer(int level)
        {
            for(int i=0;i<11;i++)if(ranks[i]==0&&UnlockLevel[i]<=level)return i;
            var eligible=new List<int>();for(int i=0;i<11;i++)if(ranks[i]>0&&ranks[i]<5)eligible.Add(i);
            return eligible.Count==0?-1:eligible[UnityEngine.Random.Range(0,eligible.Count)];
        }
        void Update()
        {
            if(!_game||!_game.Player||_game.Player.IsDead||Time.timeScale==0)return;
            for(int i=0;i<11;i++)
            {
                if(ranks[i]==0)continue;_clocks[i]-=Time.deltaTime;if(_clocks[i]>0)continue;
                var target=Nearest(_game.Player.transform.position,null,13);if(!target)continue;
                _clocks[i]=Cooldown(i);var exclude=new HashSet<SurvivalEnemy>();
                int count=i==7?3:1;for(int n=0;n<count&&target;n++){Fire(i,target);exclude.Add(target);target=Nearest(_game.Player.transform.position,exclude,13);}
            }
        }
        void Fire(int id,SurvivalEnemy enemy)
        {
            var shot=_pool.Find(p=>!p.gameObject.activeSelf);if(!shot){if(_pool.Count>=64)return;shot=Create();}
            shot.Launch(_game.Player.transform.position,enemy,id,icons[id],_game.Damage*(.7f+ranks[id]*.18f));
        }
        public SurvivalEnemy Nearest(Vector3 point,HashSet<SurvivalEnemy> excluded,float radius)
        {
            SurvivalEnemy result=null;float best=radius*radius;
            foreach(var e in _game.Enemies){if(!e||!e.Alive||excluded!=null&&excluded.Contains(e))continue;float d=(e.transform.position-point).sqrMagnitude;if(d<best){result=e;best=d;}}
            return result;
        }
        public void Blast(Vector3 center,float radius,float damage)
        {
            // Hits can remove entries from Enemies; iterate backwards.
            for(int i=_game.Enemies.Count-1;i>=0;i--){var e=_game.Enemies[i];if(e.Alive&&(e.transform.position-center).sqrMagnitude<radius*radius){e.Hit(damage);_game.ShowImpact(e.transform.position,damage);}}
        }
    }
}
