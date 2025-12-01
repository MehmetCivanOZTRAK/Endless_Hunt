using UnityEngine;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    // Düşman havuzu
    private List<GameObject> enemyPool = new List<GameObject>(); 

    [Header("Spawn Ayarları")]
    [SerializeField] private float sizeX = 10f; // Spawn alanının genişliği
    [SerializeField] private float sizeY = 10f; // Spawn alanının yüksekliği
    [SerializeField] private float spawnCooldown = 2f;

    private float spawnTimer;

    private void Start()
    {
        spawnTimer = spawnCooldown;
    }

    private void Update()
    {
        if (enemyPool.Count == 0) return;

        if (spawnTimer > 0)
        {
            spawnTimer -= Time.deltaTime;
        }
        else
        {
            Spawn();
            spawnTimer = spawnCooldown;
        }
    }

    private void Spawn()
    {
        // Havuzdan rastgele bir düşman seç
        int randomIndex = Random.Range(0, enemyPool.Count);
        GameObject selectedEnemy = enemyPool[randomIndex];

        // Rastgele pozisyon belirle
        float xPos = (Random.value - 0.5f) * sizeX + transform.position.x;
        float yPos = (Random.value - 0.5f) * sizeY + transform.position.y;
        
        Instantiate(selectedEnemy, new Vector3(xPos, yPos, 0), Quaternion.identity);
    }

    // Yeni level'da yeni düşmanı havuza ekleyen fonksiyon
    public void AddEnemyToPool(GameObject newEnemy)
    {
        if (!enemyPool.Contains(newEnemy))
        {
            enemyPool.Add(newEnemy);
        }
    }

    public void ClearEnemyPool()
    {
        enemyPool.Clear();
    }
    
}