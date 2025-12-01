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
        AddNewEnemyToSpawners(); // İsim değişikliği: Update yerine Add

        Debug.Log($"Level {_currentLevel + 1} başladı! Yeni canavar havuza eklendi.");
    }

    private void AddNewEnemyToSpawners()
    {
        // Eğer level sayısı canavar sayısını geçerse hata vermesin, son canavarı eklemeye devam etmesin diye kontrol
        if (_currentLevel >= _enemyPrefabs.Length) return;

        GameObject newEnemyType = _enemyPrefabs[_currentLevel];

        foreach (var spawner in _spawners)
        {
            if (spawner != null)
            {
                if (!spawner.gameObject.activeSelf) 
                    spawner.gameObject.SetActive(true);

                // Eskileri silmeden YENİSİNİ EKLE
                spawner.AddEnemyToPool(newEnemyType);
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