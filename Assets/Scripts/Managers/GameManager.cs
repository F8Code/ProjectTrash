using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    //Scoring logic
    public ScoreSystem ScoreSystem;

    //State machine
    private StateMachine _stateMachine;

    public IGameState CurrentState => _stateMachine.CurrentState;
    public IGameState PreviousState => _stateMachine.PreviousState;

    //Game Statistics
    private float _activeGameTime = 0f;

    private float _roundedDeltaTime = 0.01666f; //60FPS
    public float GameTime => _activeGameTime;
    public int FPS => (int)(1.0f / _roundedDeltaTime);

    //Leaderboard UI
    [Header("UI References")]
    [SerializeField] private LeaderboardUI _leaderboardUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _stateMachine = new();
    }

    private void Start()
    {
        _stateMachine.ChangeState(new GameTutorialState());
    }

    private void Update()
    {
        _stateMachine.Update();

        // Only update game logic during Tutorial and Playing states, NOT during Game Over
        if (CurrentState is GameTutorialState or GamePlayingState)
        {
            _activeGameTime += Time.deltaTime;
            ScoreSystem.CustomUpdate();
        }

        _roundedDeltaTime += (Time.unscaledDeltaTime - _roundedDeltaTime) * 0.01f;
    }

    public void ToggleGamePause() => SetState(new GamePausedState());

    public void EndTutorialStage() => SetState(new GamePlayingState());

    public void EndGame()
    {
        Debug.Log($"<color=red>GAME OVER - Final Score: {ScoreSystem.Score}, Game Time: {_activeGameTime:F2}s</color>");

        // Change state first
        SetState(new GameOverState());

        // Show Game Over panel with final score
        if (_leaderboardUI != null)
        {
            Debug.Log($"<color=green>[GameManager] Showing game over screen with score: {ScoreSystem.Score}</color>");
            _leaderboardUI.ShowGameOver(ScoreSystem.Score);
        }
        else
        {
            Debug.LogError("<color=red>[GameManager] LeaderboardUI reference is missing! Please assign it in the Inspector.</color>");
        }
    }

    private void SetState(IGameState state)
    {
        if (CurrentState is not GamePausedState)
            _stateMachine.ChangeState(state);
        else if (state is not GamePausedState)
            _stateMachine.SetPreviousState(state);
    }
}