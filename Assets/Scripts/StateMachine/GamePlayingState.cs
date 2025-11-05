using UnityEngine;

public class GamePlayingState : IGameState
{
    public void Enter()
    {

    }

    public void Update()
    {
        PlayerManager.Instance.CustomUpdate();
        TrashManager.Instance.CustomUpdate();
    }

    public void Exit()
    {

    }
}
