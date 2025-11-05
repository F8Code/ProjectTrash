using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField, Range(0, 10)] uint _scoreMultiplierLevels = 5;
    [SerializeField, Range(0.1f, 2f)] float _scoreMultiplierPerLevel = 1f;

    //State machine
    StateMachine _stateMachine;

    //Game Statistics
    float _activeGameTime = 0f;
    float _roundedDeltaTime = 0.01666f; //60FPS

    //Score and score multiplier related
    uint _score = 0, consecutiveScoreCount;
    float _lastScoreTime = 0f;

    public IGameState CurrentState => _stateMachine.CurrentState;
    public IGameState PreviousState => _stateMachine.PreviousState;
    public float GameTime => _activeGameTime;
    public int FPS => (int)(1.0f / _roundedDeltaTime);

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _stateMachine = new();
    }

    void Start()
    {
        _stateMachine.ChangeState(new GameTutorialState());
    }

    void Update()
    {
        _stateMachine.Update();

        if (CurrentState is GameTutorialState or GamePlayingState)
        {
            _activeGameTime += Time.deltaTime;
        }

        _roundedDeltaTime += (Time.unscaledDeltaTime - _roundedDeltaTime) * 0.01f;
    }

    public void ToggleGamePause()
    {
        _stateMachine.ChangeState(CurrentState is not GamePausedState ? new GamePausedState() : PreviousState);
    }

    public void EndTutorialStage()
    {
        if (CurrentState is GamePausedState)
            _stateMachine.SetPreviousState(new GamePlayingState());
        else
            _stateMachine.ChangeState(new GamePlayingState());
    }

    public void ModifyScore(int scoreAward)
    {
        if (scoreAward > 0)
            _score += (uint)scoreAward;
            
        Debug.Log("Current score: " + _score);
    }
}