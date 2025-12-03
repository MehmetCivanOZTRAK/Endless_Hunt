using UnityEngine;

public class PanelGecis : MonoBehaviour
{
    [Header("Ayarlar")]
    public GameObject kapanacakPanel; // Şu an açık olan (kapanmasını istediğin)
    public GameObject acilacakPanel;  // Yeni açılacak olan

    public void GecisYap()
    {
        // 1. Öncekini kapat
        if (kapanacakPanel != null)
        {
            kapanacakPanel.SetActive(false);
        }

        // 2. Yenisini aç
        if (acilacakPanel != null)
        {
            acilacakPanel.SetActive(true);
        }
    }
}