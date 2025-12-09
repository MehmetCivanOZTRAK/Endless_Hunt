using UnityEngine;

public class DiamondPickUp : MonoBehaviour
{
   [SerializeField] private int diamondAmount = 10;
private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            DiamondBar diamondBar = FindAnyObjectByType<DiamondBar>();
            if (diamondBar != null)
            {
                diamondBar.AddDiamonds(diamondAmount);
            }
            Destroy(gameObject);
        }
    }
}
