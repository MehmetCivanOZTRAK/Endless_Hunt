using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Callbacks;
using UnityEngine;

namespace LayerLab.ArtMaker.EditorTools
{
    [InitializeOnLoad]
    internal static class SpineRuntimeSetup
    {
        internal const string SpineDefine = "LAYERLAB_SPINE_RUNTIME_AVAILABLE";

        private static readonly string[] LegacySpineDefines =
        {
            "LAYERLAB_SPINE",
            "LAYERLAB_SPINE_RUNTIME"
        };
        private const string SessionKey = "LayerLab.ArtMaker.SpineRuntimeNotice.ShownThisSession";
        private const string PreferenceKeyPrefix = "LayerLab.ArtMaker.SpineRuntimeNotice.DontShowAgain.v1.";
        private const string SpineAssemblyName = "spine-unity";
        private const string SpineTypeName = "Spine.Unity.SkeletonAnimation";

        static SpineRuntimeSetup()
        {
            EditorApplication.delayCall += Initialize;
        }

        private static void Initialize()
        {
            bool hasSpineRuntimeAssets = HasSpineRuntimeAssets();
            bool isSpineInstalled = hasSpineRuntimeAssets && IsSpineRuntimeTypeLoaded();

            if (SynchronizeScriptingDefine(hasSpineRuntimeAssets))
            {
                return;
            }

            if (!isSpineInstalled)
            {
                ShowNoticeOncePerSession();
            }
        }

        internal static bool IsSpineRuntimeInstalled()
        {
            return HasSpineRuntimeAssets() && IsSpineRuntimeTypeLoaded();
        }

        private static bool IsSpineRuntimeTypeLoaded()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetType(SpineTypeName, false) != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasSpineRuntimeAssets()
        {
            foreach (string path in AssetDatabase.GetAllAssetPaths())
            {
                if (IsSpineRuntimeAssetPath(path))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool IsSpineRuntimeAssetPath(string path)
        {
            string fileName = Path.GetFileName(path);
            if (string.Equals(fileName, SpineAssemblyName + ".asmdef",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(fileName, SpineAssemblyName + ".dll",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(fileName, "spine-csharp.dll",
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return string.Equals(fileName, "SkeletonAnimation.cs",
                       StringComparison.OrdinalIgnoreCase) &&
                   path.IndexOf("/Spine/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool SynchronizeScriptingDefine(bool shouldBeEnabled)
        {
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            if (buildTargetGroup == BuildTargetGroup.Unknown)
            {
                return false;
            }

            NamedBuildTarget namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
            string currentSymbols = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
            var symbols = new HashSet<string>(
                currentSymbols.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));

            bool changed = shouldBeEnabled
                ? symbols.Add(SpineDefine)
                : symbols.Remove(SpineDefine);
            foreach (string legacySpineDefine in LegacySpineDefines)
            {
                changed = symbols.Remove(legacySpineDefine) || changed;
            }

            if (!changed)
            {
                return false;
            }

            PlayerSettings.SetScriptingDefineSymbols(
                namedBuildTarget,
                string.Join(";", symbols.OrderBy(symbol => symbol, StringComparer.Ordinal)));
            return true;
        }

        private static void ShowNoticeOncePerSession()
        {
            if (SessionState.GetBool(SessionKey, false) ||
                EditorPrefs.GetBool(GetPreferenceKey(), false))
            {
                return;
            }

            SessionState.SetBool(SessionKey, true);
            SpineRuntimeNoticeWindow.Open();
        }

        internal static string GetPreferenceKey()
        {
            string projectHash = Hash128.Compute(Application.dataPath).ToString();
            return PreferenceKeyPrefix + projectHash;
        }

        internal static void RefreshAfterAssetChange()
        {
            EditorApplication.delayCall -= Initialize;
            EditorApplication.delayCall += Initialize;
            EditorApplication.delayCall -= SpineRuntimeNoticeWindow.RefreshOpenWindows;
            EditorApplication.delayCall += SpineRuntimeNoticeWindow.RefreshOpenWindows;
        }

        [DidReloadScripts]
        private static void RefreshAfterScriptReload()
        {
            EditorApplication.delayCall += SpineRuntimeNoticeWindow.RefreshOpenWindows;
        }
    }

    internal sealed class SpineRuntimeAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (ContainsSpineRuntimeAsset(importedAssets) ||
                ContainsSpineRuntimeAsset(deletedAssets) ||
                ContainsSpineRuntimeAsset(movedAssets) ||
                ContainsSpineRuntimeAsset(movedFromAssetPaths))
            {
                SpineRuntimeSetup.RefreshAfterAssetChange();
            }
        }

        private static bool ContainsSpineRuntimeAsset(IEnumerable<string> paths)
        {
            return paths.Any(SpineRuntimeSetup.IsSpineRuntimeAssetPath);
        }
    }

    internal sealed class SpineRuntimeNoticeWindow : EditorWindow
    {
        private const string DownloadUrl = "https://esotericsoftware.com/spine-unity-download";

        private bool _dontShowAgain;
        private bool _isSpineInstalled;

        [MenuItem("Tools/Layer Lab/Spine Runtime Setup")]
        internal static void Open()
        {
            var window = GetWindow<SpineRuntimeNoticeWindow>(true, "Spine Runtime Setup", true);
            window.minSize = new Vector2(480f, 300f);
            window.maxSize = new Vector2(480f, 300f);
            window.Show();
        }

        private void OnEnable()
        {
            _dontShowAgain = EditorPrefs.GetBool(SpineRuntimeSetup.GetPreferenceKey(), false);
            RefreshInstallationStatus();
        }

        private void OnFocus()
        {
            RefreshInstallationStatus();
        }

        private void OnGUI()
        {
            const float sidePadding = 12f;
            const float topPadding = 18f;
            const float bottomPadding = 14f;
            Rect contentRect = new Rect(
                sidePadding,
                topPadding,
                position.width - sidePadding * 2f,
                position.height - topPadding - bottomPadding);

            GUILayout.BeginArea(contentRect);
            try
            {
                EditorGUILayout.LabelField("Spine Unity Runtime Status", EditorStyles.boldLabel);
                EditorGUILayout.Space(10f);
                DrawInstallationStatus();

                EditorGUILayout.Space(10f);
                if (_isSpineInstalled)
                {
                    DrawFullWidthHelpBox(
                        "Spine Unity Runtime was detected in this project. " +
                        "Spine animation and character features are available.",
                        MessageType.Info,
                        contentRect.width);
                }
                else
                {
                    DrawFullWidthHelpBox(
                        "Layer Lab 2D Art Maker requires Spine Unity Runtime 4.3 " +
                        "for animation and character features. Spine features will be " +
                        "enabled automatically after installation.",
                        MessageType.Warning,
                        contentRect.width);

                    EditorGUILayout.Space(10f);
                    if (DrawFullWidthButton("Open Spine Runtime Download Page", 34f))
                    {
                        Application.OpenURL(DownloadUrl);
                    }
                }

                GUILayout.FlexibleSpace();
                if (DrawFullWidthButton("Close", 30f))
                {
                    Close();
                }

                if (!_isSpineInstalled)
                {
                    EditorGUILayout.Space(8f);
                    Rect toggleRect = GUILayoutUtility.GetRect(
                        0f,
                        EditorGUIUtility.singleLineHeight,
                        GUILayout.ExpandWidth(true));
                    bool dontShowAgain = EditorGUI.ToggleLeft(
                        toggleRect,
                        "Do not show this notice again",
                        _dontShowAgain);
                    if (dontShowAgain != _dontShowAgain)
                    {
                        _dontShowAgain = dontShowAgain;
                        EditorPrefs.SetBool(SpineRuntimeSetup.GetPreferenceKey(), _dontShowAgain);
                    }
                }
            }
            finally
            {
                GUILayout.EndArea();
            }
        }

        private static void DrawFullWidthHelpBox(
            string message,
            MessageType messageType,
            float contentWidth)
        {
            var content = new GUIContent(message);
            float height = EditorStyles.helpBox.CalcHeight(content, contentWidth);
            Rect helpBoxRect = GUILayoutUtility.GetRect(
                0f,
                height,
                GUILayout.ExpandWidth(true));
            EditorGUI.HelpBox(helpBoxRect, message, messageType);
        }

        private static bool DrawFullWidthButton(string text, float height)
        {
            Rect buttonRect = GUILayoutUtility.GetRect(
                0f,
                height,
                GUILayout.ExpandWidth(true));
            return GUI.Button(buttonRect, text);
        }

        private void DrawInstallationStatus()
        {
            string statusIcon = _isSpineInstalled ? "✓" : "✕";
            string statusText = _isSpineInstalled ? "Installed" : "Not Installed";
            string tooltip = _isSpineInstalled
                ? "Spine Unity Runtime was detected."
                : "Spine Unity Runtime was not detected.";
            Color statusColor = _isSpineInstalled
                ? new Color(0.2f, 0.7f, 0.25f)
                : new Color(0.9f, 0.25f, 0.25f);

            Rect statusRect = GUILayoutUtility.GetRect(
                0f,
                46f,
                GUILayout.ExpandWidth(true));
            GUI.Box(statusRect, GUIContent.none, EditorStyles.helpBox);

            var iconStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18
            };
            iconStyle.normal.textColor = statusColor;

            var statusStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 13
            };
            statusStyle.normal.textColor = statusColor;

            Rect iconRect = new Rect(statusRect.x + 10f, statusRect.y, 28f, statusRect.height);
            Rect textRect = new Rect(
                iconRect.xMax + 8f,
                statusRect.y,
                statusRect.width - 56f,
                statusRect.height);
            GUI.Label(iconRect, new GUIContent(statusIcon, tooltip), iconStyle);
            GUI.Label(textRect, new GUIContent(statusText, tooltip), statusStyle);
        }

        private void RefreshInstallationStatus()
        {
            _isSpineInstalled = SpineRuntimeSetup.IsSpineRuntimeInstalled();
        }

        internal static void RefreshOpenWindows()
        {
            foreach (SpineRuntimeNoticeWindow window in
                     Resources.FindObjectsOfTypeAll<SpineRuntimeNoticeWindow>())
            {
                window.RefreshInstallationStatus();
                window.Repaint();
            }
        }
    }
}
