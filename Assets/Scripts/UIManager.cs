using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text TitleText;
    public Text MaxScoreText;
    public Text ScoreText;
    public Text StatusText;

    public Button RestartButton;

    public void UpdateUI()
    {
        ScoreText.text = "Score: " + GameManager.instance.Score.ToString();
        StatusText.gameObject.SetActive(GameManager.instance.IsGameOver);
        RestartButton.gameObject.SetActive(GameManager.instance.IsGameOver);
        MaxScoreText.text = "Max Score: " + GameManager.instance.MaxScore.ToString();
    }

    public void Restart()
    {
        GameManager.instance.RestartGame();
    }

}
