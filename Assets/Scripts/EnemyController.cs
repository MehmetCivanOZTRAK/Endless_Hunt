using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Transform _player;
    public float damageAmount = 10f;
    [SerializeField] private float speed;
    [SerializeField] private float stoppingDistance;
    [SerializeField] private GameObject diamondPrefab;
    [Range(0f, 100f)]
    [SerializeField] private float diamondDropChance = 100f;
 
    [SerializeField] private Score scoreManager;

    private Animator _animator;
    private int health = 1;

    private void Awake()
    {

        rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();

        if (scoreManager == null)
            scoreManager = FindFirstObjectByType<Score>();
        
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            _player = playerObject.GetComponent<Transform>();
        }
    }

    private void FixedUpdate()
    {
        EnemyFollow();
        if(health <= 0)
        {
            scoreManager.ScoreUpdate(10);
            Destroy(gameObject);
            DropDiamond();
        }
    }

    private void EnemyFollow()
    {

        if (_player == null || Vector2.Distance(transform.position, _player.position) <= stoppingDistance)
        {
            rb.linearVelocity = Vector2.zero;
        }
        else
        {

            Vector2 direction = (_player.position - transform.position).normalized;

            rb.linearVelocity = direction * speed;

            _animator.SetFloat("Horizontal", direction.x);
            _animator.SetFloat("Vertical", direction.y);
        }
    }



    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.GetDamage(damageAmount);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Bullet")
        {
            health--;
        }
    }
    private void DropDiamond()
    {
        float randomValue = Random.Range(0f, 100f);
        if (randomValue <= diamondDropChance)
        {
            Instantiate(diamondPrefab, transform.position, Quaternion.identity);
        }
    }
}
