using UnityEngine;

namespace EndlessHunt
{
    public sealed class MeleeWeapon : MonoBehaviour
    {
        private SurvivalGame _game;
        private SurvivalPlayer _player;
        private WeaponDefinition _weapon;
        private SurvivalEnemy _target;
        private float _cooldown;
        private float _hitClock;
        private bool _hitPending;
        private float _speedMultiplier = 1f;

        public void Setup(SurvivalGame game, SurvivalPlayer player, WeaponDefinition weapon)
        {
            _game = game; _player = player; _weapon = weapon;
        }

        public void MultiplyAttackSpeed(float value) => _speedMultiplier *= value;
        public void MultiplyDamage(float value) => _game.MeleeDamageMultiplier *= value;

        private void Update()
        {
            if (_game == null || _weapon == null || _player.IsDead) return;
            _cooldown -= Time.deltaTime;
            if (_hitPending)
            {
                _hitClock -= Time.deltaTime;
                if (_hitClock <= 0f)
                {
                    _hitPending = false;
                    if (_target != null && _target.gameObject.activeSelf &&
                        Vector2.Distance(transform.position, _target.transform.position) <= _weapon.attackRange * 1.25f)
                        _target.Hit(_game.Damage);
                }
            }
            if (_cooldown > 0f || _hitPending) return;
            _target = FindNearest();
            if (_target == null) return;
            Vector2 direction = _target.transform.position - transform.position;
            if (direction.sqrMagnitude > _weapon.attackRange * _weapon.attackRange) return;
            _player.Face(direction.x);
            float duration = Mathf.Max(.25f, _weapon.attackCooldown / _speedMultiplier);
            _player.PlayAttack(_weapon.attackAnimation, duration);
            _hitClock = duration * _weapon.hitMoment;
            _hitPending = true;
            _cooldown = duration;
        }

        private SurvivalEnemy FindNearest()
        {
            SurvivalEnemy target = null; float best = float.MaxValue;
            foreach (SurvivalEnemy enemy in _game.Enemies)
            {
                if (enemy == null || !enemy.gameObject.activeSelf) continue;
                float distance = (enemy.transform.position - transform.position).sqrMagnitude;
                if (distance < best) { best = distance; target = enemy; }
            }
            return target;
        }
    }
}
