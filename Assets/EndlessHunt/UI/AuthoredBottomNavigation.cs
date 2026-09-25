using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessHunt.UI
{
    // Normal/pressed artwork stays authored in the scene. Only selection and horizontal travel are animated.
    public sealed class AuthoredBottomNavigation : IDisposable
    {
        sealed class Slot
        {
            public int index;
            public RectTransform rect;
            public Image normal, pressed;
            public Vector2 origin;
            public bool baselinePressed;
            public Tween movement;
        }

        readonly Slot[] _slots;
        readonly RectTransform _parent;
        readonly float _duration;
        readonly Vector3[] _corners = new Vector3[4];
        Vector2 _parentSize;
        int _selected = -1;

        public AuthoredBottomNavigation(Button[] buttons, Image[] pressedVisuals, float duration)
        {
            _duration = Mathf.Max(0, duration);
            var slots = new System.Collections.Generic.List<Slot>();
            for (int i = 0; i < buttons.Length; i++)
            {
                if (!buttons[i]) continue;
                var normal = buttons[i].GetComponent<Image>();
                var pressed = pressedVisuals != null && i < pressedVisuals.Length ? pressedVisuals[i] : null;
                // Older prefabs also contain an authored pressed child; never replace its sprite from a theme.
                if (!pressed)
                    foreach (var child in buttons[i].GetComponentsInChildren<Image>(true))
                        if (child != normal && (child.name.IndexOf("pressed", StringComparison.OrdinalIgnoreCase) >= 0 || child.name == "Selected Gold Panel"))
                        { pressed = child; break; }
                if (!normal || !pressed) continue;
                var rect = (RectTransform)buttons[i].transform;
                slots.Add(new Slot { index = i, rect = rect, normal = normal, pressed = pressed,
                    origin = rect.anchoredPosition, baselinePressed = pressed.gameObject.activeSelf && pressed.enabled });
            }
            _slots = slots.ToArray();
            if (_slots.Length == 0) return;
            _parent = _slots[0].rect.parent as RectTransform;
            // Page indexes and hierarchy order need not match the visual left-to-right order.
            Array.Sort(_slots, (a, b) => a.rect.localPosition.x.CompareTo(b.rect.localPosition.x));
            _parentSize = _parent.rect.size;
        }

        public void Select(int index, bool animate = true)
        {
            if (_selected == index) return;
            _selected = index;
            foreach (var slot in _slots)
            {
                bool selected = slot.index == index;
                slot.normal.enabled = !selected;
                slot.pressed.gameObject.SetActive(selected);
            }
            Layout(animate);
        }

        public void RefreshDimensions()
        {
            if (!_parent || _parent.rect.size == _parentSize) return;
            _parentSize = _parent.rect.size;
            Layout(false);
        }

        void Layout(bool animate)
        {
            if (!_parent || _slots.Length == 0) return;
            float widthChange = 0;
            foreach (var slot in _slots)
                widthChange += Bounds(slot, slot.index == _selected).size.x - Bounds(slot, slot.baselinePressed).size.x;

            // Retain the authored gaps and overall centre, including asymmetric pressed-child offsets.
            float travel = -widthChange * .5f;
            foreach (var slot in _slots)
            {
                var baseline = Bounds(slot, slot.baselinePressed);
                var selected = Bounds(slot, slot.index == _selected);
                float x = slot.origin.x + travel + baseline.min.x - selected.min.x;
                slot.movement?.Kill();
                if (animate && _duration > 0)
                    slot.movement = slot.rect.DOAnchorPosX(x, _duration).SetEase(Ease.OutCubic).SetUpdate(true);
                else
                    slot.rect.anchoredPosition = new Vector2(x, slot.rect.anchoredPosition.y);
                travel += selected.size.x - baseline.size.x;
            }
        }

        Bounds Bounds(Slot slot, bool pressed)
        {
            var image = pressed ? slot.pressed : slot.normal;
            var r = image.rectTransform;
            var rect = r.rect;
            // Match Image's aspect fit without modifying the authored RectTransform.
            if (image.preserveAspect && image.sprite && image.type == Image.Type.Simple)
            {
                float aspect = image.sprite.rect.width / image.sprite.rect.height;
                var size = rect.size;
                if (size.x / size.y > aspect) size.x = size.y * aspect;
                else size.y = size.x / aspect;
                rect = new Rect(rect.position + Vector2.Scale(rect.size - size, r.pivot), size);
            }
            _corners[0] = new Vector3(rect.xMin, rect.yMin);
            _corners[1] = new Vector3(rect.xMin, rect.yMax);
            _corners[2] = new Vector3(rect.xMax, rect.yMax);
            _corners[3] = new Vector3(rect.xMax, rect.yMin);
            var bounds = new Bounds();
            for (int i = 0; i < 4; i++)
            {
                var point = _parent.InverseTransformPoint(r.TransformPoint(_corners[i]));
                point.x -= slot.rect.anchoredPosition.x - slot.origin.x;
                if (i == 0) bounds = new Bounds(point, Vector3.zero);
                else bounds.Encapsulate(point);
            }
            return bounds;
        }

        public void Dispose()
        {
            foreach (var slot in _slots) slot.movement?.Kill();
        }
    }
}
