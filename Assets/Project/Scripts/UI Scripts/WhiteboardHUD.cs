using TMPro;
using UnityEngine;

public class WhiteboardHUD : MonoBehaviour
{

    [SerializeField] private TMP_Text gamemodeCondition;
    [SerializeField] private TMP_Text gamemodeValue;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text multiplierText;
    [SerializeField] private TMP_Text multiplierCountdownText;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GameManager.Instance.CurrentGamemode == GameManager.Gamemode.Timebased)
            gamemodeCondition.text = "Time left:";
        else
            gamemodeCondition.text = "Mistakes left:";
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.CurrentGamemode == GameManager.Gamemode.Timebased)
            gamemodeValue.text = ((int)GameManager.Instance.GameDuration - (int)GameManager.Instance.GameTime).ToString();
        else
            gamemodeValue.text = GameManager.Instance.ScoreSystem.LivesRemaining.ToString();
        scoreText.text = GameManager.Instance.ScoreSystem.Score.ToString();
        multiplierText.text = GameManager.Instance.ScoreSystem.ScoreMultiplier.ToString();
        int remainingDurationInSeconds = (int)GameManager.Instance.ScoreSystem.ScoreMultiplierRemainingDuration;
        multiplierCountdownText.text = remainingDurationInSeconds.ToString();


    }
}
