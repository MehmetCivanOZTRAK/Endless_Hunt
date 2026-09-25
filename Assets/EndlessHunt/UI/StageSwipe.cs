using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace EndlessHunt
{
    public sealed class StageSwipe : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public SurvivorMenu menu;
        private Vector2 _start;
        private RectTransform _rect;
        private Vector2 _origin;
        private Canvas _canvas;
        private void Awake() { _rect = (RectTransform)transform; _origin = _rect.anchoredPosition; _canvas = GetComponentInParent<Canvas>(); }
        public void OnBeginDrag(PointerEventData data) { _rect.DOKill(); _start = data.position; }
        public void OnDrag(PointerEventData data)
        {
            float distance = (data.position.x - _start.x) / Mathf.Max(.01f, _canvas.scaleFactor);
            _rect.anchoredPosition = _origin + Vector2.right * Mathf.Clamp(distance, -120f, 120f);
        }
        public void OnEndDrag(PointerEventData data)
        {
            Vector2 delta = data.position - _start;
            if (Mathf.Abs(delta.x) > Screen.width * .08f && Mathf.Abs(delta.x) > Mathf.Abs(delta.y)) menu.CycleStage(delta.x < 0f ? 1 : -1);
            _rect.DOAnchorPos(_origin,.2f).SetEase(Ease.OutCubic).SetUpdate(true);
        }
        private void OnDisable() { if (_rect != null) { _rect.DOKill(); _rect.anchoredPosition = _origin; } }
    }
}
