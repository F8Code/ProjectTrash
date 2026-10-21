using TMPro;
using UnityEngine;

public class WhiteboardHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text gamemodeCondition;
    [SerializeField] private TMP_Text gamemodeValue;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text multiplierText;
    [SerializeField] private TMP_Text multiplierCountdownText;

    [Header("VFX")]
    [SerializeField] GameObject _particleSpawn;
    [SerializeField] ParticleSystem _streakVFXPrefab;

    //Counter for tutorial
    private uint _tutorialObjects = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ApplyGamemodeLabel();
        if (GameManager.Instance.CurrentGamemode == GameManager.Gamemode.Tutorial)
        {
            TrashManager.Instance.TrashSpawned += Instance_TrashSpawned;
            _tutorialObjects = GameManager.Instance.MistakeLimit;
        }

        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.LanguageChanged += ApplyGamemodeLabel;

        GameManager.Instance.ScoreSystem.OnScoreFlash += ScoreSystem_OnScoreFlash;
    }

    private void ScoreSystem_OnScoreFlash(float streak)
    {
        ParticleSystem vfx = Instantiate(_streakVFXPrefab, _particleSpawn.transform.position, _particleSpawn.transform.rotation, _particleSpawn.transform);
        vfx.Play();
        Destroy(vfx.gameObject, vfx.main.duration);
    }

    void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.LanguageChanged -= ApplyGamemodeLabel;
    }

    void ApplyGamemodeLabel()
    {
        string source = GameManager.Instance.CurrentGamemode switch
        {
            GameManager.Gamemode.Timebased => "Time left:",
            GameManager.Gamemode.Tutorial => "Trash left:",
            _ => "Mistakes left:"
        };
        gamemodeCondition.text = LocalizationManager.Instance != null ? LocalizationManager.Instance.Get(source) : source;
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
        multiplierText.text = $"x{GameManager.Instance.ScoreSystem.ScoreMultiplier}";
        int remainingDurationInSeconds = (int)GameManager.Instance.ScoreSystem.ScoreMultiplierRemainingDuration;
        multiplierCountdownText.text = remainingDurationInSeconds.ToString();
    }
}
