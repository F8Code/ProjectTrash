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

    // Pause state
    private bool _isPaused = false;

    // Internal logic
    private int _initialMistakesAllowed;

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
        _initialMistakesAllowed = ScoreSystem.LivesRemaining;
        _stateMachine.ChangeState(new GameTutorialState());
    }

    private void Update()
    {
        // Handle pause input
        HandlePauseInput();

        _stateMachine.Update();

        if (!_isPaused && CurrentState is GameTutorialState or GamePlayingState)
        {
            _activeGameTime += Time.deltaTime;
            ScoreSystem.CustomUpdate();
        }

        _roundedDeltaTime += (Time.unscaledDeltaTime - _roundedDeltaTime) * 0.01f;
    }

    private void HandlePauseInput()
    {
        if (CurrentState is not (GameTutorialState or GamePlayingState or GamePausedState))
            return;

        if (InputManager.Instance != null && InputManager.Instance.UIActions.Cancel.WasPressedThisFrame())
            ToggleGamePause();
    }

    public void ToggleGamePause()
    {
        // Only allow pausing during Tutorial or Playing states
        if (CurrentState is not (GameTutorialState or GamePlayingState) && !_isPaused)
            return;

        if (_isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    private void PauseGame()
    {
        _isPaused = true;

        if (UIManager.Instance != null)
            UIManager.Instance.ShowPanel(GameConstants.Canvas.PauseMenu);

        // Show cursor for UI interaction
        if (CursorManager.Instance != null)
            CursorManager.Instance.ShowCursorForUI();

        // Change to paused state
        SetState(new GamePausedState());
    }

    public void ResumeGame()
    {
        _isPaused = false;

        // Hide pause menu via UIManager
        if (UIManager.Instance != null)
            UIManager.Instance.HidePanel(GameConstants.Canvas.PauseMenu);

        if (CursorManager.Instance != null)
            CursorManager.Instance.HideCursorForUI();

        // Return to previous state (Tutorial or Playing)
        if (PreviousState != null)
            _stateMachine.ChangeState(PreviousState);
    }

    public void EndTutorialStage()
    {
        while (ScoreSystem.LivesRemaining < _initialMistakesAllowed)
            ScoreSystem.RestoreLife();

        SetState(new GamePlayingState());
    }

    public void EndGame()
    {
        Debug.Log($"<color=red>GAME OVER - Final Score: {ScoreSystem.Score}, Game Time: {_activeGameTime:F2}s</color>");

        // Ensure game is not paused
        _isPaused = false;
        Time.timeScale = 1f;

        // Change state first
        SetState(new GameOverState());

        // Show Game Over panel with final score
        if (_leaderboardUI != null)
            _leaderboardUI.ShowGameOver(ScoreSystem.Score);
    }

    private void SetState(IGameState state)
    {
        if (CurrentState is not GamePausedState)
            _stateMachine.ChangeState(state);
        else if (state is not GamePausedState)
            _stateMachine.SetPreviousState(state);
    }
}