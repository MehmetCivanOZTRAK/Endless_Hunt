using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float _maxHealth;
    [SerializeField] private GameObject _gameoverpanel;
    private bool _isDead = false;
    private Animator _animator;
    private Rigidbody2D _rigidbody2D;
    private PlayerController _playerController;
    private float _currentHealth;
    public HealthBar healthBar;
    
    private readonly int DEATH_HASH = Animator.StringToHash("isDead");

    private void Start()
    {
        _isDead = false;
        _currentHealth = _maxHealth;
        healthBar.SetMaxHealth(_maxHealth);
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _playerController = GetComponent<PlayerController>();

    }

   /* private void Update()
    {
        if (_currentHealth <= 0 && !_isDead)
        {
            _isDead = true;
            Destroy(gameObject);
            _currentHealth = 0;
            _animator.SetBool(DEATH_HASH, true);
            StartCoroutine(DeathLoading());
        }
    }*/
    private IEnumerator DeathLoading()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        EndGamePanel();
       
        
    }
    public void GetDamage(float damage)
    {
        //if (_isDead) return;
        _currentHealth -= damage;
        healthBar.SetHealth(_currentHealth);
        if (_currentHealth <= 0)
        {
            Die();
            _currentHealth = 0;
            
        }
    }
    private void Die()
    {
       if(_isDead) return;
        _isDead = true;

        if (_animator != null)
        {
            _animator.SetBool(DEATH_HASH, true);
        }
        if (_playerController != null)
        {
            _playerController.OnDeath();
        }
        if (_rigidbody2D != null)
        {
            _rigidbody2D.linearVelocity = Vector2.zero;
            _rigidbody2D.angularVelocity = 0f;
        }
        StartCoroutine(DeathLoading());
        
    }
    public void EndGamePanel()
    {
        Time.timeScale = 0f;
        _gameoverpanel.SetActive(true);
        _gameoverpanel.GetComponent<GameOver>();
        if (_gameoverpanel != null)
        {
            _gameoverpanel.GetComponent<GameOver>().SetScores(Score.score);
        }
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            GetDamage(10f);
        }
    }

    public void AddHealth(float health)
    {
        _currentHealth += health;
        if (_currentHealth > _maxHealth)
        {
            _currentHealth = _maxHealth;
        }
        healthBar.SetHealth(_currentHealth);
    }


}
