using System;
using System.IO;
using System.Linq;
using EndlessHunt;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Rebuilds the menu and replaces only gameplay HUDs; keeps the existing world and pools.</summary>
public static class EndlessHuntMobilePolish
{
    [MenuItem("Endless Hunt/UI/Apply Mobile Survivor Layout")]
    public static void Apply()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before applying the layout.");
        string[] paths = { "Assets/Scenes/MainMenu.unity", "Assets/Scenes/GameScene.unity", "Assets/Scenes/GameScene2.unity" };
        Directory.CreateDirectory("Logs/MobilePolishBackup");
        foreach (string path in paths)
        {
            string backup = "Logs/MobilePolishBackup/" + Path.GetFileName(path);
            if (!File.Exists(backup)) File.Copy(path, backup);
        }
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.defaultScreenWidth = 1080; PlayerSettings.defaultScreenHeight = 1920;
        var character = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CharacterPrefabs/Character_20260827_211359.prefab");
        var weapons = AssetDatabase.FindAssets("t:WeaponDefinition", new[] { "Assets/EndlessHunt/Data/Weapons" })
            .Select(g => AssetDatabase.LoadAssetAtPath<WeaponDefinition>(AssetDatabase.GUIDToAssetPath(g))).OrderBy(w => w.id).ToArray();
        if (character == null || weapons.Length == 0) throw new MissingReferenceException("Character or weapon catalog missing.");
        Scene menu = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EndlessHuntMenuSceneBuilder.Build(character, weapons);
        var menuView = UnityEngine.Object.FindFirstObjectByType<SurvivorMenu>();
        menuView.homePage.SetActive(true); menuView.equipmentPage.SetActive(false);
        menuView.upgradePage.SetActive(false); menuView.storePage.SetActive(false);
        GameObject menuRoot = new GameObject("Mobile Menu");
        menuView.transform.SetParent(menuRoot.transform, false);
        menuView.homePage.GetComponentInParent<Canvas>().transform.SetParent(menuRoot.transform, false);
        PrefabUtility.SaveAsPrefabAssetAndConnect(menuRoot,"Assets/EndlessHunt/UI/Screens/UI_MobileMenu.prefab",InteractionMode.AutomatedAction);
        EditorSceneManager.SaveScene(menu, paths[0]);
        foreach (string path in paths.Skip(1))
        {
            Scene scene = EditorSceneManager.OpenScene(path);
            SurvivalGame game = UnityEngine.Object.FindFirstObjectByType<SurvivalGame>();
            if (game == null) throw new MissingReferenceException("SurvivalGame missing: " + path);
            string walkPath = "Assets/Enemy/Orc1/Orc1_walk/orc1_walk_full.png";
            game.enemyWalkFrames = AssetDatabase.LoadAllAssetsAtPath(walkPath).OfType<Sprite>()
                .OrderBy(s => int.TryParse(s.name.Substring(s.name.LastIndexOf('_') + 1), out int n) ? n : 0).ToArray();
            if (game.enemyWalkFrames.Length >= 24)
            {
                game.enemySprites = new[] { game.enemyWalkFrames[0] };
                // Measure visible pixels, excluding the transparent margin in the 64px sprite cells.
                Texture2D pixels = new Texture2D(2,2); pixels.LoadImage(File.ReadAllBytes(walkPath));
                Sprite first = game.enemyWalkFrames[0]; Rect rect = first.rect;
                int min = (int)rect.yMax, max = (int)rect.yMin;
                for(int y=(int)rect.yMin;y<(int)rect.yMax;y++)
                    for(int x=(int)rect.xMin;x<(int)rect.xMax;x++)
                        if(pixels.GetPixel(x,y).a > .1f) { min=Mathf.Min(min,y); max=Mathf.Max(max,y); }
                game.enemyVisibleHeight = (max-min+1) / first.pixelsPerUnit;
                game.enemyVisibleHeight = Mathf.Max(game.enemyVisibleHeight, game.enemyWalkFrames.Max(frame => frame.bounds.size.y));
                UnityEngine.Object.DestroyImmediate(pixels);
            }
            Canvas oldHud = game.healthBar.GetComponentInParent<Canvas>();
            UnityEngine.Object.DestroyImmediate(oldHud.gameObject);
            EndlessHuntGameSceneBuilder.BuildHud(game);
            EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene(paths[0]);
        Debug.Log("MOBILE_POLISH_APPLIED: menu, equipment, talents, HUD; existing maps preserved.");
    }
}
