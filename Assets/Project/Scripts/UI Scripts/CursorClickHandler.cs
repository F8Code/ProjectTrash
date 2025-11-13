using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles cursor icon changes when user clicks and releases mouse button
/// Changes cursor sprite on click and reverts back on release
/// Uses Unity's new Input System via InputManager
/// </summary>
public class CursorClickHandler : MonoBehaviour
{
    [Header("Cursor Textures")]
    [Tooltip("Normal cursor texture when not clicking")]
    [SerializeField] private Texture2D _normalCursor;

    [Tooltip("Cursor texture when mouse button is pressed")]
    [SerializeField] private Texture2D _clickedCursor;

    [Header("Cursor Settings")]
    [Tooltip("Hot spot offset for normal cursor (default: center)")]
    [SerializeField] private Vector2 _normalHotspot = Vector2.zero;

    [Tooltip("Hot spot offset for clicked cursor (default: center)")]
    [SerializeField] private Vector2 _clickedHotspot = Vector2.zero;

    [Tooltip("Use center of texture as hotspot automatically")]
    [SerializeField] private bool _useAutoCenterHotspot = true;

    [Tooltip("Cursor mode (Auto, ForceSoftware, ForceHardware)")]
    [SerializeField] private CursorMode _cursorMode = CursorMode.Auto;

    [Header("Input Settings")]
    [Tooltip("Which mouse button to track (0=Left, 1=Right, 2=Middle)")]
    [SerializeField] private int _mouseButton = 0;

    [Tooltip("Enable cursor change on click")]
    [SerializeField] private bool _enableClickEffect = true;

    [Header("Debug")]
    [SerializeField] private bool _showDebugLogs = true;

    private bool _isClicking = false;
    private InputAction _currentClickAction;
    private bool _wasButtonPressed = false;

    public static CursorClickHandler Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (_showDebugLogs)
            Debug.Log("<color=cyan>[CursorClickHandler] Awake - Instance created</color>");
    }

    private void Start()
    {
        // Auto-calculate hotspot to center if enabled
        if (_useAutoCenterHotspot)
        {
            if (_normalCursor != null)
                _normalHotspot = new Vector2(_normalCursor.width / 2f, _normalCursor.height / 2f);

            if (_clickedCursor != null)
                _clickedHotspot = new Vector2(_clickedCursor.width / 2f, _clickedCursor.height / 2f);
        }

        // Set initial cursor
        SetNormalCursor();

        // Subscribe to input events
        SubscribeToInputEvents();

        if (_showDebugLogs)
            Debug.Log("<color=cyan>[CursorClickHandler] Initialized with mouse button: " + _mouseButton + "</color>");
    }

    private void Update()
    {
        if (!_enableClickEffect || _currentClickAction == null)
            return;

        // Check button state each frame
        bool isButtonPressed = _currentClickAction.ReadValue<float>() > 0.1f;

        // Detect press (transition from not pressed to pressed)
        if (isButtonPressed && !_wasButtonPressed)
        {
            OnButtonPressed();
        }
        // Detect release (transition from pressed to not pressed)
        else if (!isButtonPressed && _wasButtonPressed)
        {
            OnButtonReleased();
        }

        _wasButtonPressed = isButtonPressed;
    }

    private void OnDisable()
    {
        if (_showDebugLogs)
            Debug.Log("<color=yellow>[CursorClickHandler] OnDisable called</color>");

        UnsubscribeFromInputEvents();

        // Reset cursor when disabled
        if (_isClicking)
        {
            _isClicking = false;
            SetNormalCursor();
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from input events
        UnsubscribeFromInputEvents();

        // Reset to system cursor when destroyed
        ResetToSystemCursor();

        if (Instance == this)
            Instance = null;
    }

    private void OnApplicationQuit() => ResetToSystemCursor();

    private void SubscribeToInputEvents()
    {
        // Unsubscribe first to avoid double subscription
        UnsubscribeFromInputEvents();

        // Subscribe to the appropriate click action based on mouse button
        switch (_mouseButton)
        {
            case 0: // Left Click
                _currentClickAction = InputManager.Instance.UIActions.Click;
                if (_showDebugLogs)
                    Debug.Log("<color=cyan>[CursorClickHandler] Using UIActions.Click (Left button)</color>");
                break;

            case 1: // Right Click
                _currentClickAction = InputManager.Instance.UIActions.RightClick;
                if (_showDebugLogs)
                    Debug.Log("<color=cyan>[CursorClickHandler] Using UIActions.RightClick</color>");
                break;

            case 2: // Middle Click
                _currentClickAction = InputManager.Instance.UIActions.MiddleClick;
                if (_showDebugLogs)
                    Debug.Log("<color=cyan>[CursorClickHandler] Using UIActions.MiddleClick</color>");
                break;

            default:
                _currentClickAction = InputManager.Instance.UIActions.Click;
                if (_showDebugLogs)
                    Debug.Log("<color=cyan>[CursorClickHandler] Using UIActions.Click (default)</color>");
                break;
        }

        if (_currentClickAction != null)
        {
            // Check if the action itself is enabled
            if (_showDebugLogs)
                Debug.Log($"<color=cyan>[CursorClickHandler] Action '{_currentClickAction.name}' enabled: {_currentClickAction.enabled}</color>");

            if (_showDebugLogs)
                Debug.Log($"<color=green>[CursorClickHandler] ✓ Ready to track {_currentClickAction.name} via Update()</color>");
        }
    }

    private void UnsubscribeFromInputEvents()
    {
        // Reset button state tracking
        _wasButtonPressed = false;

        if (_showDebugLogs && _currentClickAction != null)
            Debug.Log("<color=yellow>[CursorClickHandler] Unsubscribed from input events</color>");
    }

    private void OnButtonPressed()
    {
        if (!_enableClickEffect || _isClicking)
            return;

        _isClicking = true;
        SetClickedCursor();

        if (_showDebugLogs)
            Debug.Log("<color=yellow>[CursorClickHandler] Mouse button pressed - cursor changed!</color>");
    }

    private void OnButtonReleased()
    {
        if (!_enableClickEffect || !_isClicking)
            return;

        _isClicking = false;
        SetNormalCursor();

        if (_showDebugLogs)
            Debug.Log("<color=green>[CursorClickHandler] Mouse button released - cursor reverted!</color>");
    }

    /// <summary>
    /// Set cursor to normal state
    /// </summary>
    public void SetNormalCursor()
    {
        if (_normalCursor != null && _normalCursor.isReadable)
        {
            Cursor.SetCursor(_normalCursor, _normalHotspot, _cursorMode);

            if (_showDebugLogs)
                Debug.Log("<color=cyan>[CursorClickHandler] Normal cursor set</color>");
        }
        else
        {
            // Reset to default system cursor
            Cursor.SetCursor(null, Vector2.zero, _cursorMode);

            if (_showDebugLogs)
            {
                if (_normalCursor != null && !_normalCursor.isReadable)
                    Debug.LogWarning("<color=orange>[CursorClickHandler] Normal cursor is not readable! Check texture import settings.</color>");
            }
        }
    }

    /// <summary>
    /// Set cursor to clicked state
    /// </summary>
    public void SetClickedCursor()
    {
        if (_clickedCursor != null && _clickedCursor.isReadable)
        {
            Cursor.SetCursor(_clickedCursor, _clickedHotspot, _cursorMode);

            if (_showDebugLogs)
                Debug.Log("<color=yellow>[CursorClickHandler] Clicked cursor set</color>");
        }
        else if (_showDebugLogs)
        {
            if (_clickedCursor != null && !_clickedCursor.isReadable)
                Debug.LogWarning("<color=orange>[CursorClickHandler] Clicked cursor is not readable! Check texture import settings.</color>");
        }
    }

    /// <summary>
    /// Reset cursor to system default
    /// </summary>
    public void ResetToSystemCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, _cursorMode);
        _isClicking = false;

        if (_showDebugLogs)
            Debug.Log("<color=magenta>[CursorClickHandler] Reset to system default cursor</color>");
    }

    /// <summary>
    /// Enable or disable the click effect
    /// </summary>
    public void SetClickEffectEnabled(bool enabled)
    {
        _enableClickEffect = enabled;

        if (!enabled && _isClicking)
        {
            _isClicking = false;
            SetNormalCursor();
        }

        if (_showDebugLogs)
            Debug.Log($"<color=cyan>[CursorClickHandler] Click effect {(enabled ? "enabled" : "disabled")}</color>");
    }

    /// <summary>
    /// Change the normal cursor texture at runtime
    /// </summary>
    public void SetNormalCursorTexture(Texture2D texture, Vector2? hotspot = null)
    {
        _normalCursor = texture;

        if (hotspot.HasValue)
            _normalHotspot = hotspot.Value;
        else if (_useAutoCenterHotspot && texture != null)
            _normalHotspot = new Vector2(texture.width / 2f, texture.height / 2f);

        if (!_isClicking)
            SetNormalCursor();

        if (_showDebugLogs)
            Debug.Log("<color=cyan>[CursorClickHandler] Normal cursor texture changed</color>");
    }

    /// <summary>
    /// Change the clicked cursor texture at runtime
    /// </summary>
    public void SetClickedCursorTexture(Texture2D texture, Vector2? hotspot = null)
    {
        _clickedCursor = texture;

        if (hotspot.HasValue)
            _clickedHotspot = hotspot.Value;
        else if (_useAutoCenterHotspot && texture != null)
            _clickedHotspot = new Vector2(texture.width / 2f, texture.height / 2f);

        if (_isClicking)
            SetClickedCursor();

        if (_showDebugLogs)
            Debug.Log("<color=yellow>[CursorClickHandler] Clicked cursor texture changed</color>");
    }

    /// <summary>
    /// Get the currently tracked mouse button
    /// </summary>
    public int GetMouseButton() => _mouseButton;

    /// <summary>
    /// Check if currently clicking
    /// </summary>
    public bool IsClicking() => _isClicking;
}