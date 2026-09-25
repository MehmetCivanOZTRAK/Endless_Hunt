using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessHunt
{
    public sealed class BattleFeedback : MonoBehaviour
    {
        public Slider health, delayedHealth;
        public Image damageOverlay;
        public Text healthLabel;
        public Text[] damageLabels;
        private float _lastHealth = 1f;
        private int _nextLabel;
        private Tween _healthTween, _trailTween, _overlayTween;

        public void SetHealth(float value)
        {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(value, _lastHealth)) return;
            bool healing = value > _lastHealth;
            _lastHealth = value;
            _healthTween?.Kill(); _trailTween?.Kill();
            _healthTween = health.DOValue(value, .14f).SetUpdate(true);
            _trailTween = delayedHealth.DOValue(value, .42f).SetDelay(healing ? 0f : .28f).SetEase(Ease.OutCubic).SetUpdate(true);
            if (healthLabel != null) healthLabel.text = Mathf.CeilToInt(value * 100f) + "%";
        }

        public void Damage(float value)
        {
            SetHealth(value);
            _overlayTween?.Kill();
            damageOverlay.color = new Color(.8f,.02f,.03f,.2f);
            _overlayTween = damageOverlay.DOFade(0f, .38f).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        public void Impact(Vector3 worldPosition, float amount, Camera worldCamera)
        {
            if (!LocalPlayerStore.Data.damageNumbers || damageLabels == null || damageLabels.Length == 0) return;
            Text label = damageLabels[_nextLabel++ % damageLabels.Length];
            RectTransform rect = label.rectTransform;
            Canvas canvas = label.canvas;
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)rect.parent,
                worldCamera.WorldToScreenPoint(worldPosition + Vector3.up * .7f),
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out Vector2 local);
            label.DOKill(); rect.DOKill();
            rect.anchoredPosition = local; rect.localScale = Vector3.one;
            label.text = Mathf.RoundToInt(amount).ToString(); label.color = new Color(1f,.9f,.4f,1f);
            rect.DOAnchorPosY(local.y + 85f, .48f).SetEase(Ease.OutCubic);
            rect.DOPunchScale(Vector3.one * .18f, .18f, 1, .2f);
            label.DOFade(0f, .22f).SetDelay(.24f);
        }

        private void OnDestroy()
        {
            _healthTween?.Kill(); _trailTween?.Kill(); _overlayTween?.Kill();
            if (damageLabels == null) return;
            foreach (Text label in damageLabels) if (label != null) { label.DOKill(); label.rectTransform.DOKill(); }
        }
    }
}
