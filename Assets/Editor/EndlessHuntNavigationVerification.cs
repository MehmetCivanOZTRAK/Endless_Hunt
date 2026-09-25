using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DG.Tweening;
using EndlessHunt;
using EndlessHunt.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class EndlessHuntNavigationVerification
{
    const string Key = "EH.NavigationVerification";
    static readonly List<string> Checks = new();
    static int _step, _errors;
    static double _next;
    static bool _resume;
    static Button[] Tabs(SurvivorMenu m) => new[] { m.storeTab, m.equipmentTab, m.homeTab, m.upgradeTab, m.arsenalTab };
    static EndlessHuntNavigationVerification()
    {
        if (!SessionState.GetBool(Key, false)) return;
        EditorApplication.playModeStateChanged += Changed;
        Application.logMessageReceived += Log;
    }
    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        var m = Object.FindFirstObjectByType<SurvivorMenu>();
        SessionState.SetString(Key + ".style", Style(m));
        SessionState.SetFloat(Key + ".battleVisibleX", Bounds(m.tabPressedVisuals[2].rectTransform).center.x);
        foreach (var b in Tabs(m)) SessionState.SetVector3(Key + "." + b.name, ((RectTransform)b.transform).anchoredPosition);
        SessionState.SetBool(Key, true);
        EditorApplication.isPlaying = true;
    }
    static string Style(SurvivorMenu m)
    {
        var parts = new List<string>();
        foreach (var b in Tabs(m))
        {
            var r = (RectTransform)b.transform;
            parts.Add(b.name + ":" + r.anchorMin + r.anchorMax + r.sizeDelta + r.pivot + r.localScale + ":" + r.anchoredPosition.y);
            parts.Add(b.transition + ":" + JsonUtility.ToJson(b.colors) + JsonUtility.ToJson(b.spriteState));
            foreach (var image in b.GetComponentsInChildren<Image>(true))
            {
                parts.Add(image.name + ":" + AssetDatabase.GetAssetPath(image.sprite) + ":" + image.color + ":" + image.preserveAspect + ":" + image.type);
                if (image.transform != b.transform) parts.Add(JsonUtility.ToJson(image.rectTransform.anchoredPosition) + image.rectTransform.sizeDelta + image.rectTransform.localScale);
            }
        }
        foreach (var b in m.inventoryWeaponCards) parts.Add(AssetDatabase.GetAssetPath(b.button.GetComponent<Image>().sprite));
        foreach (var b in m.equippedSlotButtons) parts.Add(AssetDatabase.GetAssetPath(b.GetComponent<Image>().sprite));
        foreach (var t in m.homePage.transform.root.GetComponentsInChildren<TMP_Text>(true).Where(t => t.name == "Myriad SDF"))
            parts.Add(t.name + ":" + AssetDatabase.GetAssetPath(t.font) + ":" + AssetDatabase.GetAssetPath(t.fontSharedMaterial));
        return string.Join("\n", parts);
    }
    static void Changed(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode) { EditorApplication.isPaused = false; _next = EditorApplication.timeSinceStartup + 4; EditorApplication.update += Tick; }
        if (state == PlayModeStateChange.ExitingPlayMode)
        { SessionState.SetString(Key + ".report", string.Join("\n", Checks)); SessionState.SetInt(Key + ".errors", _errors); }
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            SessionState.SetBool(Key, false); EditorApplication.update -= Tick;
            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/navigation-verification.txt", SessionState.GetString(Key + ".report", "") + "\nERRORS: " + SessionState.GetInt(Key + ".errors", 0));
            EditorApplication.delayCall += Finish;
        }
    }
    static void Finish()
    {
        int errors = SessionState.GetInt(Key + ".errors", 0);
        if (errors == 0) try { EndlessHuntAndroidScriptCheck.Run(); } catch (Exception e) { File.AppendAllText("Logs/navigation-verification.txt", "\nANDROID FAILED " + e); errors++; }
        EditorApplication.Exit(errors == 0 ? 0 : 1);
    }
    static void Check(bool ok, string message) { Checks.Add((ok ? "PASS " : "FAIL ") + message); if (!ok) _errors++; }
    static void State(SurvivorMenu m, int selected)
    {
        Check(m.tabPressedVisuals.Count(i => i.gameObject.activeSelf) == 1 && m.tabPressedVisuals[selected].gameObject.activeSelf, "Exactly selected pressed visual: " + selected);
        Check(Tabs(m).Select((b, i) => b.GetComponent<Image>().enabled == (i != selected)).All(x => x), "Other four buttons show normal artwork: " + selected);
        var ordered = Tabs(m).Select((b, i) => Bounds((i == selected ? m.tabPressedVisuals[i] : b.GetComponent<Image>()).rectTransform)).OrderBy(b => b.min.x).ToArray();
        Check(ordered.Zip(ordered.Skip(1), (a, b) => b.min.x >= a.max.x - 16).All(x => x), "Only authored border overlap remains: " + selected);
        Check(Style(m) == SessionState.GetString(Key + ".style", ""), "Authored artwork, fonts, sizes and Y positions preserved: " + selected);
    }
    static Bounds Bounds(RectTransform r)
    {
        var corners = new Vector3[4]; r.GetWorldCorners(corners);
        var parent = Object.FindFirstObjectByType<SurvivorMenu>().homeTab.transform.parent;
        var b = new Bounds(parent.InverseTransformPoint(corners[0]), Vector3.zero);
        foreach (var c in corners) b.Encapsulate(parent.InverseTransformPoint(c));
        return b;
    }
    static void Click(SurvivorMenu m, int index, bool pressed = false)
    {
        var b = Tabs(m)[index]; var rect = pressed ? m.tabPressedVisuals[index].rectTransform : (RectTransform)b.transform;
        var canvas = b.GetComponentInParent<Canvas>();
        var data = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left,
            position = RectTransformUtility.WorldToScreenPoint(canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, rect.TransformPoint(rect.rect.center)) };
        var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(data, hits);
        var hit = hits.Count > 0 ? hits[0].gameObject.GetComponentInParent<Button>() : null;
        Check(hit == b, "Actual touch raycast reaches " + b.name + (pressed ? " pressed artwork" : ""));
        if (hit) ExecuteEvents.Execute(hit.gameObject, data, ExecuteEvents.pointerClickHandler);
    }
    static void Tick()
    {
        if (_resume) { _resume = false; EditorApplication.isPaused = false; _next = EditorApplication.timeSinceStartup + 3; return; }
        if (EditorApplication.timeSinceStartup < _next) return;
        _next = EditorApplication.timeSinceStartup + .6;
        try
        {
            var m = Object.FindFirstObjectByType<SurvivorMenu>();
            // In batch mode asset loading/rendering can consume wall time before a tween gets a frame.
            // Inspect settled UI, not an arbitrary wall-clock instant halfway through its animation.
            if (Tabs(m).Any(b => DOTween.IsTweening(b.transform)) ||
                new[] { m.homePage, m.equipmentPage, m.upgradePage, m.storePage }.Any(p => p.TryGetComponent<CanvasGroup>(out var g) && DOTween.IsTweening(g)))
            { _next = EditorApplication.timeSinceStartup + .1; return; }
            switch (_step++)
            {
                case 0:
                    State(m, 2); Check(m.homePage.activeSelf, "Battle page initially active");
                    Check(Tabs(m).All(b => Vector2.Distance(((RectTransform)b.transform).anchoredPosition, SessionState.GetVector3(Key + "." + b.name, Vector3.zero)) < .02f), "Initial navigation positions match editor exactly");
                    EndlessHuntDemoVerification.Capture("nav-01-battle", 540, 1110); Click(m, 1); break;
                case 1:
                    State(m, 1); Check(m.equipmentPage.activeSelf && !m.homePage.activeSelf, "Equipment click switches page");
                    Check(Mathf.Abs(Bounds(m.homeTab.GetComponent<Image>().rectTransform).center.x - SessionState.GetFloat(Key + ".battleVisibleX", 0)) > 50, "Neighbour's visible button moves to make space");
                    EndlessHuntDemoVerification.Capture("nav-02-equipment", 540, 1110); Click(m, 1, true); Click(m, 0); break;
                case 2:
                    State(m, 0); Check(GameObject.Find("Currency Shop"), "Shop click opens test currency store");
                    EndlessHuntDemoVerification.Capture("nav-03-shop", 540, 1110); Click(m, 3); break;
                case 3:
                    State(m, 3); Check(m.upgradePage.activeSelf, "Upgrades click opens talent tree");
                    EndlessHuntDemoVerification.Capture("nav-04-upgrades", 540, 1110); Click(m, 4); break;
                case 4:
                    State(m, 4); Check(m.storePage.activeSelf, "Fifth button reference and existing arsenal page work");
                    EndlessHuntDemoVerification.Capture("nav-05-arsenal", 540, 1110);
                    Time.timeScale = 0; m.homeTab.onClick.Invoke(); m.equipmentTab.onClick.Invoke(); m.storeTab.onClick.Invoke(); m.homeTab.onClick.Invoke(); break;
                case 5:
                    State(m, 2); Check(Tabs(m).All(b => Vector2.Distance(((RectTransform)b.transform).anchoredPosition, SessionState.GetVector3(Key + "." + b.name, Vector3.zero)) < .02f), "Rapid interrupted transitions return exactly home, including timeScale zero");
                    Time.timeScale = 1; EndlessHuntDemoVerification.Capture("nav-06-tablet", 768, 1024); EndlessHuntDemoVerification.Capture("nav-07-tall", 540, 1200); EditorApplication.isPlaying = false; break;
            }
        }
        catch (Exception e) { _errors++; Checks.Add("EXCEPTION " + e); EditorApplication.isPlaying = false; }
    }
    static void Log(string message, string trace, LogType type)
    {
        if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
        if (trace.Contains("UnityEditor.Search.SearchDatabase")) { Checks.Add("EDITOR DIAGNOSTIC: copied-project search index"); _resume = true; return; }
        _errors++; Checks.Add("RUNTIME ERROR " + message + "\n" + trace);
    }
}
