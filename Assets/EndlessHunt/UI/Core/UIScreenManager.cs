using System;
using System.Collections.Generic;
using UnityEngine;

namespace EndlessHunt.UI
{
    [DisallowMultipleComponent]
    public sealed class UIScreenManager : MonoBehaviour
    {
        [SerializeField] private Transform screenLayer;
        [SerializeField] private UIScreen initialScreen;
        [SerializeField] private bool discoverChildrenOnAwake = true;

        private readonly Dictionary<string, UIScreen> _screens = new(StringComparer.Ordinal);
        private readonly List<UIScreen> _history = new();

        public UIScreen ActiveScreen { get; private set; }
        public int HistoryCount => _history.Count;
        public event Action<UIScreen, UIScreen> ActiveScreenChanged;

        private void Awake() => Initialize();

        public void Configure(Transform layer, UIScreen firstScreen = null)
        {
            screenLayer = layer;
            initialScreen = firstScreen;
        }

        public void Initialize()
        {
            if (discoverChildrenOnAwake && screenLayer != null)
            {
                foreach (UIScreen screen in screenLayer.GetComponentsInChildren<UIScreen>(true)) Register(screen);
            }
            if (initialScreen != null) Open(initialScreen, false, true);
        }

        public bool Register(UIScreen screen)
        {
            if (screen == null || string.IsNullOrWhiteSpace(screen.ScreenId)) return false;
            if (_screens.TryGetValue(screen.ScreenId, out UIScreen existing) && existing != screen)
            {
                Debug.LogWarning($"Duplicate UI screen id ignored: {screen.ScreenId}", screen);
                return false;
            }
            _screens[screen.ScreenId] = screen;
            return true;
        }

        public void Unregister(UIScreen screen)
        {
            if (screen == null) return;
            if (_screens.TryGetValue(screen.ScreenId, out UIScreen existing) && existing == screen)
                _screens.Remove(screen.ScreenId);
            _history.Remove(screen);
            if (ActiveScreen == screen) ActiveScreen = null;
        }

        public bool Open(string screenId, bool rememberCurrent = true)
        {
            return _screens.TryGetValue(screenId, out UIScreen screen) && Open(screen, rememberCurrent);
        }

        public bool Open(UIScreen screen, bool rememberCurrent = true, bool clearHistory = false)
        {
            if (screen == null || screen == ActiveScreen) return screen != null;
            Register(screen);
            if (clearHistory) _history.Clear();
            UIScreen previous = ActiveScreen;
            if (previous != null)
            {
                if (rememberCurrent) _history.Add(previous);
                previous.SetVisible(false);
            }
            ActiveScreen = screen;
            screen.SetVisible(true);
            ActiveScreenChanged?.Invoke(previous, screen);
            return true;
        }

        public bool CloseActive()
        {
            if (ActiveScreen == null) return false;
            UIScreen previous = ActiveScreen;
            previous.SetVisible(false);
            ActiveScreen = null;
            ActiveScreenChanged?.Invoke(previous, null);
            return true;
        }

        public bool GoBack()
        {
            while (_history.Count > 0)
            {
                int index = _history.Count - 1;
                UIScreen destination = _history[index];
                _history.RemoveAt(index);
                if (destination == null) continue;
                UIScreen previous = ActiveScreen;
                if (previous != null) previous.SetVisible(false);
                ActiveScreen = destination;
                destination.SetVisible(true);
                ActiveScreenChanged?.Invoke(previous, destination);
                return true;
            }
            return false;
        }

        public void ClearHistory() => _history.Clear();
    }
}
