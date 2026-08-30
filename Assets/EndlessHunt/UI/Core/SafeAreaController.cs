using UnityEngine;

namespace EndlessHunt.UI
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaController : MonoBehaviour
    {
        [SerializeField] private bool conformHorizontal = true;
        [SerializeField] private bool conformVertical = true;

        private RectTransform _rectTransform;
        private Rect _lastSafeArea = new(-1f, -1f, -1f, -1f);
        private Vector2Int _lastScreenSize = new(-1, -1);

        public Rect LastAppliedSafeArea => _lastSafeArea;

        private void OnEnable()
        {
            _rectTransform = GetComponent<RectTransform>();
            Refresh(true);
        }

        private void Update()
        {
            Refresh(false);
        }

        public void Refresh(bool force = true)
        {
            if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
            var screenSize = new Vector2Int(Screen.width, Screen.height);
            Rect safeArea = Screen.safeArea;
            if (!force && safeArea == _lastSafeArea && screenSize == _lastScreenSize) return;
            ApplySafeArea(safeArea, screenSize);
        }

        public void ApplySafeArea(Rect safeArea, Vector2 screenSize)
        {
            if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
            CalculateAnchors(safeArea, screenSize, conformHorizontal, conformVertical,
                out Vector2 anchorMin, out Vector2 anchorMax);
            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
            _lastSafeArea = safeArea;
            _lastScreenSize = new Vector2Int(Mathf.RoundToInt(screenSize.x), Mathf.RoundToInt(screenSize.y));
        }

        public static void CalculateAnchors(Rect safeArea, Vector2 screenSize, bool useHorizontal,
            bool useVertical, out Vector2 anchorMin, out Vector2 anchorMax)
        {
            float width = Mathf.Max(1f, screenSize.x);
            float height = Mathf.Max(1f, screenSize.y);
            anchorMin = new Vector2(safeArea.xMin / width, safeArea.yMin / height);
            anchorMax = new Vector2(safeArea.xMax / width, safeArea.yMax / height);
            if (!useHorizontal) { anchorMin.x = 0f; anchorMax.x = 1f; }
            if (!useVertical) { anchorMin.y = 0f; anchorMax.y = 1f; }
            anchorMin = Vector2.Max(Vector2.zero, Vector2.Min(Vector2.one, anchorMin));
            anchorMax = Vector2.Max(anchorMin, Vector2.Min(Vector2.one, anchorMax));
        }
    }
}
