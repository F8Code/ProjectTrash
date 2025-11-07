using UnityEngine;

public class StateMachine
{
    IGameState _currentState, _previousState;
    public IGameState CurrentState => _currentState;
    public IGameState PreviousState => _previousState;

    public void SetPreviousState(IGameState state) => _previousState = state;

    public void ChangeState(IGameState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();

        if (newState is not GamePausedState)
            _previousState = _currentState;
    }

    public void Update()
    {
        _currentState?.Update();
    }
}