using System;
using System.Collections;
using UnityEngine;

namespace EndlessHunt.UI
{
    [DisallowMultipleComponent]
    public sealed class UIAnimationController : MonoBehaviour
    {
        [SerializeField, Range(.05f, .5f)] private float defaultDuration = .18f;

        public float DefaultDuration => defaultDuration;

        public Coroutine Fade(CanvasGroup target, float to, float duration = -1f, Action completed = null)
        {
            if (target == null) { completed?.Invoke(); return null; }
            return StartCoroutine(FadeRoutine(target, target.alpha, to, ResolveDuration(duration), completed));
        }

        public Coroutine Scale(RectTransform target, Vector3 to, float duration = -1f, Action completed = null)
        {
            if (target == null) { completed?.Invoke(); return null; }
            return StartCoroutine(ScaleRoutine(target, target.localScale, to, ResolveDuration(duration), completed));
        }

        public Coroutine Slide(RectTransform target, Vector2 to, float duration = -1f, Action completed = null)
        {
            if (target == null) { completed?.Invoke(); return null; }
            return StartCoroutine(SlideRoutine(target, target.anchoredPosition, to, ResolveDuration(duration), completed));
        }

        public Coroutine FadeScale(CanvasGroup group, RectTransform target, bool show, float duration = -1f,
            Action completed = null)
        {
            if (group == null || target == null) { completed?.Invoke(); return null; }
            return StartCoroutine(FadeScaleRoutine(group, target, show, ResolveDuration(duration), completed));
        }

        public Coroutine ButtonPress(RectTransform target, float duration = .1f)
        {
            return target == null ? null : StartCoroutine(ButtonPressRoutine(target, Mathf.Max(.05f, duration)));
        }

        private float ResolveDuration(float duration) => duration > 0f ? duration : defaultDuration;
        private static float Delta => Time.unscaledDeltaTime;
        private static float Ease(float value) => 1f - Mathf.Pow(1f - Mathf.Clamp01(value), 3f);

        private static IEnumerator FadeRoutine(CanvasGroup target, float from, float to, float duration, Action completed)
        {
            float time = 0f;
            while (time < duration)
            {
                time += Delta;
                target.alpha = Mathf.LerpUnclamped(from, to, Ease(time / duration));
                yield return null;
            }
            target.alpha = to;
            completed?.Invoke();
        }

        private static IEnumerator ScaleRoutine(RectTransform target, Vector3 from, Vector3 to, float duration, Action completed)
        {
            float time = 0f;
            while (time < duration)
            {
                time += Delta;
                target.localScale = Vector3.LerpUnclamped(from, to, Ease(time / duration));
                yield return null;
            }
            target.localScale = to;
            completed?.Invoke();
        }

        private static IEnumerator SlideRoutine(RectTransform target, Vector2 from, Vector2 to, float duration, Action completed)
        {
            float time = 0f;
            while (time < duration)
            {
                time += Delta;
                target.anchoredPosition = Vector2.LerpUnclamped(from, to, Ease(time / duration));
                yield return null;
            }
            target.anchoredPosition = to;
            completed?.Invoke();
        }

        private static IEnumerator FadeScaleRoutine(CanvasGroup group, RectTransform target, bool show,
            float duration, Action completed)
        {
            float startAlpha = group.alpha;
            float endAlpha = show ? 1f : 0f;
            Vector3 startScale = target.localScale;
            Vector3 endScale = show ? Vector3.one : Vector3.one * .92f;
            if (show && startAlpha <= .001f) startScale = Vector3.one * .92f;
            float time = 0f;
            while (time < duration)
            {
                time += Delta;
                float eased = Ease(time / duration);
                group.alpha = Mathf.LerpUnclamped(startAlpha, endAlpha, eased);
                target.localScale = Vector3.LerpUnclamped(startScale, endScale, eased);
                yield return null;
            }
            group.alpha = endAlpha;
            target.localScale = endScale;
            completed?.Invoke();
        }

        private static IEnumerator ButtonPressRoutine(RectTransform target, float duration)
        {
            Vector3 original = target.localScale;
            float half = duration * .5f;
            float time = 0f;
            while (time < half)
            {
                time += Delta;
                target.localScale = Vector3.Lerp(original, original * .94f, time / half);
                yield return null;
            }
            time = 0f;
            while (time < half)
            {
                time += Delta;
                target.localScale = Vector3.Lerp(original * .94f, original, time / half);
                yield return null;
            }
            target.localScale = original;
        }
    }
}
