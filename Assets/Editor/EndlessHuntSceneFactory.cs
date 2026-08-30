using System.Linq;
using Spine.Unity;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

internal static class EndlessHuntSceneFactory
{
    internal const string VectorUi = "Assets/Vector_UI_Pack_dobo_ui/";
    internal const string LayerThumbnails = "Assets/Layer Lab/2D Art Maker/AMCasual Character/Demo/PartsThumbnail/";

    private static Font _font;
    internal static Font Font => _font != null ? _font : _font =
        AssetDatabase.LoadAssetAtPath<Font>("Assets/Carter_One/CarterOne-Regular.ttf") ??
        Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    internal static Sprite Sprite(string path)
    {
        Sprite direct = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        return direct != null ? direct : AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
    }

    internal static Sprite Ui(string relative) => Sprite(VectorUi + relative);

    internal static Sprite GearIcon(EndlessHunt.GearDefinition gear)
    {
        if (!string.IsNullOrEmpty(gear.spineSkin))
        {
            Sprite icon = Sprite(LayerThumbnails + gear.spineSkin + ".png");
            if (icon != null) return icon;
        }
        return Ui("Icons/128px/armor_icon_128px.png");
    }

    internal static Sprite WeaponIcon(EndlessHunt.WeaponDefinition weapon) =>
        weapon != null && weapon.icon != null ? weapon.icon : Ui("Icons/128px/sword_icon_128px.png");

    internal static Camera Camera(string name, Color background, float size)
    {
        GameObject go = new(name, typeof(Camera), typeof(AudioListener));
        go.tag = "MainCamera";
        Camera camera = go.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = size;
        camera.backgroundColor = background;
        camera.transform.position = new Vector3(0f, 0f, -10f);
        return camera;
    }

    internal static Canvas Canvas(string name)
    {
        GameObject go = new(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = .55f;
        return canvas;
    }

    internal static void EventSystem()
    {
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    internal static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max,
        Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject go = new(name, typeof(RectTransform));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        SetRect(rect, min, max, offsetMin, offsetMax);
        return rect;
    }

    internal static Image Image(string name, Transform parent, Sprite sprite, Vector2 min, Vector2 max,
        Vector2 offsetMin, Vector2 offsetMax, Color? color = null, bool raycast = false)
    {
        RectTransform rect = Rect(name, parent, min, max, offsetMin, offsetMax);
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color ?? Color.white;
        image.raycastTarget = raycast;
        if (sprite != null && sprite.border.sqrMagnitude > 0f)
            image.type = UnityEngine.UI.Image.Type.Sliced;
        return image;
    }

    internal static Text Text(string name, Transform parent, string value, int size, Color color,
        TextAnchor alignment, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax,
        FontStyle style = FontStyle.Normal)
    {
        RectTransform rect = Rect(name, parent, min, max, offsetMin, offsetMax);
        Text text = rect.gameObject.AddComponent<Text>();
        text.font = Font;
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.alignment = alignment;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = Mathf.Max(12, size / 2);
        text.resizeTextMaxSize = size;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        return text;
    }

    internal static Button Button(string name, Transform parent, Sprite sprite, string label, int size,
        Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax, Color? color = null)
    {
        Image image = Image(name, parent, sprite, min, max, offsetMin, offsetMax, color ?? Color.white, true);
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(1f, 1f, 1f, .9f);
        colors.pressedColor = new Color(.75f, .8f, .9f, 1f);
        button.colors = colors;
        if (!string.IsNullOrEmpty(label))
            Text("Label", button.transform, label, size, Color.white, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, new Vector2(12f, 8f), new Vector2(-12f, -8f), FontStyle.Bold);
        return button;
    }

    internal static Slider Bar(string name, Transform parent, Sprite background, Sprite fillSprite,
        Color fillColor, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
    {
        Image root = Image(name, parent, background, min, max, offsetMin, offsetMax, Color.white);
        Slider slider = root.gameObject.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.interactable = false;
        Image fill = Image("Fill", root.transform, fillSprite, Vector2.zero, Vector2.one,
            new Vector2(8f, 8f), new Vector2(-8f, -8f), fillColor);
        slider.fillRect = fill.rectTransform;
        slider.targetGraphic = fill;
        return slider;
    }

    internal static SkeletonAnimation SpineCharacter(SkeletonDataAsset data, string name, Transform parent,
        Vector3 localPosition, float scale)
    {
        SkeletonComponents<SkeletonRenderer, SkeletonAnimation> components =
            SkeletonAnimation.NewSkeletonAnimationGameObject(data);
        SkeletonAnimation view = components.skeletonAnimation;
        components.skeletonRenderer.SkeletonDataAsset = data;
        components.skeletonRenderer.Initialize(true);
        view.Initialize(true);
        EditorUtility.SetDirty(components.skeletonRenderer);
        EditorUtility.SetDirty(view);
        view.name = name;
        view.transform.SetParent(parent, false);
        view.transform.localPosition = localPosition;
        view.transform.localScale = Vector3.one * scale;
        return view;
    }

    internal static SkeletonAnimation PrefabCharacter(GameObject prefab, string name, Transform parent,
        Vector3 localPosition, float scale)
    {
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        instance.name = name;
        instance.transform.localPosition = localPosition;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one * scale;
        SkeletonAnimation view = instance.GetComponentInChildren<SkeletonAnimation>(true);
        if (view == null) throw new MissingComponentException(name + " içinde SkeletonAnimation bulunamadı.");
        foreach (MonoBehaviour component in instance.GetComponents<MonoBehaviour>())
        {
            if (component == null) continue;
            string typeName = component.GetType().Name;
            if (typeName == "CharacterPrefabData" || typeName == "PartsManager")
                Object.DestroyImmediate(component);
        }
        return view;
    }

    internal static SkeletonGraphic SpineGraphic(GameObject prefab, string name, Transform parent,
        Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax, float scale)
    {
        SkeletonAnimation source = prefab.GetComponentInChildren<SkeletonAnimation>(true);
        if (source == null || source.SkeletonDataAsset == null)
            throw new MissingReferenceException("Başlangıç prefabında SkeletonDataAsset bulunamadı.");
        RectTransform rect = Rect(name, parent, min, max, offsetMin, offsetMax);
        rect.localScale = Vector3.one * scale;
        Material defaultMaterial = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Spine/Runtime/spine-unity/Materials/UI-PMATexture/SkeletonGraphicDefault.mat");
        SkeletonComponents<SkeletonGraphic, SkeletonAnimation> components =
            SkeletonGraphic.AddSkeletonGraphicAnimationComponents(rect.gameObject, source.SkeletonDataAsset, defaultMaterial);
        SkeletonGraphic graphic = components.skeletonRenderer;
        graphic.additiveMaterial = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Spine/Runtime/spine-unity/Materials/UI-PMATexture/SkeletonGraphicAdditive.mat");
        graphic.multiplyMaterial = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Spine/Runtime/spine-unity/Materials/UI-PMATexture/SkeletonGraphicMultiply.mat");
        graphic.screenMaterial = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Spine/Runtime/spine-unity/Materials/UI-PMATexture/SkeletonGraphicScreen.mat");
        graphic.raycastTarget = false;
        components.skeletonAnimation.AnimationState.SetAnimation(0, "Idle", true);
        return graphic;
    }

    internal static void SetRect(RectTransform rect, Vector2 min, Vector2 max,
        Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }
}
