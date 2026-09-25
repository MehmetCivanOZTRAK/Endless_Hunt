using UnityEngine;
namespace EndlessHunt
{
    public sealed class EnemyProjectile : MonoBehaviour
    {
        public SpriteRenderer artwork;
        public TrailRenderer trail;
        public LineRenderer aimLine;
        SurvivalGame _game;
        SurvivalEnemy _owner;
        Vector3 _aim, _direction;
        float _clock, _damage;
        bool _flying;
        public bool Flying => _flying;
        public void Launch(SurvivalGame game, SurvivalEnemy owner)
        {
            _game = game; _owner = owner; _clock = 0; _flying = false;
            _aim = game.Player.transform.position;
            transform.position = owner.transform.position; transform.rotation = Quaternion.identity;
            _damage = game.balance.Damage(game.WaveNumber, owner.IsElite) * game.balance.projectileDamageMultiplier;
            gameObject.SetActive(true); artwork.enabled = true; trail.Clear(); trail.emitting = false;
            aimLine.enabled = true; aimLine.SetPosition(0, transform.position); aimLine.SetPosition(1, _aim);
        }
        void Update()
        {
            if (!_game || Time.timeScale == 0 || _game.Player.IsDead) return;
            _clock += Time.deltaTime;
            if (!_flying)
            {
                if (!_owner || !_owner.Alive) { gameObject.SetActive(false); return; }
                transform.position = _owner.transform.position;
                aimLine.SetPosition(0, transform.position);
                if (_clock < _game.balance.rangedWindup) return;
                _direction = (_aim - transform.position).normalized; _flying = true; _clock = 0;
                aimLine.enabled = false; trail.emitting = true;
            }
            Vector3 previous = transform.position;
            transform.position += _direction * (_game.balance.projectileSpeed * Time.deltaTime);
            transform.Rotate(0, 0, 400 * Time.deltaTime);
            if (SegmentDistanceSquared(previous, transform.position, _game.Player.transform.position) < .62f * .62f)
            { _game.Player.Damage(_damage); gameObject.SetActive(false); return; }
            if (_clock > 5) gameObject.SetActive(false);
        }
        public static float SegmentDistanceSquared(Vector2 a, Vector2 b, Vector2 point)
        {
            var d = b - a; float t = d.sqrMagnitude > .00001f ? Mathf.Clamp01(Vector2.Dot(point - a, d) / d.sqrMagnitude) : 0;
            return (point - (a + d * t)).sqrMagnitude;
        }
        void OnDisable() { if (trail) { trail.emitting = false; trail.Clear(); } if (aimLine) aimLine.enabled = false; }
    }
}
