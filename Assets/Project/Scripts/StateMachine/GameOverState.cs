using UnityEngine;

public class GameOverState : IGameState
{
    public void Enter()
    {
        Debug.Log("<color=orange>[GameOverState] Entered Game Over state</color>");
    }

    public void Update()
    {
    }

    public void Exit()
    {
        Debug.Log("<color=orange>[GameOverState] Exiting Game Over state</color>");
    }
}