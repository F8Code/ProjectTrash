using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Centralized manager for cursor visibility and lock state
/// Automatically shows cursor when UI is visible, hides when gameplay is active
/// </summary>
public class CursorManager : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Default cursor lock mode during gameplay")]
    [SerializeField] private CursorLockMode _gameplayCursorLockMode = CursorLockMode.Locked;

    [Tooltip("Cursor lock mode when UI is active")]
    [SerializeField] private CursorLockMode _uiCursorLockMode = CursorLockMode.None;

    [Header("Debug")]
    [SerializeField] private bool _showDebugLogs = false;

    private int _uiVisibilityCount = 0;
    private bool _isCursorVisible = false;
    private CursorLockMode _currentLockMode = CursorLockMode.Locked;

    public static CursorManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log(this.name);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;

    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void Start() => SetGameplayCursor();

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            UpdateCursorState();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset UI count and cursor state when a new scene loads
        ResetUICount();

        if (_showDebugLogs)
            Debug.Log($"<color=magenta>[CursorManager] Scene loaded: {scene.name}. Cursor reset to gameplay state.</color>");
    }

    /// <summary>
    /// Call this when a UI panel becomes visible
    /// </summary>
    public void ShowCursorForUI()
    {
        _uiVisibilityCount++;

        if (_showDebugLogs)
            Debug.Log($"<color=cyan>[CursorManager] UI Shown. Count: {_uiVisibilityCount}</color>");

        UpdateCursorState();
    }

    /// <summary>
    /// Call this when a UI panel is hidden
    /// </summary>
    public void HideCursorForUI()
    {
        _uiVisibilityCount = Mathf.Max(0, _uiVisibilityCount - 1);

        if (_showDebugLogs)
            Debug.Log($"<color=cyan>[CursorManager] UI Hidden. Count: {_uiVisibilityCount}</color>");

        UpdateCursorState();
    }

    /// <summary>
    /// Force cursor to gameplay state (hidden and locked)
    /// </summary>
    public void SetGameplayCursor()
    {
        _uiVisibilityCount = 0;
        SetCursorState(false, _gameplayCursorLockMode);

        if (_showDebugLogs)
            Debug.Log("<color=green>[CursorManager] Gameplay cursor set (hidden, locked)</color>");
    }

    /// <summary>
    /// Force cursor to UI state (visible and unlocked)
    /// </summary>
    public void SetUICursor()
    {
        SetCursorState(true, _uiCursorLockMode);

        if (_showDebugLogs)
            Debug.Log("<color=green>[CursorManager] UI cursor set (visible, unlocked)</color>");
    }

    /// <summary>
    /// Manually set cursor visibility and lock mode
    /// </summary>
    public void SetCursorState(bool visible, CursorLockMode lockMode)
    {
        _isCursorVisible = visible;
        _currentLockMode = lockMode;

        Cursor.visible = _isCursorVisible;
        Cursor.lockState = _currentLockMode;

        if (_showDebugLogs)
            Debug.Log($"<color=yellow>[CursorManager] Cursor: {(visible ? "Visible" : "Hidden")}, Lock: {lockMode}</color>");
    }

    /// <summary>
    /// Reset the UI visibility counter (useful when changing scenes)
    /// </summary>
    public void ResetUICount()
    {
        _uiVisibilityCount = 0;
        UpdateCursorState();

        if (_showDebugLogs)
            Debug.Log("<color=orange>[CursorManager] UI count reset</color>");
    }

    private void UpdateCursorState()
    {
        if (_uiVisibilityCount > 0)
            SetCursorState(true, _uiCursorLockMode);
        else
            SetCursorState(false, _gameplayCursorLockMode);
    }
}