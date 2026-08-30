using UnityEngine;

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
            spriteRenderer.color = amount > 1 ? new Color(.75f, .3f, 1f) : new Color(.2f, .9f, 1f);
            transform.localScale = Vector3.one * (amount > 1 ? .42f : .3f);
            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (_game == null || _game.Player == null) return;
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
