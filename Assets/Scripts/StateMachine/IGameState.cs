using UnityEngine;

public interface IGameState
{
    void Enter();
    void Update();
    void Exit();
}

