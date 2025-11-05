using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    //Scoring logic
    public ScoreSystem ScoreSystem;

    //State machine
    StateMachine _stateMachine;
    public IGameState CurrentState => _stateMachine.CurrentState;
    public IGameState PreviousState => _stateMachine.PreviousState;

    //Game Statistics
    float _activeGameTime = 0f;
    float _roundedDeltaTime = 0.01666f; //60FPS
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
            ScoreSystem.CustomUpdate();
        }

        _roundedDeltaTime += (Time.unscaledDeltaTime - _roundedDeltaTime) * 0.01f;
    }

    public void ToggleGamePause() => SetState(new GamePausedState());

    public void EndTutorialStage()  => SetState(new GamePlayingState());

    public void EndGame()
    {
        SetState(new GameOverState());
        Debug.Log("GAME OVER");
    } 

    void SetState(IGameState state)
    {
        if (CurrentState is not GamePausedState)
            _stateMachine.ChangeState(state);
        else if (state is not GamePausedState)
            _stateMachine.SetPreviousState(state);
    }
}