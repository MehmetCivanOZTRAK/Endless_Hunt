using System;
using UnityEngine;

namespace EndlessHunt.UI
{
    [DisallowMultipleComponent]
    public sealed class GameProfileUIAdapter : MonoBehaviour
    {
        public readonly struct Snapshot : IEquatable<Snapshot>
        {
            public readonly int Coins;
            public readonly int Gems;
            public readonly int Energy;
            public readonly int PlayerLevel;

            public Snapshot(int coins, int gems, int energy, int playerLevel)
            {
                Coins = coins;
                Gems = gems;
                Energy = energy;
                PlayerLevel = playerLevel;
            }

            public bool Equals(Snapshot other) => Coins == other.Coins && Gems == other.Gems &&
                                                  Energy == other.Energy && PlayerLevel == other.PlayerLevel;
        }

        private Snapshot _lastSnapshot;
        private bool _hasSnapshot;

        public event Action<Snapshot> ProfileChanged;

        public int GetCoins() => GameProfile.Coins;
        public int GetGems() => GameProfile.Gems;
        public int GetEnergy() => GameProfile.Energy;
        public int GetEnergyMaximum() => GameProfile.MaxEnergy;
        public string GetEnergyTimer() => GameProfile.EnergyTimer();
        public int GetPlayerLevel() => Mathf.Max(1, PlayerPrefs.GetInt("EH_PlayerLevel", 20));
        public int GetUpgradeLevel(int index) => Mathf.Max(1, PlayerPrefs.GetInt("EH2_Upgrade_" + index, 1));
        public bool Owns(string itemId) => !string.IsNullOrEmpty(itemId) && GameProfile.Owns(itemId);
        public string GetEquippedGearId(GearSlot slot) => GameProfile.Equipped(slot);
        public string GetEquippedWeaponId() => GameProfile.EquippedWeaponId;
        public int GetWeaponLevel(string weaponId) => GameProfile.WeaponLevel(weaponId);

        public Snapshot ReadSnapshot() => new(GetCoins(), GetGems(), GetEnergy(), GetPlayerLevel());

        public void Refresh(bool force = false)
        {
            Snapshot snapshot = ReadSnapshot();
            if (!force && _hasSnapshot && snapshot.Equals(_lastSnapshot)) return;
            _lastSnapshot = snapshot;
            _hasSnapshot = true;
            ProfileChanged?.Invoke(snapshot);
        }
    }
}
