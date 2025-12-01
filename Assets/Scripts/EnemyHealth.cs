using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float damageAmount = 10f; // Oyuncuya verilecek hasar miktarı

    // Bir çarpışma başladığında bu fonksiyon çağrılır
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Çarpışmanın gerçekleştiği objenin "Player" etiketi var mı kontrol et
        if (collision.gameObject.CompareTag("Player"))
        {
            // Oyuncunun PlayerHealth script'ine eriş
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

            // Script'e erişebildiysek canını azalt
            if (playerHealth != null)
            {
                playerHealth.GetDamage(damageAmount);
            }
        }
    }
    
}
