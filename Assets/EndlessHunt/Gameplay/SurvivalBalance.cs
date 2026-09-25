using UnityEngine;
namespace EndlessHunt
{
    [CreateAssetMenu(menuName = "Endless Hunt/Survival Balance")]
    public sealed class SurvivalBalance : ScriptableObject
    {
        [Header("Progression")]
        public int firstRewardCost = 12;
        public float rewardGrowth = 1.20f;
        public int rewardLinearGrowth = 8, rewardQuadraticGrowth = 2, extraWeaponCost = 4;
        public float minimumRewardInterval = 10;
        [Header("Waves and crowd")]
        public float waveSeconds = 35;
        public int initialPopulation = 18, populationPerWave = 9, maximumPopulation = 120;
        public float initialSpawnInterval = 1.3f, minimumSpawnInterval = .5f;
        public float bodyRadius = .56f, spawnMargin = 2.5f;
        [Header("Movement and melee")]
        public float playerSpeed = 3.8f, enemySpeed = 1.55f, maximumEnemySpeed = 3.25f;
        public float contactDamage = 8, damageGrowth = 1.15f;
        public float attackRange = 1.2f, attackWindup = .35f, attackCooldown = 1.05f;
        [Header("Ranged enemies")]
        public int firstRangedWave = 4, maximumRanged = 10;
        public float rangedCooldown = 6, rangedWindup = .8f, projectileSpeed = 7;
        public float projectileDamageMultiplier = 2.5f, rangedDistance = 7.5f;
        public int RewardCost(int rewardsTaken, int weaponsUnlocked)
        {
            int n = Mathf.Clamp(rewardsTaken, 0, 100);
            return Mathf.CeilToInt(firstRewardCost * Mathf.Pow(rewardGrowth, n) + rewardLinearGrowth * n +
                rewardQuadraticGrowth * n * n + extraWeaponCost * Mathf.Max(0, weaponsUnlocked - 1));
        }
        public int Population(int wave) => Mathf.Min(maximumPopulation, initialPopulation + Mathf.Max(0, wave - 1) * populationPerWave);
        public int RangedPopulation(int wave) => wave < firstRangedWave ? 0 : Mathf.Min(maximumRanged, 1 + (wave - firstRangedWave) / 2);
        public float SpawnInterval(int wave) => Mathf.Max(minimumSpawnInterval, initialSpawnInterval - .055f * (wave - 1));
        public float EnemySpeed(int wave, int type) => Mathf.Min(maximumEnemySpeed, enemySpeed * Mathf.Pow(1.025f, Mathf.Max(0, wave - 1)) * (type == 1 ? 1.12f : type == 2 ? .9f : 1));
        public float Damage(int wave, bool elite = false) => contactDamage * Mathf.Pow(damageGrowth, Mathf.Clamp(wave - 1, 0, 100)) * (elite ? 1.6f : 1);
    }
}
