using UnityEngine;

public class ItemController : MonoBehaviour
{
    [SerializeField] private GameObject item;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.AddHealth(20);
            }
           

            Destroy(item);
        }



    }
}
