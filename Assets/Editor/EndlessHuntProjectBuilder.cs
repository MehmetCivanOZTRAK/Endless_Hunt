using System.Collections.Generic;
using System.Linq;
using EndlessHunt;
using Spine.Unity;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class EndlessHuntProjectBuilder
{
    private const string CharacterPath =
        "Assets/Layer Lab/2D Art Maker/AMCasual Character/Demo/SpineAnimation/Casual Character_SkeletonData.asset";
    private const string StartPrefabPath = "Assets/CharacterPrefabs/Character_20260827_211359.prefab";

    [MenuItem("Endless Hunt/Rebuild Hierarchy-Based Mobile Game")]
    public static void BuildProject()
    {
        SkeletonDataAsset character = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(CharacterPath);
        if (character == null)
            throw new MissingReferenceException("LayerLab SkeletonDataAsset bulunamadı: " + CharacterPath);
        GameObject startPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(StartPrefabPath);
        if (startPrefab == null) throw new MissingReferenceException("Başlangıç karakter prefabı bulunamadı: " + StartPrefabPath);

        ConfigureMobilePlayer();
        WeaponDefinition[] weapons = EndlessHuntWeaponAssetBuilder.BuildAll();
        Sprite[] enemies = FindEnemySprites();

        Scene menuScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EndlessHuntMenuSceneBuilder.Build(startPrefab, weapons);
        EditorSceneManager.SaveScene(menuScene, "Assets/Scenes/MainMenu.unity");

        BuildGameScene("Assets/Scenes/GameScene.unity", startPrefab, weapons, enemies, -1);
        BuildGameScene("Assets/Scenes/GameScene2.unity", startPrefab, weapons, enemies, 1);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/GameScene.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/GameScene2.unity", true)
        };

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);
        Debug.Log("Endless Hunt hierarchy rebuild complete: Vector UI, Joystick Pack, LayerLab gear icons, " +
                  "40 weapon assets, LayerLab map chunks, 120 enemies and 100 XP objects are prepared in scenes.");
    }

    private static void BuildGameScene(string path, GameObject startPrefab, WeaponDefinition[] weapons, Sprite[] enemies, int forcedMap)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EndlessHuntGameSceneBuilder.Build(startPrefab, weapons, enemies, forcedMap);
        EditorSceneManager.SaveScene(scene, path);
    }

    private static void ConfigureMobilePlayer()
    {
        PlayerSettings.companyName = "Endless Hunt Studio";
        PlayerSettings.productName = "Endless Hunt";
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.endlesshunt.game");
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.MTRendering = true;
        PlayerSettings.colorSpace = ColorSpace.Gamma;
    }

    private static Sprite[] FindEnemySprites()
    {
        string[] roots =
        {
            "Assets/Enemy", "Assets/DUSMAN_ASSETS", "Assets/DUSMAN_ASSETS2"
        };
        List<Sprite> sprites = new();
        foreach (string root in roots)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Sprite", new[] { root }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null) sprites.Add(sprite);
                if (sprites.Count >= 6) return sprites.ToArray();
            }
        }
        return sprites.Distinct().Take(6).ToArray();
    }
}
