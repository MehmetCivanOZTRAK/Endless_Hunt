using NUnit.Framework;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rigidbody2D;
    [SerializeField] private float speed = 2f;
    private Vector2 _moveVector;
    private Animator _animator;
    private bool isDead = false;

    [SerializeField] private DynamicJoystick joystick;
    private Vector3 defaultScale = new Vector3(1,1,1);

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        
    }
    private void Update()
    {
       /* if (isDead) return;
        _moveVector = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
            );*/
    }

    private void FixedUpdate()
    {
        Move();
    }
  

    private void Move()
    {
        if (isDead) return;
        
        float inputX = joystick.Horizontal;
        float inputY = joystick.Vertical;

        _moveVector = new Vector2(inputX, inputY).normalized;

        // Hareket varsa animasyonu güncelle
        if (_moveVector != Vector2.zero)
        {
            _animator.SetFloat("Horizontal", _moveVector.x);
            _animator.SetFloat("Vertical", _moveVector.y);

            // Son yönü kaydet
            _animator.SetFloat("LastInputX", _moveVector.x);
            _animator.SetFloat("LastInputY", _moveVector.y);

            _animator.SetBool("isWalking", true);
        }
        else
        {
            // Hareket yoksa, son yönü sabit tut
            _animator.SetFloat("Horizontal", _animator.GetFloat("LastInputX"));
            _animator.SetFloat("Vertical", _animator.GetFloat("LastInputY"));

            _animator.SetBool("isWalking", false);
        }

        // Hareketi uygula
        _rigidbody2D.linearVelocity = _moveVector * speed;
    }
    public void OnDeath()
    {
        isDead = true;
        _rigidbody2D.linearVelocity = Vector2.zero;
       // _animator.SetBool("isDead", true);

      
    }
}

