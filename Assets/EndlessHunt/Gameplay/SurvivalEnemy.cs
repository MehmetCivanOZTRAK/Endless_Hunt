using UnityEngine;
using DG.Tweening;
namespace EndlessHunt
{
    public sealed class SurvivalEnemy : MonoBehaviour
    {
        public SpriteRenderer spriteRenderer;
        SurvivalGame _game;
        GoblinSet _skin;
        float _health,_speed,_contactDamage,_stagger,_clock,_deathClock,_attackClock,_slowTime,_slowFactor=1,_poisonTime,_poisonTick,_poisonDamage;
        int _xp;
        Vector3 _baseScale;
        Vector2 _direction;
        bool _dying, _elite, _ranged, _winding;
        float _rangedClock, _windup, _rangedHold;
        Vector3 _knockback;
        public bool IsElite => _elite;
        public bool IsRanged => _ranged;
        public void SetRanged(bool value) { _ranged = value; _rangedClock = Random.Range(2f, 5f); }
        public bool Alive=>gameObject.activeSelf&&!_dying&&_health>0;
        public void Bind(SurvivalGame game)=>_game=game;
        public void SetSkin(GoblinSet skin)=>_skin=skin;
        public void Activate(Vector2 position,Sprite sprite,float health,float speed,int xp,bool elite)
        {
            transform.DOKill();spriteRenderer.DOKill();transform.position=position;
            float height=_skin?_skin.visibleHeight:_game.enemyVisibleHeight;
            _baseScale=Vector3.one*(SurvivalPlayer.CharacterHeight/Mathf.Max(.01f,height));transform.localScale=_baseScale;
            var collider=GetComponent<CircleCollider2D>();collider.radius=.48f/_baseScale.x;collider.offset=Vector2.zero;collider.enabled=true;
            _stagger=_deathClock=_attackClock=_slowTime=_poisonTime=0;_clock=Random.value;_dying=false;_slowFactor=1;
            _health=health;_speed=speed;_xp=xp;_elite=elite;_direction=Vector2.down;
            _ranged = _winding = false; _windup = _rangedHold = 0; _knockback = Vector3.zero;
            spriteRenderer.sprite=_skin?_skin.run[0]:sprite;spriteRenderer.color=Color.white;spriteRenderer.flipX=false;spriteRenderer.sortingOrder=9;
            gameObject.name=(_skin?_skin.title:"Goblin")+" (Pool)";gameObject.SetActive(true);
        }
        void Update()
        {
            if(!_game||!_game.Player||Time.timeScale==0)return;
            if(_dying){_deathClock+=Time.deltaTime;if(_skin&&_skin.death.Length>0)spriteRenderer.sprite=_skin.death[Mathf.Min(_skin.death.Length-1,(int)(_deathClock*12))];if(_deathClock>.55f)gameObject.SetActive(false);return;}
            if(_game.Player.IsDead)return;
            _clock+=Time.deltaTime;_stagger=Mathf.Max(0,_stagger-Time.deltaTime);_slowTime-=Time.deltaTime;
            if(_poisonTime>0){_poisonTime-=Time.deltaTime;_poisonTick-=Time.deltaTime;if(_poisonTick<=0){_poisonTick=1;Hit(_poisonDamage);if(_dying)return;}}
            var tuning = _game.balance;
            Vector3 delta=_game.Player.transform.position-transform.position;_direction=delta.normalized;
            bool close=delta.sqrMagnitude<tuning.attackRange*tuning.attackRange;
            _attackClock -= Time.deltaTime; _rangedClock -= Time.deltaTime; _rangedHold -= Time.deltaTime;
            if(_skin)spriteRenderer.sprite=_skin.Frame(_direction,_winding?tuning.attackWindup-_windup:_clock,_winding||_rangedHold>0,_stagger>0);
            transform.position += _knockback * Time.deltaTime;
            if(_stagger>0) { _winding=false; return; }
            if (_rangedHold > 0) return;
            if (_winding)
            {
                _windup -= Time.deltaTime;
                if (_windup <= 0)
                {
                    _winding=false; _attackClock=tuning.attackCooldown;
                    if (delta.sqrMagnitude < (tuning.attackRange+.15f)*(tuning.attackRange+.15f)) _game.Player.Damage(tuning.Damage(_game.WaveNumber,_elite));
                }
                return;
            }
            if(close) { if(_attackClock<=0){_winding=true;_windup=tuning.attackWindup;} return; }
            if (_ranged && _rangedClock <= 0 && delta.sqrMagnitude < tuning.rangedDistance*tuning.rangedDistance && _game.ThrowAtPlayer(this))
            { _rangedClock=tuning.rangedCooldown+Random.Range(0,2f);_rangedHold=tuning.rangedWindup;return; }
            transform.position+=(Vector3)_direction*(_speed*(_slowTime>0?_slowFactor:1)*Time.deltaTime);
        }
        public void Slow(float seconds,float factor){_slowTime=seconds;_slowFactor=factor;}
        public void Poison(float seconds,float damage){_poisonTime=seconds;_poisonTick=1;_poisonDamage=damage;}
        public void Hit(float damage)
        {
            if(!Alive)return;_health-=damage;_stagger=.14f;
            transform.DOKill();spriteRenderer.DOKill();transform.localScale=_baseScale;
            var away=(transform.position-_game.Player.transform.position).normalized;
            _knockback=away*3.2f;
            DOTween.To(()=>_knockback,value=>_knockback=value,Vector3.zero,.16f).SetEase(Ease.OutCubic).SetTarget(transform);
            spriteRenderer.color=new Color(1,.45f,.35f);spriteRenderer.DOColor(Color.white,.16f);
            if(_health>0){transform.DOPunchScale(_baseScale*.06f,.16f,1,.2f);return;}
            _dying=true;_game.EnemyKilled(this,_xp);GetComponent<CircleCollider2D>().enabled=false;
            spriteRenderer.DOFade(0,.2f).SetDelay(.35f);
        }
        void OnDisable(){transform.DOKill();if(spriteRenderer)spriteRenderer.DOKill();}
    }
}
