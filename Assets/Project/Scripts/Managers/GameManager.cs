using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Gamemode settings")]
    [Tooltip("If Mistakebased, the game ends once player makes as many mistakes as configured. Otherwise on Timebased, the game ends once time runs out or the multiplier is interrupted")]
    [SerializeField] Gamemode _currentGamemode = Gamemode.Mistakebased;
    [Tooltip("How many mistakes are allowed before the game ends")]
    [SerializeField, Range(0, 20)] uint _mistakesAllowed = 10;
    [Tooltip("How many seconds the game lasts before it ends")]
    [SerializeField, Range(0, 120)] uint _gameDuration = 60;

    //Scoring logic
    public ScoreSystem ScoreSystem;

    //State machine
    private StateMachine _stateMachine;

    public IGameState CurrentState => _stateMachine.CurrentState;
    public IGameState PreviousState => _stateMachine.PreviousState;

    //Game Statistics
    private float _activeGameTime = 0f;

    private float _roundedDeltaTime = 0.01666f; //60FPS
    public Gamemode CurrentGamemode => _currentGamemode;
    public float GameDuration => _gameDuration;
    public float GameTime => _activeGameTime;
    public uint MistakeLimit => _mistakesAllowed;
    public int FPS => (int)(1.0f / _roundedDeltaTime);

    //Leaderboard UI
    [Header("UI References")]
    [SerializeField] private LeaderboardUI _leaderboardUI;

    // Pause state
    private bool _isPaused = false;

    private bool _inPauseContext = false; // Track if we're in pause menu navigation

    // Internal logic
    //private int _initialMistakesAllowed;

    [Header("Music Settings")]
    [Tooltip("Intro Music AudioClip")]
    public AudioClip IntroMusicSound;

    [Tooltip("Music AudioClip")]
    public AudioClip MusicSound;

    [Tooltip("Game Over Music AudioClip")]
    public AudioClip GameOverMusicSound;

    [Tooltip("Music volume multiplier")]
    [SerializeField, Range(0f, 1f)] private float _musicVolume = 0.5f;

    private AudioSource _audioSource;

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
        switch (_currentGamemode)
        {
            case Gamemode.Tutorial:
                _stateMachine.ChangeState(new GameTutorialState());
                ScoreSystem.LivesRemaining = _mistakesAllowed;
                break;
            case Gamemode.Mistakebased:
                _stateMachine.ChangeState(new GamePlayingState());
                ScoreSystem.LivesRemaining = _mistakesAllowed;
                break;
            case Gamemode.Timebased:
                _stateMachine.ChangeState(new GamePlayingState());
                break;
            default:
                break;
        }

        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = IntroMusicSound;
        _audioSource.Play();
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

            if (!_audioSource.isPlaying)
            {
                _audioSource.clip = MusicSound;
                _audioSource.Play();
            }

            if (_currentGamemode == Gamemode.Timebased)
                if (_activeGameTime > _gameDuration && ScoreSystem.ScoreMultiplierRemainingDuration == 0)
                    EndGame();
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
        _inPauseContext = true;

        //Time.timeScale = 0;

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
        _inPauseContext = false;

        //Time.timeScale = 1;

        // Hide all UI panels before resuming
        if (UIManager.Instance != null)
            UIManager.Instance.HideAllPanels();

        if (CursorManager.Instance != null)
            CursorManager.Instance.HideCursorForUI();

        // Return to previous state (Tutorial or Playing)
        if (PreviousState != null)
            _stateMachine.ChangeState(PreviousState);
    }

    public void EndGame()
    {
        Debug.Log($"<color=red>GAME OVER - Final Score: {ScoreSystem.Score}, Game Time: {_activeGameTime:F2}s</color>");

        // Ensure game is not paused
        _isPaused = false;
        Time.timeScale = 1f;

        // Change state first
        SetState(new GameOverState());

        _audioSource.Stop();
        _audioSource.clip = GameOverMusicSound;
        _audioSource.Play();

        if (_leaderboardUI != null)
            _leaderboardUI.ShowGameOver(ScoreSystem.Score);

        //switch (_currentGamemode)
        //{
        //    case Gamemode.Tutorial:
        //        _leaderboardUI.ShowGameover();
        //        break;
        //    default:
        //        if (_leaderboardUI != null)
        //            _leaderboardUI.ShowGameOver(ScoreSystem.Score);
        //        break;
        //}
    }

    public void AddTimeBasedOnStreak(int extraSeconds)
    {
        _activeGameTime -= extraSeconds;
    }

    private void SetState(IGameState state)
    {
        if (CurrentState is not GamePausedState)
            _stateMachine.ChangeState(state);
        else if (state is not GamePausedState)
            _stateMachine.SetPreviousState(state);
    }

    public enum Gamemode
    {
        Tutorial,
        Mistakebased,
        Timebased
    }
}