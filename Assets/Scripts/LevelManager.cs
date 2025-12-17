using System;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [Header("Bağlantılar")]
    [SerializeField] private DiamondBar _diamondBar;
    [SerializeField] private Text _levelText;

    [Header("Spawner Ayarları")]
    [SerializeField] private Spawner[] _spawners; 

    [Header("Level Canavarları")]
    [SerializeField] private GameObject[] _enemyPrefabs; 

    [SerializeField] private int _levellimit = 3;

    private int _currentLevel = -1; 

    private void Start()
    {
        if (_diamondBar != null)
            _diamondBar.OnBarFilled += LevelUp;

        // ÖNEMLİ: Oyun başladığında spawnerların hafızasını temizle
        ResetAllSpawners();

        // İlk leveli başlat
        LevelUp();
    }

    private void LevelUp()
    {
        _currentLevel++;

        UpdateLevelText();
        AddNewEnemyToSpawners(); 

        Debug.Log($"Level {_currentLevel + 1} başladı! Yeni canavar havuza eklendi.");
    }

    private void AddNewEnemyToSpawners()
    {

        if(_currentLevel % _levellimit != 0)
        {
            return;
        }

        int enemyindex = _currentLevel / _levellimit;
        
        if (enemyindex >= _enemyPrefabs.Length) return;
        
        GameObject newEnemyType = _enemyPrefabs[enemyindex];

        foreach (var spawner in _spawners)
        {
            if (spawner != null)
            {
                if (!spawner.gameObject.activeSelf) 
                    spawner.gameObject.SetActive(true);

                // Eskileri silmeden YENİSİNİ EKLE
                spawner.AddEnemyToPool(newEnemyType);
                Debug.Log($"Spawner'a yeni canavar eklendi: {newEnemyType.name}");
            }
        }
    }

    private void ResetAllSpawners()
    {
        foreach (var spawner in _spawners)
        {
            if (spawner != null)
            {
                spawner.ClearEnemyPool(); // Spawner içindeki listeyi boşalt
            }
        }
    }

    private void UpdateLevelText()
    {
        if (_levelText != null)
        {
            _levelText.text = "Level: " + (_currentLevel + 1);
        }
    }

    private void OnDestroy()
    {
        if (_diamondBar != null)
            _diamondBar.OnBarFilled -= LevelUp;
    }
}