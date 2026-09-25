using EndlessHunt.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

internal static class EndlessHuntUIPrefabFactory
{
    internal const string ThemePath = "Assets/EndlessHunt/UI/Theme/EndlessHuntUITheme.asset";
    internal const string PrefabFolder = "Assets/EndlessHunt/UI/Common/Prefabs";

    internal static Sprite Ui(string relativePath) => EndlessHuntSceneFactory.Ui(relativePath);

    internal static RectTransform Root(string name, Vector2 size, params System.Type[] components)
    {
        GameObject root = new(name, typeof(RectTransform));
        foreach (System.Type component in components) root.AddComponent(component);
        RectTransform rect = root.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
        rect.pivot = new Vector2(.5f, .5f);
        rect.sizeDelta = size;
        return rect;
    }

    internal static RectTransform Rect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax)
    {
        return EndlessHuntSceneFactory.Rect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
    }

    internal static Image Image(string name, Transform parent, Sprite sprite, Vector2 anchorMin,
        Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color? color = null, bool raycast = false)
    {
        return EndlessHuntSceneFactory.Image(name, parent, sprite, anchorMin, anchorMax, offsetMin, offsetMax,
            color, raycast);
    }

    internal static Text Text(string name, Transform parent, string value, int size, Color color,
        TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
        FontStyle style = FontStyle.Normal)
    {
        return EndlessHuntSceneFactory.Text(name, parent, value, size, color, alignment, anchorMin, anchorMax,
            offsetMin, offsetMax, style);
    }

    internal static Button Button(string name, Transform parent, Sprite sprite, string label, int size,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        return EndlessHuntSceneFactory.Button(name, parent, sprite, label, size, anchorMin, anchorMax,
            offsetMin, offsetMax);
    }

    internal static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    internal static string PrefabPath(string name) => $"{PrefabFolder}/{name}.prefab";

    internal static GameObject Save(RectTransform root, string name)
    {
        string path = PrefabPath(name);
        GameObject saved = PrefabUtility.SaveAsPrefabAsset(root.gameObject, path);
        Object.DestroyImmediate(root.gameObject);
        if (saved == null) throw new UnityException("Failed to save UI prefab: " + path);
        return saved;
    }

    internal static void ApplySliced(Image image)
    {
        if (image != null && image.sprite != null && image.sprite.border.sqrMagnitude > 0f)
            image.type = UnityEngine.UI.Image.Type.Sliced;
    }

    internal static Image AddSelection(Transform parent)
    {
        Image selection = Image("Selected", parent, Ui("Effects/effect_yellow.png"), Vector2.zero,
            Vector2.one, new Vector2(-4f, -4f), new Vector2(4f, 4f), Color.white);
        selection.enabled = false;
        selection.raycastTarget = false;
        return selection;
    }
}
