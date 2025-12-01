using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Item Spawner Settings")]
    [SerializeField] private GameObject[] itemPrefab;
    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private int maxitem;

    [Header("Spawn Area Settings")]
    [SerializeField] private Vector2 spawnAreaMin = new Vector2(-8f, -4f);
    [SerializeField] private Vector2 spawnAreaMax = new Vector2(8f, 4f);



    private float _spawnTimer;
    private void Update()
    {
        _spawnTimer += Time.deltaTime;
        if (_spawnTimer >= spawnInterval)
        {
            _spawnTimer = 0f;
            SpawnItem();
        }
    }
    private void SpawnItem()
    {
        if (GameObject.FindGameObjectsWithTag("Item").Length >= maxitem)
        {
            return;
        }
        GameObject item = itemPrefab[Random.Range(0, itemPrefab.Length)];


        Vector2 spawnPosition = new Vector2(
            Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            Random.Range(spawnAreaMin.x, spawnAreaMax.y)
            );

        Instantiate(item, spawnPosition, Quaternion.identity);
    }
}
