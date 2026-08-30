using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessHunt.UI.Common
{
    public enum UICurrencyType { Coins, Gems, Energy }

    [DisallowMultipleComponent]
    public sealed class UICurrencyDisplay : MonoBehaviour
    {
        [SerializeField] private UICurrencyType currencyType;
        [SerializeField] private GameProfileUIAdapter profileAdapter;
        [SerializeField] private Image icon;
        [SerializeField] private Text valueText;
        [SerializeField] private Text timerText;
        [SerializeField] private bool useThousandsSeparator = true;

        private Coroutine _energyRefresh;

        public UICurrencyType CurrencyType => currencyType;

        private void OnEnable()
        {
            if (profileAdapter == null) profileAdapter = GetComponentInParent<GameProfileUIAdapter>();
            if (profileAdapter == null) profileAdapter = FindFirstObjectByType<GameProfileUIAdapter>();
            if (profileAdapter == null) return;
            profileAdapter.ProfileChanged += HandleProfileChanged;
            profileAdapter.Refresh(true);
            if (currencyType == UICurrencyType.Energy)
                _energyRefresh = StartCoroutine(RefreshEnergyTimer());
        }

        private void OnDisable()
        {
            if (profileAdapter != null) profileAdapter.ProfileChanged -= HandleProfileChanged;
            if (_energyRefresh != null) StopCoroutine(_energyRefresh);
            _energyRefresh = null;
        }

        public void Configure(UICurrencyType type, GameProfileUIAdapter adapter, Image currencyIcon,
            Text value, Text timer = null)
        {
            currencyType = type;
            profileAdapter = adapter;
            icon = currencyIcon;
            valueText = value;
            timerText = timer;
        }

        public void SetIcon(Sprite sprite)
        {
            if (icon != null) icon.sprite = sprite;
        }

        public void Refresh()
        {
            if (profileAdapter == null) return;
            profileAdapter.Refresh();
            UpdateValue(profileAdapter.ReadSnapshot());
        }

        public static string FormatValue(UICurrencyType type, int value, int maximum, bool separator = true)
        {
            if (type == UICurrencyType.Energy) return $"{value}/{maximum}";
            return separator ? value.ToString("N0") : value.ToString();
        }

        private void HandleProfileChanged(GameProfileUIAdapter.Snapshot snapshot) => UpdateValue(snapshot);

        private void UpdateValue(GameProfileUIAdapter.Snapshot snapshot)
        {
            if (valueText == null || profileAdapter == null) return;
            int value = currencyType switch
            {
                UICurrencyType.Coins => snapshot.Coins,
                UICurrencyType.Gems => snapshot.Gems,
                _ => snapshot.Energy
            };
            valueText.text = FormatValue(currencyType, value, profileAdapter.GetEnergyMaximum(), useThousandsSeparator);
            if (timerText != null)
            {
                timerText.gameObject.SetActive(currencyType == UICurrencyType.Energy);
                if (currencyType == UICurrencyType.Energy) timerText.text = profileAdapter.GetEnergyTimer();
            }
        }

        private IEnumerator RefreshEnergyTimer()
        {
            var wait = new WaitForSecondsRealtime(1f);
            while (enabled)
            {
                profileAdapter.Refresh();
                UpdateValue(profileAdapter.ReadSnapshot());
                yield return wait;
            }
        }
    }
}
