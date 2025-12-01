using UnityEngine;
using UnityEngine.UI;

public class AmmoBarUI : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private float _smoothSpeed = 5f;

    private float _targetValue;
    private float _currentValue;

    private void Update()
    {
        _currentValue = Mathf.MoveTowards(_currentValue, _targetValue, _smoothSpeed * Time.deltaTime);

        if (slider != null)
        {
            slider.value = _currentValue;
        }
    }

    public void SetFillNormalized(float normalized)
    {
        _targetValue = Mathf.Clamp01(normalized);
    }
}
