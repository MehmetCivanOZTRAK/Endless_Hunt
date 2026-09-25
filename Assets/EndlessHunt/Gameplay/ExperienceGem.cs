using UnityEngine;
using DG.Tweening;

namespace EndlessHunt
{
    public sealed class ExperienceGem : MonoBehaviour
    {
        public SpriteRenderer spriteRenderer;
        private SurvivalGame _game;
        private int _amount;
        public void Bind(SurvivalGame game) => _game = game;

        public void Activate(Vector3 position, int amount)
        {
            transform.position = position; _amount = amount;
            spriteRenderer.color = Color.white;
            spriteRenderer.sortingOrder=6;
            float height=spriteRenderer.sprite?spriteRenderer.sprite.bounds.size.y:1;
            transform.localScale = Vector3.one * ((amount>1?.46f:.34f)/Mathf.Max(.01f,height));
            gameObject.SetActive(true);
            transform.DOKill();var target=transform.localScale;transform.localScale=target*.6f;transform.DOScale(target,.18f).SetEase(Ease.OutBack);
        }
        public void Accumulate(int amount) => _amount+=Mathf.Max(0,amount);
        private void OnDisable()=>transform.DOKill();

        private void Update()
        {
            if (_game == null || _game.Player == null || Time.timeScale==0) return;
            float distance = Vector2.Distance(transform.position, _game.Player.transform.position);
            if (distance < _game.PickupRadius)
                transform.position = Vector3.MoveTowards(transform.position, _game.Player.transform.position,
                    Time.deltaTime * 9f);
            if (distance >= .42f) return;
            _game.AddExperience(_amount);
            gameObject.SetActive(false);
        }
    }
}
