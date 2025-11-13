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
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _input = new();

        InitializeActions();
    }

    private void OnEnable()
    {
        _input?.Enable();
    }

    private void OnDisable()
    {
        _input?.Disable();
    }

    private void OnDestroy()
    {
        if (_input != null)
        {
            _input.Disable();
            _input.Dispose();
            _input = null;
        }

        if (Instance == this)
            Instance = null;
    }

    private void InitializeActions()
    {
        PlayerActions = _input.Player;
        UIActions = _input.UI;
    }
}