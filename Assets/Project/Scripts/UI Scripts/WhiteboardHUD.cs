using TMPro;
using UnityEngine;

public class WhiteboardHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text gamemodeCondition;
    [SerializeField] private TMP_Text gamemodeValue;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text multiplierText;
    [SerializeField] private TMP_Text multiplierCountdownText;

    //Counter for tutorial
    private uint _tutorialObjects = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GameManager.Instance.CurrentGamemode == GameManager.Gamemode.Timebased)
            gamemodeCondition.text = "Time left:";
        else if (GameManager.Instance.CurrentGamemode == GameManager.Gamemode.Tutorial)
        {
            gamemodeCondition.text = "Trash left:";
            TrashManager.Instance.TrashSpawned += Instance_TrashSpawned;
            _tutorialObjects = GameManager.Instance.MistakeLimit;
        }
        else
            gamemodeCondition.text = "Mistakes left:";
    }

    private void Instance_TrashSpawned(object sender, System.EventArgs e)
    {
        _tutorialObjects--;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.CurrentGamemode == GameManager.Gamemode.Timebased)
        {
            if ((int)GameManager.Instance.GameDuration - (int)GameManager.Instance.GameTime >= 0)
                gamemodeValue.text = ((int)GameManager.Instance.GameDuration - (int)GameManager.Instance.GameTime).ToString();
        }
        else if (GameManager.Instance.CurrentGamemode == GameManager.Gamemode.Tutorial)
            gamemodeValue.text = _tutorialObjects.ToString();
        else
            gamemodeValue.text = GameManager.Instance.ScoreSystem.LivesRemaining.ToString();

        scoreText.text = GameManager.Instance.ScoreSystem.Score.ToString();
        multiplierText.text = GameManager.Instance.ScoreSystem.ScoreMultiplier.ToString();
        int remainingDurationInSeconds = (int)GameManager.Instance.ScoreSystem.ScoreMultiplierRemainingDuration;
        multiplierCountdownText.text = remainingDurationInSeconds.ToString();
    }
}
