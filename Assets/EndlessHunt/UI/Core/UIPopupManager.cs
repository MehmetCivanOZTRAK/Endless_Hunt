using System;
using System.Collections.Generic;
using UnityEngine;

namespace EndlessHunt.UI
{
    [DisallowMultipleComponent]
    public sealed class UIPopupManager : MonoBehaviour
    {
        [SerializeField] private Transform popupLayer;
        [SerializeField] private bool discoverChildrenOnAwake = true;

        private readonly Dictionary<string, UIPopup> _popups = new(StringComparer.Ordinal);
        private readonly List<UIPopup> _stack = new();

        public bool HasOpenPopup => _stack.Count > 0;
        public UIPopup TopPopup => _stack.Count > 0 ? _stack[_stack.Count - 1] : null;
        public event Action<UIPopup, bool> PopupStateChanged;

        private void Awake() => Initialize();

        public void Configure(Transform layer) => popupLayer = layer;

        public void Initialize()
        {
            if (!discoverChildrenOnAwake || popupLayer == null) return;
            foreach (UIPopup popup in popupLayer.GetComponentsInChildren<UIPopup>(true)) Register(popup);
        }

        public bool Register(UIPopup popup)
        {
            if (popup == null || string.IsNullOrWhiteSpace(popup.PopupId)) return false;
            if (_popups.TryGetValue(popup.PopupId, out UIPopup existing) && existing != popup)
            {
                Debug.LogWarning($"Duplicate UI popup id ignored: {popup.PopupId}", popup);
                return false;
            }
            _popups[popup.PopupId] = popup;
            popup.CloseRequested -= HandleCloseRequested;
            popup.CloseRequested += HandleCloseRequested;
            return true;
        }

        public bool OpenPopup(string popupId, bool animated = true)
        {
            return _popups.TryGetValue(popupId, out UIPopup popup) && OpenPopup(popup, animated);
        }

        public bool OpenPopup(UIPopup popup, bool animated = true)
        {
            if (popup == null) return false;
            Register(popup);
            _stack.Remove(popup);
            _stack.Add(popup);
            popup.transform.SetAsLastSibling();
            popup.Show(animated);
            PopupStateChanged?.Invoke(popup, true);
            return true;
        }

        public bool CloseTopPopup(bool animated = true)
        {
            return _stack.Count > 0 && ClosePopup(_stack[_stack.Count - 1], animated);
        }

        public bool TryCloseTopOnBack(bool animated = true)
        {
            UIPopup popup = TopPopup;
            return popup != null && popup.CloseOnBack && ClosePopup(popup, animated);
        }

        public bool ClosePopup(UIPopup popup, bool animated = true)
        {
            if (popup == null || !_stack.Remove(popup)) return false;
            popup.Hide(animated);
            PopupStateChanged?.Invoke(popup, false);
            return true;
        }

        public void CloseAllPopups(bool animated = false)
        {
            for (int i = _stack.Count - 1; i >= 0; i--)
            {
                UIPopup popup = _stack[i];
                if (popup == null) continue;
                popup.Hide(animated);
                PopupStateChanged?.Invoke(popup, false);
            }
            _stack.Clear();
        }

        private void HandleCloseRequested(UIPopup popup) => ClosePopup(popup);
    }
}
