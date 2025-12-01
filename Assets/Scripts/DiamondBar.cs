using UnityEngine;
using UnityEngine.UI;


public class DiamondBar : MonoBehaviour
{
    [SerializeField] private Slider fillSlider;

    public System.Action OnBarFilled;
    private float currentvalue = 0f;
    private float maxvalue = 100f;

    public void AddDiamonds(float amount)
    {
        currentvalue += amount;
        currentvalue = Mathf.Clamp(currentvalue, 0f, maxvalue);
        fillSlider.value = currentvalue / maxvalue;
    }
    private void Update()
    {
        if (Mathf.Approximately(currentvalue, maxvalue))
        {
            OnBarFilled?.Invoke();
            currentvalue = 0f;
            fillSlider.value = currentvalue / maxvalue;
        }
    }   
}
