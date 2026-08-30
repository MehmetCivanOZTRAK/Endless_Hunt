using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessHunt.UI.Common
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class UIToast : MonoBehaviour
    {
        [SerializeField] private Text messageText;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform animatedRoot;
        [SerializeField] private UIAnimationController animationController;
        [SerializeField, Min(.2f)] private float defaultDuration = 1.4f;

        private Coroutine _routine;

        public void Configure(Text message, CanvasGroup group, RectTransform root, UIAnimationController animations)
        {
            messageText = message;
            canvasGroup = group;
            animatedRoot = root;
            animationController = animations;
        }

        public void Show(string message, float duration = -1f)
        {
            if (_routine != null) StopCoroutine(_routine);
            gameObject.SetActive(true);
            if (messageText != null) messageText.text = message ?? string.Empty;
            _routine = StartCoroutine(ShowRoutine(duration > 0f ? duration : defaultDuration));
        }

        public void HideImmediate()
        {
            if (_routine != null) StopCoroutine(_routine);
            _routine = null;
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        private IEnumerator ShowRoutine(float duration)
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (animatedRoot == null) animatedRoot = transform as RectTransform;
            canvasGroup.alpha = 0f;
            animatedRoot.localScale = Vector3.one * .94f;
            if (animationController != null)
                animationController.FadeScale(canvasGroup, animatedRoot, true, .14f);
            else
                canvasGroup.alpha = 1f;
            yield return new WaitForSecondsRealtime(Mathf.Max(.2f, duration));
            bool finished = false;
            if (animationController != null)
                animationController.FadeScale(canvasGroup, animatedRoot, false, .14f, () => finished = true);
            else
                finished = true;
            while (!finished) yield return null;
            _routine = null;
            gameObject.SetActive(false);
        }
    }
}
