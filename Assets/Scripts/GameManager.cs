using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    internal UnityEvent _onPause;
    internal UnityEvent _onResume;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        _onPause = new UnityEvent();
        _onResume = new UnityEvent();
    }
}