using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Paneller")]
    [SerializeField] private GameObject settingspanel;
    [SerializeField] private GameObject quitpanel;
    [SerializeField] private GameObject mappanel;
    [SerializeField] private GameObject languagepanel;
    
    // Zaten var olan MenuPanel'in ne işe yaradığını bilmediğim için dokunmadım
    [SerializeField] private GameObject MenuPanel; 

    [Header("Ana Ekran Objeleri")]
    // BURASI YENİ: Kaybolmasını istediğin butonların olduğu ana grup
    [SerializeField] private GameObject mainInterfaceGroup; 

    public void GameStart1()
    {
        SceneManager.LoadScene("GameScene2");
    }
    public void GameStart2()
    {
        SceneManager.LoadScene("GameScene");
    }

    // --- AYARLAR PANELİ ---
    public void OptionOpen()
    {
        settingspanel.SetActive(true);
        mainInterfaceGroup.SetActive(false); // Arkadakileri gizle
    }  
    public void OptionExit()
    {
        settingspanel.SetActive(false);
        mainInterfaceGroup.SetActive(true); // Arkadakileri geri getir
    }

    // --- ÇIKIŞ PANELİ ---
    public void QuitOpen()
    {
        quitpanel.SetActive(true);
        mainInterfaceGroup.SetActive(false); // Arkadakileri gizle
    }
    public void QuitExit()
    {
        quitpanel.SetActive(false);
        mainInterfaceGroup.SetActive(true); // Arkadakileri geri getir
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void MenuOpenExit()
    {
        bool isActive = MenuPanel.activeSelf;
        MenuPanel.SetActive(!isActive);
    }

    // --- HARİTA PANELİ (Senin asıl sorduğun yer) ---
    public void MapOpen()
    {
        mappanel.SetActive(true);       // Haritayı aç
        mainInterfaceGroup.SetActive(false); // Ana butonları/simgeleri gizle
    }
    public void MapExit()
    {
        mappanel.SetActive(false);      // Haritayı kapat
        mainInterfaceGroup.SetActive(true);  // Ana butonları geri getir
    }

    // --- DİL PANELİ ---
    public void LanguageOpen()
    {
        languagepanel.SetActive(true);
        mainInterfaceGroup.SetActive(false); // Arkadakileri gizle
    }
    public void LanguageExit()
    {
        languagepanel.SetActive(false);
        mainInterfaceGroup.SetActive(true); // Arkadakileri geri getir
    }
    
}