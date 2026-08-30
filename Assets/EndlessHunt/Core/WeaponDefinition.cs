using UnityEngine;

namespace EndlessHunt
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "Endless Hunt/Weapon Definition")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        public string id;
        public string title;
        public string spineSkin;
        public Sprite icon;
        [Min(0)] public int price;
        [Min(1f)] public float baseDamage = 18f;
        [Min(.5f)] public float attackRange = 1.7f;
        [Min(.15f)] public float attackCooldown = .8f;
        [Range(.05f, .9f)] public float hitMoment = .32f;
        public string attackAnimation = "Attack1";

        public float DamageAtLevel(int level) => baseDamage * (1f + Mathf.Max(0, level - 1) * .18f);
    }
}
