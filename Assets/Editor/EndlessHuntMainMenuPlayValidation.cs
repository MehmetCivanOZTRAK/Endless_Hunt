using System.IO;
using System.Linq;
using EndlessHunt;
using EndlessHunt.UI;
using EndlessHunt.UI.Screens;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[InitializeOnLoad]
public static class EndlessHuntMainMenuPlayValidation
{
    private const string RunningKey = "EndlessHunt.MainMenuValidation.Running";
    private const string ErrorKey = "EndlessHunt.MainMenuValidation.Errors";
    private const string ResolutionKey = "EndlessHunt.MainMenuValidation.Resolution";
    private const string ScenePath = "Assets/Scenes/MainMenu.unity";
    private const string CaptureFolder = "Logs/MainMenuPreviews";

    private static readonly Vector2Int[] Resolutions =
    {
        new(1080, 1920),
        new(1080, 2160),
        new(1080, 2340)
    };

    private static double _nextActionAt;
    private static bool _captureRequested;
    private static bool _carouselExercisePending;
    private static int _carouselOriginalIndex;
    private static int _carouselTargetIndex;

    static EndlessHuntMainMenuPlayValidation()
    {
        if (SessionState.GetBool(RunningKey, false)) Subscribe();
    }

    public static void RunResponsive()
    {
        string output = Path.Combine(
            Directory.GetParent(Application.dataPath).FullName,
            CaptureFolder.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(output);
        SessionState.SetBool(RunningKey, true);
        SessionState.SetInt(ErrorKey, 0);
        SessionState.SetInt(ResolutionKey, 0);
        Subscribe();
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        EditorApplication.isPlaying = true;
    }

    private static void Subscribe()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        Application.logMessageReceived -= OnLog;
        Application.logMessageReceived += OnLog;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (!SessionState.GetBool(RunningKey, false)) return;

        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            ValidateRuntime();
            BeginCarouselExercise();
            SetResolution(0);
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            EditorApplication.update -= Tick;
            int errors = SessionState.GetInt(ErrorKey, 0);
            SessionState.SetBool(RunningKey, false);
            Debug.Log("Main Menu responsive play validation finished with " + errors +
                      " runtime error(s). Captures: " + CaptureFolder + "/MainMenu_1080x1920.png, " +
                      "1080x2160.png, 1080x2340.png.");
            EditorApplication.Exit(errors == 0 ? 0 : 1);
        }
    }

    private static void SetResolution(int index)
    {
        SessionState.SetInt(ResolutionKey, index);
        Vector2Int resolution = Resolutions[index];
        Screen.SetResolution(resolution.x, resolution.y, false);
        _captureRequested = false;
        _nextActionAt = EditorApplication.timeSinceStartup + 2d;
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
    }

    private static void Tick()
    {
        if (EditorApplication.timeSinceStartup < _nextActionAt) return;

        int index = SessionState.GetInt(ResolutionKey, 0);
        Vector2Int resolution = Resolutions[index];
        if (!_captureRequested)
        {
            if (_carouselExercisePending) ValidateCarouselExerciseAndRestore();
            RenderCurrentMenu(resolution);
            _captureRequested = true;
            _nextActionAt = EditorApplication.timeSinceStartup + 1.25d;
            return;
        }

        int next = index + 1;
        if (next < Resolutions.Length)
        {
            SetResolution(next);
            return;
        }

        EditorApplication.update -= Tick;
        EditorApplication.isPlaying = false;
    }

    private static void RenderCurrentMenu(Vector2Int resolution)
    {
        UIRoot root = Object.FindFirstObjectByType<UIRoot>();
        if (root == null)
        {
            Debug.LogError("Runtime MainMenu capture requires UIRoot.");
            return;
        }

        Canvas canvas = root.GetComponent<Canvas>();
        RenderMode previousRenderMode = canvas.renderMode;
        Camera previousWorldCamera = canvas.worldCamera;
        float previousPlaneDistance = canvas.planeDistance;

        GameObject cameraObject = new("Main Menu Capture Camera", typeof(Camera));
        cameraObject.hideFlags = HideFlags.HideAndDontSave;
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.enabled = false;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.cullingMask = ~0;
        camera.nearClipPlane = .01f;
        camera.farClipPlane = 10f;
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 1f;

        var renderTexture = new RenderTexture(
            resolution.x, resolution.y, 24, RenderTextureFormat.ARGB32);
        var texture = new Texture2D(
            resolution.x, resolution.y, TextureFormat.RGBA32, false);
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture previousTarget = camera.targetTexture;
        camera.targetTexture = renderTexture;
        RenderTexture.active = renderTexture;
        Canvas.ForceUpdateCanvases();
        camera.Render();
        texture.ReadPixels(new Rect(0, 0, resolution.x, resolution.y), 0, 0);
        texture.Apply();
        AssertCaptureNotEmpty(texture, resolution);

        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string path = Path.Combine(
            projectRoot,
            CaptureFolder.Replace('/', Path.DirectorySeparatorChar),
            "MainMenu_" + resolution.x + "x" + resolution.y + ".png");
        File.WriteAllBytes(path, texture.EncodeToPNG());

        canvas.renderMode = previousRenderMode;
        canvas.worldCamera = previousWorldCamera;
        canvas.planeDistance = previousPlaneDistance;
        camera.targetTexture = previousTarget;
        RenderTexture.active = previousActive;
        Object.Destroy(texture);
        Object.Destroy(renderTexture);
        Object.Destroy(cameraObject);
    }

    private static void AssertCaptureNotEmpty(Texture2D texture, Vector2Int resolution)
    {
        Color32 baseline = texture.GetPixel(0, 0);
        int variedSamples = 0;
        const int columns = 12;
        const int rows = 20;
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                Color32 sample = texture.GetPixel(
                    x * (resolution.x - 1) / (columns - 1),
                    y * (resolution.y - 1) / (rows - 1));
                int difference = Mathf.Abs(sample.r - baseline.r) +
                                 Mathf.Abs(sample.g - baseline.g) +
                                 Mathf.Abs(sample.b - baseline.b);
                if (difference > 24) variedSamples++;
            }
        }

        Assert(variedSamples >= 24, "Runtime MainMenu capture is visually empty.");
    }

    private static void ValidateRuntime()
    {
        UIRoot[] roots = Object.FindObjectsByType<UIRoot>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        HomeScreenPresenter[] homes = Object.FindObjectsByType<HomeScreenPresenter>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        EventSystem[] eventSystems = Object.FindObjectsByType<EventSystem>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        Canvas[] activeCanvases = Object.FindObjectsByType<Canvas>(
                FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(canvas => canvas.gameObject.activeInHierarchy)
            .ToArray();

        Assert(roots.Length == 1, "Runtime MainMenu must contain one UIRoot.");
        Assert(homes.Length == 1, "Runtime MainMenu must contain one HomeScreen.");
        Assert(eventSystems.Length == 1, "Runtime MainMenu must contain one EventSystem.");
        Assert(activeCanvases.Length == 1, "Runtime MainMenu must contain one active Canvas.");
        if (roots.Length != 1 || homes.Length != 1) return;

        UIRoot root = roots[0];
        HomeScreenPresenter home = homes[0];
        Assert(home.IsConfigured, "Runtime HomeScreen references are incomplete.");
        Assert(home.gameObject.activeInHierarchy, "Runtime HomeScreen is not active.");
        Assert(root.ScreenManager.ActiveScreen == home.Screen,
            "UIScreenManager did not open Home as the initial screen.");
        Assert(home.GetComponentInChildren<Spine.Unity.SkeletonGraphic>(true) == null,
            "Runtime character preview must not exist on Home.");
        Assert(home.StageCarousel != null &&
               home.StageCarousel.CardCount == GameCatalog.MapNames.Length,
            "Runtime stage carousel does not match the real stage count.");
        Assert(home.PlayButton != null, "Runtime Play button is missing.");
        Assert(home.EnergyDisplay != null, "Runtime Energy display is missing.");

        foreach (var tab in home.BottomTabs)
            Assert(tab != null, "Runtime bottom navigation contains a missing tab.");
        if (home.BottomTabs.Length == 4 && home.BottomTabs.All(tab => tab != null))
        {
            Assert(home.BottomTabs[0].CurrentPreferredWidth > home.BottomTabs[1].CurrentPreferredWidth,
                "Runtime active Home tab is not expanded.");
        }

        Assert(home.SettingsButton != null, "Runtime Settings button is missing.");
        if (home.SettingsButton != null)
        {
            home.SettingsButton.onClick.Invoke();
            Assert(root.PopupManager.HasOpenPopup &&
                   root.PopupManager.TopPopup.PopupId == HomeScreenPresenter.SettingsPopupId,
                "Runtime Settings button did not open the UI Core popup.");
            Assert(root.PopupManager.TryCloseTopOnBack(false),
                "Runtime back action did not close the Settings popup.");
        }
    }

    private static void BeginCarouselExercise()
    {
        HomeScreenPresenter home = Object.FindFirstObjectByType<HomeScreenPresenter>();
        if (home == null || home.StageCarousel == null || home.StageCarousel.CardCount < 2) return;
        _carouselOriginalIndex = home.StageCarousel.SelectedIndex;
        _carouselTargetIndex = (_carouselOriginalIndex + 1) % home.StageCarousel.CardCount;
        _carouselExercisePending = home.StageCarousel.SelectIndex(_carouselTargetIndex, true);
    }

    private static void ValidateCarouselExerciseAndRestore()
    {
        _carouselExercisePending = false;
        HomeScreenPresenter home = Object.FindFirstObjectByType<HomeScreenPresenter>();
        if (home == null || home.StageCarousel == null) return;
        UIStageCarousel carousel = home.StageCarousel;
        Assert(!carousel.IsSnapping && carousel.SelectedIndex == _carouselTargetIndex,
            "Runtime stage carousel did not finish snap paging.");
        ScrollRect scrollRect = carousel.GetComponent<ScrollRect>();
        Assert(scrollRect != null &&
               Mathf.Abs(scrollRect.horizontalNormalizedPosition - carousel.TargetNormalizedPosition) < .02f,
            "Runtime stage carousel did not center the selected card.");
        Assert(home.StageNumberView != null &&
               home.StageNumberView.text == "BÖLÜM " + (_carouselTargetIndex + 1),
            "Runtime stage number did not update after swipe.");
        Assert(home.StageNameView != null &&
               home.StageNameView.text == GameCatalog.MapNames[_carouselTargetIndex].ToUpperInvariant(),
            "Runtime stage name did not update after swipe.");
        carousel.SelectIndex(_carouselOriginalIndex, false);
    }

    private static void Assert(bool condition, string message)
    {
        if (condition) return;
        Debug.LogError(message);
        SessionState.SetInt(ErrorKey, SessionState.GetInt(ErrorKey, 0) + 1);
    }

    private static void OnLog(string condition, string stackTrace, LogType type)
    {
        if (!SessionState.GetBool(RunningKey, false)) return;
        if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
        if (condition.StartsWith("Runtime MainMenu") ||
            condition.StartsWith("Runtime HomeScreen") ||
            condition.StartsWith("UIScreenManager") ||
            condition.StartsWith("Runtime equipped") ||
            condition.StartsWith("Runtime Play") ||
            condition.StartsWith("Runtime Energy") ||
            condition.StartsWith("Runtime bottom") ||
            condition.StartsWith("Runtime active") ||
            condition.StartsWith("Runtime character") ||
            condition.StartsWith("Runtime stage") ||
            condition.StartsWith("Runtime Settings") ||
            condition.StartsWith("Runtime back"))
            return;
        SessionState.SetInt(ErrorKey, SessionState.GetInt(ErrorKey, 0) + 1);
    }
}
