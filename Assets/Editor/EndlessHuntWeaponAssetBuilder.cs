using EndlessHunt;
using UnityEditor;
using UnityEngine;

internal static class EndlessHuntWeaponAssetBuilder
{
    private const string Folder = "Assets/EndlessHunt/Data/Weapons";
    internal static WeaponDefinition[] BuildAll()
    {
        EnsureFolder("Assets/EndlessHunt", "Data");
        EnsureFolder("Assets/EndlessHunt/Data", "Weapons");
        WeaponDefinition[] result = new WeaponDefinition[40];
        string[] prefixes = { "Taş", "Avcı", "Demir", "Muhafız", "Fırtına", "Kristal", "Kadim", "Efsane" };
        for (int i = 1; i <= result.Length; i++)
        {
            string path = $"{Folder}/Weapon_{i:00}.asset";
            WeaponDefinition weapon = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(path);
            if (weapon == null) { weapon = ScriptableObject.CreateInstance<WeaponDefinition>(); AssetDatabase.CreateAsset(weapon, path); }
            weapon.id = $"weapon_{i:00}";
            weapon.title = $"{prefixes[(i - 1) / 5]} Silahı {i:00}";
            weapon.spineSkin = $"gear_right/gear_right_c_{i}";
            weapon.icon = EndlessHuntSceneFactory.Sprite($"Assets/Layer Lab/2D Art Maker/AMCasual Character/Demo/PartsThumbnail/gear_right/gear_right_c_{i}.png");
            weapon.price = i == 3 ? 0 : 90 + i * 35;
            weapon.baseDamage = 12f + i * 2.25f;
            weapon.attackRange = 1.45f + (i % 5) * .09f;
            weapon.attackCooldown = Mathf.Max(.42f, .92f - (i % 6) * .055f);
            weapon.hitMoment = .3f;
            weapon.attackAnimation = "Attack" + (((i - 1) % 3) + 1);
            EditorUtility.SetDirty(weapon);
            result[i - 1] = weapon;
        }
        AssetDatabase.SaveAssets();
        return result;
    }

    private static void EnsureFolder(string parent, string name)
    {
        string path = parent + "/" + name;
        if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, name);
    }
}
