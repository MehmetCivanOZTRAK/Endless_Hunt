using System;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace EndlessHunt
{
    public static class CharacterSkin
    {
        public static void Apply(SkeletonAnimation view, WeaponDefinition[] weapons = null)
        {
            if (view == null) return;
            if (view.Skeleton == null) view.Initialize(false);
            if (view.Skeleton == null || view.AnimationState == null) return;
            Apply(view.Skeleton, view.AnimationState, weapons);
        }

        public static void Apply(SkeletonGraphic view, WeaponDefinition[] weapons = null)
        {
            if (view == null) return;
            if (view.Skeleton == null) view.Initialize(false);
            SkeletonAnimation animation = view.Animation as SkeletonAnimation;
            if (view.Skeleton == null || animation == null || animation.AnimationState == null) return;
            Apply(view.Skeleton, animation.AnimationState, weapons);
        }

        public static WeaponDefinition FindWeapon(WeaponDefinition[] weapons, string id)
        {
            if (weapons == null) return null;
            foreach (WeaponDefinition weapon in weapons) if (weapon != null && weapon.id == id) return weapon;
            return weapons.Length > 0 ? weapons[0] : null;
        }

        private static void Apply(Skeleton skeleton, Spine.AnimationState animationState, WeaponDefinition[] weapons)
        {
            var combined = new Skin("endless-hunt-loadout");
            Add(combined, skeleton, "skin/skin_1"); Add(combined, skeleton, "back/back_c_6");
            Add(combined, skeleton, "eyes/eyes_c_6"); Add(combined, skeleton, "brow/brow_c_1");
            Add(combined, skeleton, "mouth/mouth_c_1"); Add(combined, skeleton, "gloves/gloves_c_27");
            Add(combined, skeleton, "eyewear/eyewear_c_27"); Add(combined, skeleton, "gear_left/gear_left_c_34");
            Add(combined, skeleton, GameProfile.FindEquipped(GearSlot.Hair)?.spineSkin);
            Add(combined, skeleton, GameProfile.FindEquipped(GearSlot.Top)?.spineSkin);
            Add(combined, skeleton, GameProfile.FindEquipped(GearSlot.Bottom)?.spineSkin);
            Add(combined, skeleton, GameProfile.FindEquipped(GearSlot.Boots)?.spineSkin);
            Add(combined, skeleton, GameProfile.FindEquipped(GearSlot.Helmet)?.spineSkin);
            WeaponDefinition weapon = FindWeapon(weapons, GameProfile.EquippedWeaponId);
            Add(combined, skeleton, weapon != null ? weapon.spineSkin : "gear_right/gear_right_c_3");
            skeleton.SetSkin(combined); skeleton.SetupPoseSlots(); animationState.Apply(skeleton);
            ApplySlotColor(skeleton, new Color(.9401f,.8984f,.5070f,1f), "hair", "helmet_hair");
            ApplySlotColor(skeleton, new Color(.9824f,.5849f,.2670f,1f), "brow");
            ApplySlotColor(skeleton, new Color(.9786f,.7749f,.6157f,1f), "body", "leg_l", "leg_r", "arm_l", "arm_r", "head");
        }

        private static void Add(Skin combined, Skeleton skeleton, string skinName)
        {
            if (string.IsNullOrWhiteSpace(skinName)) return;
            Skin source = skeleton.Data.FindSkin(skinName);
            if (source != null) combined.AddSkin(source);
        }

        private static void ApplySlotColor(Skeleton skeleton, Color color, params string[] prefixes)
        {
            foreach (Slot slot in skeleton.Slots)
            {
                foreach (string prefix in prefixes)
                {
                    if (!slot.Data.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;
                    slot.SetColor(color);
                    break;
                }
            }
        }
    }
}
