using System;
using UnityEngine;

namespace EndlessHunt
{
    public enum GearSlot { Top, Bottom, Helmet, Hair, Boots }

    [Serializable]
    public sealed class GearDefinition
    {
        public string id;
        public string title;
        public GearSlot slot;
        public int price;
        public string spineSkin;
        public Color color;

        public GearDefinition(string id, string title, GearSlot slot, int price, string spineSkin, Color color)
        {
            this.id = id; this.title = title; this.slot = slot; this.price = price;
            this.spineSkin = spineSkin; this.color = color;
        }
    }

    public static class GameCatalog
    {
        private static readonly GearDefinition[] StarterGear =
        {
            new("top_start", "Kamp Tişörtü", GearSlot.Top, 0, "top/top_c_47", new Color(.22f,.78f,.95f)),
            new("top_scout", "İzci Ceketi", GearSlot.Top, 260, "top/top_c_16", new Color(.2f,.86f,.48f)),
            new("top_guardian", "Muhafız Zırhı", GearSlot.Top, 700, "top/top_c_46", new Color(1f,.62f,.15f)),
            new("bottom_start", "Yırtık Pantolon", GearSlot.Bottom, 0, "bottom/bottom_c_16", new Color(.2f,.55f,.9f)),
            new("bottom_shadow", "Gölge Altlığı", GearSlot.Bottom, 320, "bottom/bottom_c_18", new Color(.56f,.36f,.95f)),
            new("helmet_none", "Açık Baş", GearSlot.Helmet, 0, "", new Color(.45f,.5f,.58f)),
            new("helmet_guard", "Gözcü Başlığı", GearSlot.Helmet, 380, "helmet/helmet_c_7", new Color(.95f,.4f,.2f)),
            new("helmet_crown", "Taçlı Miğfer", GearSlot.Helmet, 1000, "helmet/helmet_c_49", new Color(1f,.78f,.18f)),
            new("hair_start", "Kampçı Saçı", GearSlot.Hair, 0, "hair_short/hair_short_c_24", new Color(.35f,.7f,1f)),
            new("hair_wild", "Vahşi Stil", GearSlot.Hair, 180, "hair_short/hair_short_c_12", new Color(.25f,.9f,.62f)),
            new("boots_start", "Kamp Botu", GearSlot.Boots, 0, "boots/boots_c_23", new Color(.32f,.68f,.95f)),
            new("boots_dash", "Atılma Botu", GearSlot.Boots, 300, "boots/boots_c_15", new Color(.95f,.4f,.28f))
        };

        public static readonly GearDefinition[] Gear = BuildGear();
        private static GearDefinition[] BuildGear()
        {
            var result = new System.Collections.Generic.List<GearDefinition>(StarterGear);
            string[] categories = { "top", "bottom", "helmet", "hair_short", "boots" };
            string[] labels = { "Üstlük", "Altlık", "Başlık", "Saç", "Bot" };
            GearSlot[] slots = { GearSlot.Top, GearSlot.Bottom, GearSlot.Helmet, GearSlot.Hair, GearSlot.Boots };
            int[] counts = { 60, 50, 60, 30, 30 };
            for (int c = 0; c < categories.Length; c++)
                for (int i = 1; i <= counts[c]; i++)
                {
                    string skin = categories[c] + "/" + categories[c] + "_c_" + i;
                    if (result.Exists(g => g.spineSkin == skin)) continue;
                    result.Add(new GearDefinition(categories[c] + "_" + i, labels[c] + " " + i.ToString("00"), slots[c], i * 15, skin, new Color(.3f,.65f,.85f)));
                }
            return result.ToArray();
        }

        public static readonly string[] MapNames = { "Yeşil Kamp", "Kızıl Kanyon", "Gece Limanı", "Buz Diyarı", "Neon Çoraklık" };
        public static readonly Color[] MapGround =
        {
            new(.32f,.52f,.28f), new(.38f,.19f,.13f), new(.08f,.17f,.26f), new(.48f,.7f,.76f), new(.16f,.08f,.25f)
        };
    }

    public static class GameProfile
    {
        private static string Prefix => LocalPlayerStore.WalletPrefix;
        public const int MaxEnergy = 20;
        public const int BattleEnergyCost = 5;
        public const string StartingWeaponId = "weapon_03";
        public static int Coins { get => PlayerPrefs.GetInt(Prefix + "Coins", 800); set => SetInt("Coins", Mathf.Max(0, value)); }
        public static int Gems { get => PlayerPrefs.GetInt(Prefix + "Gems", 1200); set => SetInt("Gems", Mathf.Max(0, value)); }
        public static int Energy { get { RefreshEnergy(); return PlayerPrefs.GetInt(Prefix + "Energy", MaxEnergy); } }
        public static int HighestStage { get => PlayerPrefs.GetInt(Prefix + "Stage", 1); set => SetInt("Stage", Mathf.Clamp(value, 1, GameCatalog.MapNames.Length)); }
        public static int BestScore { get => PlayerPrefs.GetInt(Prefix + "Best", 0); set => SetInt("Best", Mathf.Max(value, BestScore)); }
        public static int SelectedStage { get => PlayerPrefs.GetInt(Prefix + "SelectedStage", 0); set => SetInt("SelectedStage", Mathf.Clamp(value, 0, GameCatalog.MapNames.Length - 1)); }

        // Preview release: every catalog item is available without spending currency.
        public static bool UnlockAllEquipment => true;
        public static bool Owns(string id) => UnlockAllEquipment || PlayerPrefs.GetInt(Prefix + "Own_" + id, DefaultOwned(id) ? 1 : 0) == 1;
        public static void Own(string id) { PlayerPrefs.SetInt(Prefix + "Own_" + id, 1); PlayerPrefs.Save(); }
        private static bool DefaultOwned(string id) => id == StartingWeaponId || id.EndsWith("_start") || id == "helmet_none";

        public static string Equipped(GearSlot slot)
        {
            return PlayerPrefs.GetString(Prefix + "BareEquip_" + slot, "");
        }
        public static void Equip(GearDefinition gear) { PlayerPrefs.SetString(Prefix + "BareEquip_" + gear.slot, gear.id); PlayerPrefs.Save(); }
        public static void Unequip(GearSlot slot) { PlayerPrefs.SetString(Prefix + "BareEquip_" + slot, ""); PlayerPrefs.Save(); }
        public static float GearHealthBonus => GearBonus(GearSlot.Top, 20f) + GearBonus(GearSlot.Helmet, 10f) + GearBonus(GearSlot.Bottom, 10f);
        public static float GearSpeedBonus => GearBonus(GearSlot.Boots, .12f);
        private static float GearBonus(GearSlot slot, float value)
        {
            GearDefinition gear = FindEquipped(slot);
            return gear == null || string.IsNullOrEmpty(gear.spineSkin) ? 0f : value * (1f + gear.price / 1000f);
        }
        public static GearDefinition FindEquipped(GearSlot slot) => Array.Find(GameCatalog.Gear, x => x.id == Equipped(slot));
        public static bool Buy(GearDefinition gear)
        {
            if (!Owns(gear.id))
            {
                if (Gems < gear.price) return false;
                Gems -= gear.price; Own(gear.id);
            }
            Equip(gear); return true;
        }

        public static string EquippedWeaponId { get => PlayerPrefs.GetString(Prefix + "EquippedWeapon", StartingWeaponId); set { PlayerPrefs.SetString(Prefix + "EquippedWeapon", value); PlayerPrefs.Save(); } }
        public static int WeaponLevel(string id) => PlayerPrefs.GetInt(Prefix + "WeaponLevel_" + id, 1);
        public static bool BuyWeapon(WeaponDefinition weapon)
        {
            if (!Owns(weapon.id))
            {
                if (Gems < weapon.price) return false;
                Gems -= weapon.price; Own(weapon.id);
            }
            EquippedWeaponId = weapon.id; return true;
        }
        public static int WeaponUpgradeCost(WeaponDefinition weapon) => Mathf.RoundToInt((120 + weapon.baseDamage * 5f) * WeaponLevel(weapon.id));
        public static bool UpgradeWeapon(WeaponDefinition weapon)
        {
            if (!Owns(weapon.id)) return false;
            int cost = WeaponUpgradeCost(weapon);
            if (Coins < cost) return false;
            Coins -= cost;
            PlayerPrefs.SetInt(Prefix + "WeaponLevel_" + weapon.id, WeaponLevel(weapon.id) + 1);
            PlayerPrefs.Save(); return true;
        }

        public static bool SpendEnergy(int amount)
        {
            RefreshEnergy(); int current = PlayerPrefs.GetInt(Prefix + "Energy", MaxEnergy);
            if (current < amount) return false;
            PlayerPrefs.SetInt(Prefix + "Energy", current - amount);
            if (!PlayerPrefs.HasKey(Prefix + "EnergyTime")) PlayerPrefs.SetString(Prefix + "EnergyTime", DateTime.UtcNow.Ticks.ToString());
            PlayerPrefs.Save(); return true;
        }
        public static void AddEnergy(int amount) { RefreshEnergy(); SetInt("Energy", Mathf.Min(9999, Energy + Mathf.Max(0,amount))); }
        public static string EnergyTimer()
        {
            RefreshEnergy(); if (Energy >= MaxEnergy) return "DOLU";
            var left = TimeSpan.FromMinutes(10) - (DateTime.UtcNow - new DateTime(ReadTicks(), DateTimeKind.Utc));
            if (left.TotalSeconds < 0) left = TimeSpan.Zero;
            return $"{left.Minutes:00}:{left.Seconds:00}";
        }
        private static void RefreshEnergy()
        {
            int energy = PlayerPrefs.GetInt(Prefix + "Energy", MaxEnergy); if (energy >= MaxEnergy) return;
            var elapsed = DateTime.UtcNow - new DateTime(ReadTicks(), DateTimeKind.Utc);
            int gained = Mathf.FloorToInt((float)elapsed.TotalMinutes / 10f); if (gained <= 0) return;
            PlayerPrefs.SetInt(Prefix + "Energy", Mathf.Min(MaxEnergy, energy + gained));
            PlayerPrefs.SetString(Prefix + "EnergyTime", DateTime.UtcNow.AddMinutes(-(elapsed.TotalMinutes % 10)).Ticks.ToString());
            PlayerPrefs.Save();
        }
        private static long ReadTicks() => long.TryParse(PlayerPrefs.GetString(Prefix + "EnergyTime", DateTime.UtcNow.Ticks.ToString()), out long ticks) ? ticks : DateTime.UtcNow.Ticks;
        private static void SetInt(string key, int value) { PlayerPrefs.SetInt(Prefix + key, value); PlayerPrefs.Save(); }
    }
}
