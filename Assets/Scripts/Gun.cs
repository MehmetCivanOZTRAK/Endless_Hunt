using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{

    [Header("Gun Settings")]

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float _fireRate = 0.5f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float detectorRadius = 5f;

    [Header("Joystick Settings")]
    [SerializeField] private Joystick _joystick;
    [SerializeField] private GameObject crosshair;

    [Header("Ammo Settings")]
    [SerializeField] private AmmoBarUI ammoBarUI;
    [SerializeField] private float maxAmmo = 50f;
   // [SerializeField] private float ammoPerShot = 10f;
    [SerializeField] private float reFillRate = 50f;
    [SerializeField] private float minAmmoToFire = 30f;

    [Header("Animation Settings")]
    [SerializeField] private Animator gunAnimator;

    private bool canShoot = true;
    private float currentAmmo;
    private float _firetimer;
    private Vector2 lastDirection = Vector2.right;
    private Vector3 gunDirection;
    private Transform nearestEnemy;

    private void Start()
    {
        currentAmmo = maxAmmo;
        if(ammoBarUI != null)
        {
            ammoBarUI.SetFillNormalized(currentAmmo / maxAmmo);
        }
    }
    private void Update()
    {
        nearestEnemy = FindNearestEnemy();

        if (currentAmmo < maxAmmo)
        {
            currentAmmo += reFillRate * Time.deltaTime;
            currentAmmo = Mathf.Clamp(currentAmmo, 0, maxAmmo);
            if (ammoBarUI != null)
            {
                ammoBarUI.SetFillNormalized(currentAmmo / maxAmmo);
            }
        }
        canShoot = Mathf.Approximately(currentAmmo, maxAmmo);


        if (nearestEnemy != null)
        {
            gunDirection = (nearestEnemy.position - transform.position).normalized;
        }
        else
        {
            gunDirection = new Vector2(_joystick.Horizontal, _joystick.Vertical);
        }

        if (gunDirection.sqrMagnitude > 0.1f)
        {
            lastDirection = gunDirection.normalized;


            float rotateZ = Mathf.Atan2(lastDirection.y, lastDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, rotateZ));

            if (crosshair != null)
                crosshair.transform.position = transform.position + (Vector3)(lastDirection * 2f);

        }
        if (nearestEnemy != null && currentAmmo >= minAmmoToFire)
        {
            _firetimer += Time.deltaTime;
            if (_firetimer >= _fireRate)
            {
                Shoot();
                _firetimer = 0f;

            }
        }



    }
    private void Shoot()
    {
        if (!canShoot)
            return;

        currentAmmo = 0f;
        if (ammoBarUI != null)
        {
            ammoBarUI.SetFillNormalized(currentAmmo / maxAmmo);
        }
        canShoot = false;


        /*  if (currentAmmo < minAmmoToFire)
          {
              return;
          }
          currentAmmo -= ammoPerShot;
         */


        Transform target = FindNearestEnemy();
        if (target == null)
            return;


        float angle = Mathf.Atan2(gunDirection.y, gunDirection.x) * Mathf.Rad2Deg;
        bulletPrefab.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = lastDirection * bulletSpeed;

        }
        Destroy(bullet, 2f);

        if (gunAnimator != null)
        {
            gunAnimator.SetTrigger("isAttack");
            StartCoroutine(ResetCouroutine());
        }
    }
    private IEnumerator ResetCouroutine()
    {
        yield return new WaitForSeconds(1f);
        gunAnimator.ResetTrigger("isAttack");
    }

    private Transform FindNearestEnemy()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, detectorRadius, enemyLayer);
        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = enemy.transform;
            }
        }
        return nearest;
    }


}