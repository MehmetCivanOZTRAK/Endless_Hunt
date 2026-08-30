using System.Collections.Generic;
using EndlessHunt.UI.Common;
using UnityEngine;

namespace EndlessHunt.UI
{
    [DisallowMultipleComponent]
    public sealed class UITabController : MonoBehaviour
    {
        [SerializeField] private UIScreenManager screenManager;
        [SerializeField] private List<UITabButton> tabs = new();
        [SerializeField] private int initialTabIndex;

        public UITabButton ActiveTab { get; private set; }

        private void Awake()
        {
            for (int i = 0; i < tabs.Count; i++) Register(tabs[i]);
            if (tabs.Count > 0) SelectTab(Mathf.Clamp(initialTabIndex, 0, tabs.Count - 1));
        }

        public void Configure(UIScreenManager manager) => screenManager = manager;

        public void Register(UITabButton tab)
        {
            if (tab == null) return;
            if (!tabs.Contains(tab)) tabs.Add(tab);
            tab.Clicked -= HandleTabClicked;
            tab.Clicked += HandleTabClicked;
        }

        public bool SelectTab(string tabId)
        {
            int index = tabs.FindIndex(tab => tab != null && tab.TabId == tabId);
            return index >= 0 && SelectTab(index);
        }

        public bool SelectTab(int index)
        {
            if (index < 0 || index >= tabs.Count || tabs[index] == null || !tabs[index].IsInteractable) return false;
            UITabButton selected = tabs[index];
            if (screenManager != null && !string.IsNullOrWhiteSpace(selected.TargetScreenId) &&
                !screenManager.Open(selected.TargetScreenId))
                return false;
            foreach (UITabButton tab in tabs) if (tab != null) tab.SetActive(tab == selected);
            ActiveTab = selected;
            return true;
        }

        private void HandleTabClicked(UITabButton tab)
        {
            int index = tabs.IndexOf(tab);
            if (index >= 0) SelectTab(index);
        }
    }
}
