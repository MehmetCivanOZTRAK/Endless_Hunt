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

    [MenuItem("Endless Hunt/Ensure Mobile Game And UI Core")]
    public static void BuildProject()
    {
        BuildProjectInternal(false);
    }

    [MenuItem("Endless Hunt/Advanced/Rebuild Gameplay Scenes (Destructive)")]
    public static void RebuildGameplayScenesDestructive()
    {
        if (Application.isBatchMode)
            throw new BuildFailedException("Destructive scene rebuild is disabled in batch mode.");
        if (!EditorUtility.DisplayDialog("Rebuild Gameplay Scenes?",
                "This replaces MainMenu, GameScene and GameScene2. Use only when intentional. UI Core prefab assets remain safe.",
                "Rebuild Scenes", "Cancel")) return;
        BuildProjectInternal(true);
    }

    private static void BuildProjectInternal(bool overwriteScenes)
    {
        SkeletonDataAsset character = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(CharacterPath);
        if (character == null)
            throw new MissingReferenceException("LayerLab SkeletonDataAsset bulunamadı: " + CharacterPath);
        GameObject startPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(StartPrefabPath);
        if (startPrefab == null) throw new MissingReferenceException("Başlangıç karakter prefabı bulunamadı: " + StartPrefabPath);

        ConfigureMobilePlayer();
        WeaponDefinition[] weapons = EndlessHuntWeaponAssetBuilder.BuildAll();
        Sprite[] enemies = FindEnemySprites();
        EndlessHuntUICoreBuilder.BuildAssets();

        bool menuChanged = EnsureMenuScene("Assets/Scenes/MainMenu.unity", startPrefab, weapons, overwriteScenes);
        bool gameChanged = EnsureGameScene("Assets/Scenes/GameScene.unity", startPrefab, weapons, enemies, -1, overwriteScenes);
        bool gameTwoChanged = EnsureGameScene("Assets/Scenes/GameScene2.unity", startPrefab, weapons, enemies, 1, overwriteScenes);
        EnsureBuildScenes("Assets/Scenes/MainMenu.unity", "Assets/Scenes/GameScene.unity",
            "Assets/Scenes/GameScene2.unity");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        if (menuChanged || gameChanged || gameTwoChanged)
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);
        Debug.Log(overwriteScenes
            ? "Endless Hunt explicit destructive gameplay-scene rebuild complete. UI Core prefab assets were preserved."
            : "Endless Hunt safe ensure complete. Existing gameplay scenes were preserved; UI Core assets are ready.");
    }

    private static bool EnsureMenuScene(string path, GameObject startPrefab, WeaponDefinition[] weapons, bool overwrite)
    {
        if (!overwrite && AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null)
        {
            Debug.Log("Preserved existing scene: " + path);
            return false;
        }
        Scene menuScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EndlessHuntMenuSceneBuilder.Build(startPrefab, weapons);
        EditorSceneManager.SaveScene(menuScene, path);
        return true;
    }

    private static bool EnsureGameScene(string path, GameObject startPrefab, WeaponDefinition[] weapons,
        Sprite[] enemies, int forcedMap, bool overwrite)
    {
        if (!overwrite && AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null)
        {
            Debug.Log("Preserved existing scene: " + path);
            return false;
        }
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EndlessHuntGameSceneBuilder.Build(startPrefab, weapons, enemies, forcedMap);
        EditorSceneManager.SaveScene(scene, path);
        return true;
    }

    private static void EnsureBuildScenes(params string[] requiredPaths)
    {
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
        foreach (string path in requiredPaths)
        {
            EditorBuildSettingsScene existing = scenes.FirstOrDefault(item => item.path == path);
            if (existing == null) scenes.Add(new EditorBuildSettingsScene(path, true));
            else existing.enabled = true;
        }
        EditorBuildSettings.scenes = scenes.ToArray();
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
