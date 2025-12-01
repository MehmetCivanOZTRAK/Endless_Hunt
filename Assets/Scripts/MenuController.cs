using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject settingspanel;
    [SerializeField] private GameObject quitpanel;
     [SerializeField] private GameObject scorepanel;
     [SerializeField] private GameObject mappanel;
   public void GameStart1()
    {
        SceneManager.LoadScene("GameScene2");
    }
    public void GameStart2()
    {
        SceneManager.LoadScene("GameScene");
    }
    public void OptionOpen()
    {
        settingspanel.SetActive(true);
    }
    public void OptionExit()
    {
        settingspanel.SetActive(false);
    }
    public void QuitOpen()
    {
        quitpanel.SetActive(true);
    }
    public void QuitExit()
    {
        quitpanel.SetActive(false);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void ScoreOpen()
    {
        scorepanel.SetActive(true);
    }
    public void ScoreExit()
    {
        scorepanel.SetActive(false);
    }
    public void MapOpen()
    {
        mappanel.SetActive(true);
    }
    public void MapExit()
    {
        mappanel.SetActive(false);
    }
    


}
