using UnityEngine;

public class InfiniteMap : MonoBehaviour
{
  [SerializeField] private float scrollSpeed = 0.5f;
  [SerializeField] private Transform playerTransform;
    private MeshRenderer meshRenderer;
    private Material material;
    private void Awake() {
        meshRenderer = GetComponent<MeshRenderer>();
        material = meshRenderer.material;
    }
  private void Update() {
    float x = playerTransform.position.x * scrollSpeed;
    float y = playerTransform.position.y * scrollSpeed;
    Vector2 offset = new Vector2(x,y);

    meshRenderer.material.mainTextureOffset = offset;
    
  }
}