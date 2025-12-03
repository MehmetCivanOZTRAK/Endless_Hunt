using System;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class EnergySystem : MonoBehaviour
{
    [Header("Energy Settings")]
    [SerializeField] private int maxEnergy = 10;
    [SerializeField] private int EnergyMinutes = 5;
    [Header("UI Elements")]
    [SerializeField] private Text _energyText;
    [SerializeField] private Text _energyMinutesText;
    [SerializeField] private Button _StartButton;


    private int currentEnergy;
    private DateTime NextEnergyTime;
    private bool isEnergyTimerActive = false;

    private void Start()
    {
        UploadData();
        UpdateUI();
    }
    private void Update() {
        if(!isEnergyTimerActive) return;

        TimeSpan timeSpan = NextEnergyTime - DateTime.Now;

        if(timeSpan.TotalSeconds > 0)
        {
            _energyMinutesText.text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        }
        else
        {
            AddEnergy();
        }   
    }
    private void AddEnergy()
    {
        if (currentEnergy < maxEnergy)
        {
            currentEnergy++;
            NextEnergyTime = DateTime.Now.AddMinutes(EnergyMinutes);
            UploadData();

        }
    }
    public void StartGame(string LevelIndex)
    {
        if(currentEnergy > 1)
        {
            if(currentEnergy == maxEnergy)
            {
                NextEnergyTime = DateTime.Now.AddMinutes(EnergyMinutes);
                SaveData();
            }
            currentEnergy--;
            SaveData();
            UpdateUI();

            SceneManager.LoadScene(LevelIndex);
        }
        else
        {
            Debug.Log("Not Enough Energy");
        }
    }
    private void UploadData()
    {
        currentEnergy = PlayerPrefs.GetInt("Energy", maxEnergy);

        string dateTimeString = PlayerPrefs.GetString("NextEnergyTime", DateTime.Now.ToString());
        DateTime SaveTime = DateTime.Parse(dateTimeString);

        if (DateTime.Now >= SaveTime)
        {
            double dakikafarki = (DateTime.Now - SaveTime).TotalMinutes;
            NextEnergyTime = SaveTime;
        }
        else
        {
            NextEnergyTime = SaveTime;
        }

    }

    private void OpplicationFocus(bool focus)
    {
        if (focus)
        {
            OfflineCalcEnergy();
        }
    }
    private void OfflineCalcEnergy()
    {
        string dateTimeString = PlayerPrefs.GetString("NextEnergyTime", DateTime.Now.ToString());
        DateTime hedefTime = DateTime.Parse(dateTimeString);

        while(DateTime.Now >= hedefTime)
        {
            currentEnergy++;
            hedefTime = hedefTime.AddMinutes(EnergyMinutes);
          
        }
        NextEnergyTime = hedefTime;

    }
    private void SaveData()
    {
        PlayerPrefs.SetInt("Energy", currentEnergy);
        PlayerPrefs.SetString("NextEnergyTime", NextEnergyTime.ToString());
        PlayerPrefs.Save();
    }
    private void UpdateUI()
    {
        _energyText.text = (currentEnergy + "/10").ToString();
        if (currentEnergy >= maxEnergy)
        {
            _energyMinutesText.text = "Full";
            isEnergyTimerActive = false;
        }
        else
        {
            isEnergyTimerActive = true;
        }
        _StartButton.interactable = (currentEnergy > 0);
    }

}