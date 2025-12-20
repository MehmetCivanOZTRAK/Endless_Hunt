using UnityEngine;

public class DamageController : MonoBehaviour
{
        [SerializeField] private GameObject _player;
        private Vector3 originalPosition;
        private float shakeDuration;
        private float shakeAmount;
        [SerializeField] private float isShakingtimer;
        private bool isShaking;

        private Color originalColor;

        public void GetHit()
        {       
                originalColor = _player.GetComponent<SpriteRenderer>().color;
                _player.GetComponent<SpriteRenderer>().color = Color.red;
                originalPosition = transform.position;
                ShakeIt(isShakingtimer, 0.1f);
                Invoke(nameof(ColorReset), isShakingtimer);
        }

        private void ShakeIt(float duration, float amount)
        {
                shakeDuration = duration;
                shakeAmount = amount;
                isShaking = true;

        }
        private void ShakeItControl()
        {
                if (isShaking == true)
                {
                        if (shakeDuration > 0)
                        {
                                _player.transform.position = originalPosition + Random.insideUnitSphere * shakeAmount;
                                shakeDuration -= Time.deltaTime;
                                
                        }
                        else
                        {
                                _player.transform.position = originalPosition;
                                isShaking = false;
                        }

                }
               

        }

        private void ColorReset()
        {
                _player.GetComponent<SpriteRenderer>().color = originalColor;
        }

        private void Update()
        {
                ShakeItControl();

        }
}

