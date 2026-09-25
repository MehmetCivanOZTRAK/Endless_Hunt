using System.IO;
using EndlessHunt;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class EndlessHuntMenuCapture
{
    [MenuItem("Endless Hunt/Capture Mobile Menu Preview")]
    public static void Capture()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        SurvivorMenu menu = Object.FindFirstObjectByType<SurvivorMenu>();
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        Camera camera = Camera.main;
        if (menu == null || canvas == null || camera == null)
            throw new MissingReferenceException("MainMenu capture references are missing.");

        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 1f;
        menu.confirmationPanel.SetActive(false);
        menu.toastPanel.SetActive(false);

        menu.homePage.SetActive(true);
        menu.equipmentPage.SetActive(false);
        menu.upgradePage.SetActive(false);
        menu.storePage.SetActive(false);
        CharacterSkin.Apply(menu.battlePreview, menu.weapons);
        Render(camera, "CodexPreviews/codex-battle-preview.png");

        menu.homePage.SetActive(false);
        menu.equipmentPage.SetActive(true);
        CharacterSkin.Apply(menu.equipmentPreview, menu.weapons);
        Render(camera, "CodexPreviews/codex-equipment-preview.png");
        Debug.Log("Mobile menu previews captured.");
    }

    private static void Render(Camera camera, string relativePath)
    {
        const int width = 540;
        const int height = 960;
        RenderTexture renderTexture = new(width, height, 24, RenderTextureFormat.ARGB32);
        Texture2D texture = new(width, height, TextureFormat.RGBA32, false);
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture previousTarget = camera.targetTexture;
        camera.targetTexture = renderTexture;
        RenderTexture.active = renderTexture;
        Canvas.ForceUpdateCanvases();
        camera.Render();
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string path = Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllBytes(path, texture.EncodeToPNG());
        camera.targetTexture = previousTarget;
        RenderTexture.active = previousActive;
        Object.DestroyImmediate(texture);
        Object.DestroyImmediate(renderTexture);
    }
}
