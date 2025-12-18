using System.Net.NetworkInformation;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Ayarlar")]
    [SerializeField] private float speed;
    [SerializeField] private float stoppingDistance;
    public float DamageToPlayerAmount = 10f;
    
    [Header("Efektler ve Prefablar")]
    [SerializeField] private GameObject diamondPrefab;
    [SerializeField] private GameObject damagePopUpPrefab;
    [SerializeField] private int DamageNumber = 10;
    [SerializeField] private GameObject vaporizationEffectPrefab; // YENİ: Buharlaşma efekti
    
    [Range(0f, 100f)]
    [SerializeField] private float diamondDropChance = 100f;

    [SerializeField] private Score scoreManager;

    private Rigidbody2D rb;
    private Transform _player;
    private Animator _animator;
    [SerializeField]private int health = 1; // Canı 1 ise tek vuruşta ölür
    private bool _isDead = false; // Çift ölümü engellemek için kontrol

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
        // Eğer öldüyse hareket etmesin
        if (_isDead) return;

        EnemyFollow();
        
        // Buradaki ölüm kontrolünü kaldırdım, aşağıya (Die fonksiyonuna) taşıdım.
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

            if (_animator != null)
            {
                _animator.SetFloat("Horizontal", direction.x);
                _animator.SetFloat("Vertical", direction.y);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_isDead) return; // Ölü düşman hasar veremez

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.GetDamage(DamageToPlayerAmount);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isDead) return;

        if (collision.tag == "Bullet")
        {
            TakeDamage(1); // Hasar alma işlemini fonksiyona çevirdim
            Destroy(collision.gameObject);
        }
    }

    // Hasar alma ve Ölüm kontrolü
    public void TakeDamage(int damage)
    {
        health -= damage;

        // Hasar yazısı (PopUp)
        if(damagePopUpPrefab != null)
        {   
            Vector3 randomOffset = new Vector3(Random.Range(-0.2f, 0.2f), 0.7f, 0);
            GameObject damagePopUp = Instantiate(damagePopUpPrefab, transform.position + randomOffset, Quaternion.identity);
            damagePopUp.GetComponent<DamagePopUp>().Setup(DamageNumber);
        }

        // Ölüm Kontrolü
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (_isDead) return; // Zaten öldüyse tekrar çalışma
        _isDead = true;

        // 1. Buharlaşma Efektini Yarat
        if (vaporizationEffectPrefab != null)
        {
            Instantiate(vaporizationEffectPrefab, transform.position, Quaternion.identity);
        }

        // 2. Skoru Güncelle
        if (scoreManager != null) 
            scoreManager.ScoreUpdate(10);

        // 3. Elmas Düşür
        DropDiamond();

        // 4. Düşmanı Yok Et
        Destroy(gameObject);
    }

    private void DropDiamond()  
    {
        float randomValue = Random.Range(0f, 100f);
        if (randomValue <= diamondDropChance && diamondPrefab != null)
        {
            Instantiate(diamondPrefab, transform.position, Quaternion.identity);
        }
    }
}