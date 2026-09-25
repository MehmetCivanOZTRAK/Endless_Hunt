using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
namespace EndlessHunt.UI
{
    public sealed class PreparedDialog : MonoBehaviour
    {
        public RectTransform card;
        public CanvasGroup group;
        public Button[] closeButtons;
        Vector3 _scale;
        bool _bound;
        void Bind()
        {
            if (_bound) return; _bound = true; _scale = card.localScale;
            foreach (var button in closeButtons) if (button) button.onClick.AddListener(Close);
        }
        public void Open()
        {
            Bind(); gameObject.SetActive(true); transform.SetAsLastSibling();
            group.DOKill(); card.DOKill(); group.alpha = 0; card.localScale = _scale * .94f;
            group.DOFade(1, .18f).SetUpdate(true); card.DOScale(_scale, .2f).SetEase(Ease.OutCubic).SetUpdate(true);
        }
        public void Close() { group.DOKill(); card.DOKill(); gameObject.SetActive(false); }
        void OnDisable() { if (group) group.DOKill(); if (card) card.DOKill(); }
    }
}
