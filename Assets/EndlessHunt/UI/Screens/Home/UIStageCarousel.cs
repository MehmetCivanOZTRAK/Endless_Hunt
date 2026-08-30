using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EndlessHunt.UI.Screens
{
    [DisallowMultipleComponent]
    public sealed class UIStageCarousel : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform content;
        [SerializeField] private UIStageCardView[] cards = Array.Empty<UIStageCardView>();
        [SerializeField, Min(.05f)] private float snapDuration = .22f;
        [SerializeField, Min(1f)] private float swipeThreshold = 72f;

        private Coroutine _snapRoutine;
        private Vector2 _dragStart;
        private int _unlockedCount = 1;

        public int SelectedIndex { get; private set; }
        public int CardCount => cards != null ? cards.Length : 0;
        public bool IsSnapping => _snapRoutine != null;
        public float TargetNormalizedPosition => NormalizedFor(SelectedIndex);
        public event Action<int> SelectionChanged;

        public void Configure(ScrollRect targetScrollRect, RectTransform targetContent,
            UIStageCardView[] stageCards)
        {
            scrollRect = targetScrollRect;
            content = targetContent;
            cards = stageCards ?? Array.Empty<UIStageCardView>();
        }

        public void Initialize(int selectedIndex, int unlockedCount)
        {
            _unlockedCount = Mathf.Clamp(unlockedCount, 1, Mathf.Max(1, CardCount));
            SelectedIndex = Mathf.Clamp(selectedIndex, 0, Mathf.Max(0, CardCount - 1));
            Canvas.ForceUpdateCanvases();
            if (scrollRect != null)
            {
                scrollRect.StopMovement();
                scrollRect.horizontalNormalizedPosition = NormalizedFor(SelectedIndex);
            }
            ApplyCardStates();
        }

        public void SetUnlockedCount(int unlockedCount)
        {
            _unlockedCount = Mathf.Clamp(unlockedCount, 1, Mathf.Max(1, CardCount));
            ApplyCardStates();
        }

        public bool SelectIndex(int index, bool animated = true, bool notify = true)
        {
            if (CardCount == 0 || scrollRect == null) return false;
            int target = Mathf.Clamp(index, 0, CardCount - 1);
            if (_snapRoutine != null)
            {
                StopCoroutine(_snapRoutine);
                _snapRoutine = null;
            }

            SelectedIndex = target;
            ApplyCardStates();
            if (!animated || !isActiveAndEnabled)
            {
                scrollRect.StopMovement();
                scrollRect.horizontalNormalizedPosition = NormalizedFor(target);
                if (notify) SelectionChanged?.Invoke(target);
                return true;
            }

            _snapRoutine = StartCoroutine(SnapTo(target, notify));
            return true;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _dragStart = eventData.position;
            if (_snapRoutine == null) return;
            StopCoroutine(_snapRoutine);
            _snapRoutine = null;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (CardCount <= 1 || scrollRect == null) return;
            float drag = eventData.position.x - _dragStart.x;
            int target;
            if (Mathf.Abs(drag) >= swipeThreshold)
                target = SelectedIndex + (drag < 0f ? 1 : -1);
            else
                target = Mathf.RoundToInt(scrollRect.horizontalNormalizedPosition * (CardCount - 1));
            SelectIndex(target);
        }

        private IEnumerator SnapTo(int index, bool notify)
        {
            scrollRect.StopMovement();
            float start = scrollRect.horizontalNormalizedPosition;
            float target = NormalizedFor(index);
            float elapsed = 0f;
            while (elapsed < snapDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / snapDuration);
                t = 1f - Mathf.Pow(1f - t, 3f);
                scrollRect.horizontalNormalizedPosition = Mathf.LerpUnclamped(start, target, t);
                yield return null;
            }

            scrollRect.horizontalNormalizedPosition = target;
            _snapRoutine = null;
            if (notify) SelectionChanged?.Invoke(index);
        }

        private float NormalizedFor(int index)
        {
            return CardCount <= 1 ? 0f : Mathf.Clamp01(index / (float)(CardCount - 1));
        }

        private void ApplyCardStates()
        {
            if (cards == null) return;
            for (int i = 0; i < cards.Length; i++)
                if (cards[i] != null)
                    cards[i].SetState(i == SelectedIndex, i < _unlockedCount);
        }

        private void OnDisable()
        {
            if (_snapRoutine == null) return;
            StopCoroutine(_snapRoutine);
            _snapRoutine = null;
        }
    }
}
