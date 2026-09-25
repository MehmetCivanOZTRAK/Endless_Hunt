using UnityEngine;

namespace EndlessHunt
{
    public static class MobileRuntime
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Configure()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Screen.orientation = ScreenOrientation.Portrait;
            QualitySettings.vSyncCount = 0;
            Input.multiTouchEnabled = true;
        }
    }
}
