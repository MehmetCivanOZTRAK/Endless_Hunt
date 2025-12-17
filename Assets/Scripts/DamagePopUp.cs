using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DamagePopUp : MonoBehaviour
{
   [SerializeField] private TextMeshPro _textDamage;
    [SerializeField] private float _disapperTimer = 1f;
    private Vector3 _moveVector;
    private Color _textColor;
    


    public void Setup(int damageAmount)
    {
        _textDamage.text = damageAmount.ToString();
        _textColor = _textDamage.color;

        _moveVector = new Vector3(Random.Range(-0.5f, 0.5f), 1f, 0);
    }
    private void Update()
    {
        transform.position += _moveVector * Time.deltaTime;

        _moveVector -= _moveVector * 8f * Time.deltaTime;

        _disapperTimer -= Time.deltaTime;

        if (_disapperTimer <= 0)
        {
            float fadeSpeed = 2f;
            _textColor.a -= fadeSpeed * Time.deltaTime;
            _textDamage.color = _textColor;
            if(_textColor.a <= 0)
            {
                Destroy(gameObject);
            }
        }

    }
}
