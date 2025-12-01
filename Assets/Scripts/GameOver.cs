using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    [SerializeField] private Text _highscore;
    [SerializeField] private Text _score;

    public void SetScores(int score)
    {
        _score.text = "YOUR SCORE: " + score.ToString();
         _highscore.text = "HIGH SCORE: " + PlayerPrefs.GetInt("highScore");

    }
    public void RestartGame()
    {
        Score.score = 0;
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game Scene");
    }
    public void ExitToMainMenu()
    {
        Score.score = 0;
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
  
}
