using UnityEngine;

public class InputManager : MonoBehaviour
{
    private InputSystem_Actions _input;

    internal InputSystem_Actions.PlayerActions PlayerActions { get; private set; }
    internal InputSystem_Actions.UIActions UIActions { get; private set; }

    public static InputManager Instance { get; private set; }

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

        _input = new();

        // Initialize actions
        InitializeActions();
    }

    private void OnEnable()
    {
        _input?.Enable();

        GameManager.Instance._onPause.AddListener(OnPause);
        GameManager.Instance._onResume.AddListener(OnResume);
    }

    private void OnDisable()
    {
        _input?.Disable();

        GameManager.Instance._onPause.RemoveListener(OnPause);
        GameManager.Instance._onResume.RemoveListener(OnResume);
    }

    private void InitializeActions()
    {
        PlayerActions = _input.Player;
        UIActions = _input.UI;
    }

    private void OnPause() => PlayerActions.Disable();

    private void OnResume() => PlayerActions.Enable();
}