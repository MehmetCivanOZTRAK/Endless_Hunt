using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class EndlessHuntPlaySmokeRunner
{
    private const string RunningKey = "EndlessHunt.SmokeRunning";
    private const string ErrorKey = "EndlessHunt.SmokeErrors";
    private const string CaptureKey = "EndlessHunt.CaptureMenu";
    private const string CapturedKey = "EndlessHunt.MenuCaptured";
    private static double _startedAt;

    static EndlessHuntPlaySmokeRunner()
    {
        if (SessionState.GetBool(RunningKey, false)) Subscribe();
    }

    public static void Run()
    {
        RunScene("Assets/Scenes/GameScene.unity");
    }

    public static void RunMenu()
    {
        RunScene("Assets/Scenes/MainMenu.unity");
    }

    public static void CaptureMenu()
    {
        SessionState.SetBool(CaptureKey, true);
        SessionState.SetBool(CapturedKey, false);
        RunScene("Assets/Scenes/MainMenu.unity");
    }

    private static void RunScene(string scenePath)
    {
        SessionState.SetBool(RunningKey, true);
        SessionState.SetInt(ErrorKey, 0);
        Subscribe();
        EditorSceneManager.OpenScene(scenePath);
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
            _startedAt = EditorApplication.timeSinceStartup;
            if (SessionState.GetBool(CaptureKey, false)) Screen.SetResolution(540, 960, false);
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            EditorApplication.update -= Tick;
            int errors = SessionState.GetInt(ErrorKey, 0);
            SessionState.SetBool(RunningKey, false);
            SessionState.SetBool(CaptureKey, false);
            Debug.Log($"Endless Hunt Play Mode smoke test finished with {errors} runtime error(s).");
            EditorApplication.Exit(errors == 0 ? 0 : 1);
        }
    }

    private static void Tick()
    {
        double elapsed = EditorApplication.timeSinceStartup - _startedAt;
        if (SessionState.GetBool(CaptureKey, false) && !SessionState.GetBool(CapturedKey, false) && elapsed >= 3d)
        {
            ScreenCapture.CaptureScreenshot("Temp/codex-menu-preview.png", 1);
            SessionState.SetBool(CapturedKey, true);
        }
        if (elapsed < 7d) return;
        EditorApplication.update -= Tick;
        EditorApplication.isPlaying = false;
    }

    private static void OnLog(string condition, string stackTrace, LogType type)
    {
        if (!SessionState.GetBool(RunningKey, false)) return;
        if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
        SessionState.SetInt(ErrorKey, SessionState.GetInt(ErrorKey, 0) + 1);
    }
}
