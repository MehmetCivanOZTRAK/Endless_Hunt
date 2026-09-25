using DG.Tweening;
using UnityEngine;

namespace EndlessHunt
{
    public sealed class PopupMotion : MonoBehaviour
    {
        private CanvasGroup _group;
        private void OnEnable()
        {
            if (!Application.isPlaying) return;
            _group = GetComponent<CanvasGroup>();
            if (_group == null) return;
            _group.DOKill(); _group.alpha = 0f;
            _group.DOFade(1f, .18f).SetUpdate(true);
            foreach (Transform child in transform)
            {
                child.DOKill(); child.localScale = Vector3.one * .94f;
                child.DOScale(1f, .22f).SetEase(Ease.OutCubic).SetUpdate(true);
            }
        }
        private void OnDisable()
        {
            if (_group != null) _group.DOKill();
            foreach (Transform child in transform) child.DOKill();
        }
    }
}
