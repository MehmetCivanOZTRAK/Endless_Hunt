using System.Collections;
using Spine.Unity;
using UnityEngine;
using DG.Tweening;

namespace EndlessHunt
{
    public sealed class SurvivalPlayer : MonoBehaviour
    {
        public Rigidbody2D body;
        public SkeletonAnimation characterView;
        private SurvivalGame _game;
        private Joystick _stick;
        private float _health;
        private float _maxHealth;
        private float _hurtLock;
        private float _animationLock;
        private bool _running;
        private bool _dead;
        private bool _returnToLocomotion;
        private Tween _hurtTween;
        public const float CharacterHeight = 1.8f;
        public float Speed { get; set; }
        public bool IsDead => _dead;
        public float HealthNormalized => _maxHealth <= 0f ? 0f : _health / _maxHealth;

        public void Setup(SurvivalGame game, Joystick stick, float health, float speed, WeaponDefinition[] weapons)
        {
            _game = game; _stick = stick; _maxHealth = _health = health; Speed = speed; _dead = false;
            CharacterSkin.Apply(characterView, weapons);
            characterView.AnimationState.Data.DefaultMix = .12f;
            characterView.AnimationState.SetAnimation(0, "Idle", true);
            characterView.AnimationState.Apply(characterView.Skeleton);
            characterView.Skeleton.UpdateWorldTransform(Spine.Physics.Update);
            float[] vertices = null;
            characterView.Skeleton.GetBounds(out _, out float bottom, out _, out float height, ref vertices);
            if (height > .01f)
            {
                float parentScale = characterView.transform.parent != null ? Mathf.Abs(characterView.transform.parent.lossyScale.y) : 1f;
                float scale = CharacterHeight / (height * Mathf.Max(.001f, parentScale));
                characterView.transform.localScale = Vector3.one * scale;
                Vector3 position = characterView.transform.localPosition;
                position.y = -bottom * scale - CharacterHeight * .5f / Mathf.Max(.001f, parentScale);
                characterView.transform.localPosition = position;
            }
            characterView.GetComponent<Renderer>().sortingOrder = 10;
        }

        private void FixedUpdate()
        {
            if (_dead) { body.linearVelocity = Vector2.zero; return; }
            if (_hurtLock > 0f) _hurtLock -= Time.fixedDeltaTime;
            if (_animationLock > 0f) _animationLock -= Time.fixedDeltaTime;
            Vector2 input = _stick != null ? _stick.Direction : Vector2.zero;
            input += new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            input = Vector2.ClampMagnitude(input, 1f);
            body.linearVelocity = input * Speed;
            if (_animationLock <= 0f && Mathf.Abs(input.x) > .05f) Face(input.x);
            if (_animationLock > 0f) return;
            bool moving = input.sqrMagnitude > .02f;
            if (moving == _running && !_returnToLocomotion) return;
            _returnToLocomotion = false;
            _running = moving;
            characterView.AnimationState.SetAnimation(0, moving ? "Run_Gear" : "Idle", true);
        }

        public void Face(float x)
        {
            if (Mathf.Abs(x) > .03f) characterView.Skeleton.ScaleX = x > 0f ? -1f : 1f;
        }

        public bool PlayAttack(string animationName, float duration)
        {
            if (_dead || _animationLock > .08f) return false;
            _animationLock = duration * .9f;
            _returnToLocomotion = true;
            _running = false;
            var entry = characterView.AnimationState.SetAnimation(0, string.IsNullOrWhiteSpace(animationName) ? "Attack1" : animationName, false);
            entry.TimeScale = entry.Animation.Duration / Mathf.Max(.1f, duration);
            return true;
        }

        public void Damage(float amount)
        {
            if (_hurtLock > 0f || _dead) return;
            _hurtLock = .48f; _health = Mathf.Max(0f, _health - amount * Mathf.Max(.35f,1f-LocalPlayerStore.Talent(4)*.02f));
            if(Application.isMobilePlatform && LocalPlayerStore.Data.vibration) Handheld.Vibrate();
            _game.PlayerDamaged();
            _hurtTween?.Kill();
            _hurtTween = DOTween.To(() => characterView.Skeleton.GetColor().a, value => characterView.Skeleton.SetColor(1f, 1f, 1f, value), .4f, .08f).SetLoops(4, LoopType.Yoyo).OnComplete(() => characterView.Skeleton.SetColor(Color.white));
            if (_health <= 0f) { StartCoroutine(Die()); return; }
            _animationLock = .34f;
            _returnToLocomotion = true;
            characterView.AnimationState.SetAnimation(0, "Hit", false);
        }

        private IEnumerator Die()
        {
            _dead = true; body.linearVelocity = Vector2.zero; body.simulated = false;
            characterView.AnimationState.SetAnimation(0, "Die", false);
            yield return new WaitForSecondsRealtime(1.15f);
            _game.GameOver();
        }

        public void Heal(float amount) => _health = Mathf.Min(_maxHealth, _health + amount);
        public void IncreaseMaxHealth(float amount) { _maxHealth+=amount;Heal(amount); }
        private void OnDestroy() => _hurtTween?.Kill();
    }
}
