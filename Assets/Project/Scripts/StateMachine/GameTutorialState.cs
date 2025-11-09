using UnityEngine;

public class GameTutorialState : IGameState
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
