using UnityEngine;
using UnityEngine.UI;


public class Score : MonoBehaviour
{
    public static int score = 0;
    [SerializeField] private Text scoreText;


    public void ScoreUpdate(int amount)
    {
        score += amount;
        scoreText.text = "SCORE : " + score.ToString();
    }
    private void Start()
    {
        ScoreUpdate(0);
    }
    private void Update()
    {
        SaveScore();
    }

    private void SaveScore()
    {
        if (score > PlayerPrefs.GetInt("highScore"))
        {
            PlayerPrefs.SetInt("highScore", score);
        }
        PlayerPrefs.Save();
    }
}
