using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessHunt.UI.Common
{
    [DisallowMultipleComponent]
    public sealed class UIProgressBar : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private Text valueText;
        [SerializeField] private bool showValueText = true;
        [SerializeField] private bool showAsPercent;

        private Coroutine _animation;
        private float _current;
        private float _maximum = 1f;

        public float NormalizedValue => slider != null ? slider.value : 0f;

        public void Configure(Slider targetSlider, Text targetText)
        {
            slider = targetSlider;
            valueText = targetText;
            SetNormalized(0f);
        }

        public void SetNormalized(float normalized, bool animate = false, float duration = .18f)
        {
            _current = Mathf.Clamp01(normalized);
            _maximum = 1f;
            SetTarget(_current, animate, duration);
        }

        public void SetValues(float current, float maximum, bool animate = false, float duration = .18f)
        {
            _current = Mathf.Max(0f, current);
            _maximum = Mathf.Max(.0001f, maximum);
            SetTarget(Mathf.Clamp01(_current / _maximum), animate, duration);
        }

        public void SetTextMode(bool visible, bool percent)
        {
            showValueText = visible;
            showAsPercent = percent;
            UpdateText();
        }

        private void SetTarget(float normalized, bool animate, float duration)
        {
            if (slider == null) return;
            if (_animation != null) StopCoroutine(_animation);
            if (animate && Application.isPlaying)
                _animation = StartCoroutine(AnimateTo(normalized, Mathf.Clamp(duration, .05f, .5f)));
            else
            {
                slider.value = normalized;
                UpdateText();
            }
        }

        private IEnumerator AnimateTo(float target, float duration)
        {
            float from = slider.value;
            float time = 0f;
            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                slider.value = Mathf.Lerp(from, target, time / duration);
                UpdateText();
                yield return null;
            }
            slider.value = target;
            UpdateText();
            _animation = null;
        }

        private void UpdateText()
        {
            if (valueText == null) return;
            valueText.gameObject.SetActive(showValueText);
            if (!showValueText) return;
            valueText.text = showAsPercent ? $"{Mathf.RoundToInt(NormalizedValue * 100f)}%" :
                $"{Mathf.RoundToInt(_current)}/{Mathf.RoundToInt(_maximum)}";
        }
    }
}
