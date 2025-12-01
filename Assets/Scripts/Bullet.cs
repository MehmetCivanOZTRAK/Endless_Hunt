using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector2 target;
    [SerializeField] private float speed;
    [SerializeField] private float damage;

    private bool hasTarget = false;

    private void Start()
    {
        target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void Update()
    {
        if (!hasTarget) return;

        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
        Destroy(gameObject, 2f);
    }

    public void SetTarget(Vector2 newTarget)
    {
        target = newTarget;
        hasTarget = true;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyController enemy = collision.GetComponent<EnemyController>();

            
            Destroy(gameObject);
        }
        
    }
}
