using UnityEngine;

namespace EndlessHunt
{
    public sealed class SurvivalEnemy : MonoBehaviour
    {
        public SpriteRenderer spriteRenderer;
        private SurvivalGame _game;
        private float _health;
        private float _speed;
        private int _xp;
        private float _contactDamage;
        public void Bind(SurvivalGame game) => _game = game;

        public void Activate(Vector2 position, Sprite sprite, float health, float speed, int xp, bool elite)
        {
            transform.position = position;
            transform.localScale = Vector3.one * (elite ? 2.65f : 1.65f);
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = sprite != null ? Color.white : new Color(.9f, .25f, .3f);
            _health = health; _speed = speed; _xp = xp; _contactDamage = elite ? 18f : 10f;
            gameObject.name = elite ? "Elite Hunter (Pool)" : "Hunter (Pool)";
            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (_game == null || _game.Player == null) return;
            Vector3 direction = (_game.Player.transform.position - transform.position).normalized;
            transform.position += direction * (_speed * Time.deltaTime);
            if (Mathf.Abs(direction.x) > .03f) spriteRenderer.flipX = direction.x < 0f;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.TryGetComponent(out SurvivalPlayer target)) target.Damage(_contactDamage);
        }

        public void Hit(float damage)
        {
            _health -= damage;
            if (_health > 0f) return;
            _game.EnemyKilled(this, _xp);
            gameObject.SetActive(false);
        }
    }
}
