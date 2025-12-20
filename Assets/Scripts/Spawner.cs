using UnityEngine;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    // Düşman havuzu
    private List<GameObject> enemyPool = new List<GameObject>();

    [Header("Spawn Ayarları")]
    [SerializeField] private float sizeX = 10f; // Spawn alanının genişliği
    [SerializeField] private float sizeY = 10f; // Spawn alanının yüksekliği

    [SerializeField] private int enemyspawnamount = 10; //dkda kaç düşman çıkacak  
    [SerializeField] private float spawnCooldown = 60f; //kaç snyde yeni grup çıksın

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
            SpawnWawe();
            spawnTimer = spawnCooldown;
        }
    }
    private void SpawnWawe()
    {
        for (int i = 0; i < enemyspawnamount; i++)
        {
            Spawn();
        }
    }

    private void Spawn()
    {

        int randomIndex = Random.Range(0, enemyPool.Count);
        GameObject selectedEnemy = enemyPool[randomIndex];


        float xPos = (Random.value - 0.5f) * sizeX + transform.position.x;
        float yPos = (Random.value - 0.5f) * sizeY + transform.position.y;

        Instantiate(selectedEnemy, new Vector3(xPos, yPos, 0), Quaternion.identity);
    }

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
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(sizeX, sizeY, 0));
    }

}