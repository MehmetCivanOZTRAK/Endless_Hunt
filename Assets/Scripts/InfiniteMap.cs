using UnityEngine;

public class InfiniteMap : MonoBehaviour
{
    public Camera viewCamera;
    public Material[] maps;
    public Vector2 worldTileSize = new(24, 24);
    public float edgeMargin = 6;
    MeshRenderer _renderer;
    MaterialPropertyBlock _properties;
    static readonly int TextureST = Shader.PropertyToID("_MainTex_ST");
    public void SelectMap(int index)
    {
        _renderer = GetComponent<MeshRenderer>();
        _properties ??= new MaterialPropertyBlock();
        if (maps != null && maps.Length > 0) _renderer.sharedMaterial = maps[Mathf.Abs(index) % maps.Length];
        Refresh();
    }
    void LateUpdate() => Refresh();
    public void Refresh()
    {
        if (!viewCamera || !_renderer) return;
        float h = viewCamera.orthographicSize * 2 + edgeMargin, w = viewCamera.orthographicSize * 2 * viewCamera.aspect + edgeMargin;
        var center = viewCamera.transform.position;
        transform.position = new Vector3(center.x, center.y, 5);
        transform.localScale = new Vector3(w, h, 1);
        float tileX = Mathf.Max(.1f, worldTileSize.x), tileY = Mathf.Max(.1f, worldTileSize.y);
        _properties.SetVector(TextureST, new Vector4(w / tileX, h / tileY,
            Mathf.Repeat((center.x - w * .5f) / tileX, 1), Mathf.Repeat((center.y - h * .5f) / tileY, 1)));
        _renderer.SetPropertyBlock(_properties);
    }
}
