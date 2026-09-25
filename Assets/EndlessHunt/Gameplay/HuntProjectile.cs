using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
namespace EndlessHunt
{
    public sealed class HuntProjectile : MonoBehaviour
    {
        RunArsenal _arsenal;
        SpriteRenderer _sprite;
        TrailRenderer _trail;
        LineRenderer _ring;
        SurvivalEnemy _target;
        readonly HashSet<SurvivalEnemy> _hit=new();
        Vector3 _start,_destination,_direction;
        int _id,_remaining;
        float _damage,_age;
        bool _returning,_exploding;
        public void Initialize(RunArsenal arsenal,Material material)
        {
            _arsenal=arsenal;_sprite=GetComponent<SpriteRenderer>();_sprite.sortingOrder=16;
            _trail=gameObject.AddComponent<TrailRenderer>();_trail.sharedMaterial=material;_trail.time=.22f;_trail.startWidth=.18f;_trail.endWidth=0;_trail.minVertexDistance=.06f;_trail.sortingOrder=15;
            _trail.colorGradient=new Gradient{colorKeys=new[]{new GradientColorKey(new Color(1,.9f,.32f),0),new GradientColorKey(new Color(1,.25f,.08f),1)},alphaKeys=new[]{new GradientAlphaKey(.8f,0),new GradientAlphaKey(0,1)}};
            var ring=new GameObject("Blast Ring",typeof(LineRenderer));ring.transform.SetParent(transform,false);_ring=ring.GetComponent<LineRenderer>();_ring.sharedMaterial=material;_ring.useWorldSpace=true;_ring.loop=true;_ring.widthMultiplier=.12f;_ring.positionCount=32;_ring.sortingOrder=14;_ring.enabled=false;
        }
        public void Launch(Vector3 start,SurvivalEnemy target,int id,Sprite sprite,float damage)
        {
            transform.DOKill();_id=id;_target=target;_damage=damage;_hit.Clear();_age=0;_remaining=id==8?4:3;_start=start;_destination=target.transform.position;_direction=(_destination-start).normalized;_returning=_exploding=false;
            transform.position=start;transform.rotation=Quaternion.identity;transform.localScale=Vector3.one;_sprite.sprite=sprite;_sprite.enabled=true;_sprite.color=Color.white;
            float height=_arsenal.visibleIconHeights!=null&&_arsenal.visibleIconHeights.Length>id?_arsenal.visibleIconHeights[id]:sprite?sprite.bounds.size.y:1;_sprite.transform.localScale=Vector3.one*(.52f/Mathf.Max(.01f,height));
            _ring.enabled=false;gameObject.SetActive(true);_trail.Clear();_trail.emitting=true;
        }
        void Update()
        {
            if(Time.timeScale==0||_exploding)return;_age+=Time.deltaTime;
            if(_age>5){gameObject.SetActive(false);return;}
            if(_id==3||_id==9||_id==10)
            {
                float duration=_id==10?1.2f:.85f,t=Mathf.Clamp01(_age/duration);
                transform.position=Vector3.Lerp(_start,_destination,t)+Vector3.up*Mathf.Sin(t*Mathf.PI)*(_id==10?3:1.8f);
                transform.Rotate(0,0,250*Time.deltaTime);if(t>=1)Explode();return;
            }
            if(_id==6)
            {
                if(_age>.65f)_returning=true;
                transform.position=_returning?Vector3.MoveTowards(transform.position,_arsenal.Game.Player.transform.position,12*Time.deltaTime):transform.position+_direction*9*Time.deltaTime;
                transform.Rotate(0,0,700*Time.deltaTime);
                for(int i=_arsenal.Game.Enemies.Count-1;i>=0;i--){var e=_arsenal.Game.Enemies[i];if(!_hit.Contains(e)&&(e.transform.position-transform.position).sqrMagnitude<.6f)Strike(e);}
                if(_returning&&(transform.position-_arsenal.Game.Player.transform.position).sqrMagnitude<.2f)gameObject.SetActive(false);return;
            }
            if(!_target||!_target.Alive)_target=_arsenal.Nearest(transform.position,_hit,12);
            if(!_target){gameObject.SetActive(false);return;}
            var delta=_target.transform.position-transform.position;transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg-90);
            transform.position=Vector3.MoveTowards(transform.position,_target.transform.position,(_id==8?20:11)*Time.deltaTime);
            if((_target.transform.position-transform.position).sqrMagnitude>.22f)return;
            Strike(_target);
            if(_id==1||_id==2||_id==8)
            {
                _remaining--;if(_remaining<=0){gameObject.SetActive(false);return;}
                if(_id==1){_target=null;float best=25;foreach(var e in _arsenal.Game.Enemies){var d=e.transform.position-transform.position;if(!e.Alive||_hit.Contains(e)||Vector3.Dot(d.normalized,_direction)<.8f)continue;if(d.sqrMagnitude<best){best=d.sqrMagnitude;_target=e;}}}
                else _target=_arsenal.Nearest(transform.position,_hit,5);
                if(!_target)gameObject.SetActive(false);
            }
            else gameObject.SetActive(false);
        }
        void Strike(SurvivalEnemy e){_hit.Add(e);e.Hit(_damage);_arsenal.Game.ShowImpact(e.transform.position,_damage);if(e.Alive){if(_id==4)e.Slow(2,.45f);if(_id==5)e.Poison(3,_damage*.25f);}}
        void Explode()
        {
            _exploding=true;_sprite.enabled=false;_trail.emitting=false;float radius=_id==10?2.8f:_id==9?1.6f:1.9f;
            _arsenal.Blast(_destination,radius,_damage*(_id==10?2.2f:1.5f));_ring.enabled=true;
            DOTween.To(()=>0f,t=>{for(int i=0;i<32;i++){float angle=i*Mathf.PI*2/32;_ring.SetPosition(i,_destination+new Vector3(Mathf.Cos(angle),Mathf.Sin(angle))*radius*t);}_ring.startColor=_ring.endColor=new Color(1,.55f,.12f,1-t);},1,.35f).SetTarget(transform).OnComplete(()=>gameObject.SetActive(false));
            if(_id==9)for(int i=0;i<3;i++){float a=i*Mathf.PI*2/3;_arsenal.Blast(_destination+new Vector3(Mathf.Cos(a),Mathf.Sin(a))*1.6f,1.2f,_damage*.65f);}
        }
        void OnDisable(){transform.DOKill();if(_trail){_trail.emitting=false;_trail.Clear();}}
    }
}
