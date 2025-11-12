using UnityEngine;
using TMPro;

/// <summary>
/// Simple button helper to cycle through fullscreen modes
/// Attach to a button and assign the text label to display current mode
/// </summary>
public class FullscreenModeButton : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text component that displays the current fullscreen mode")]
    [SerializeField] private TMP_Text _modeText;

    [Header("Display Options")]
    [Tooltip("Custom names for each fullscreen mode (if empty, uses defaults)")]
    [SerializeField] private string[] _customModeNames = new string[4];

    private readonly string[] _defaultModeNames = new string[]
    {
        "Fullscreen",
        "Borderless",
        "Maximized",
        "Windowed"
    };

    private void Start()
    {
        // Initialize with current mode
        UpdateModeText();
    }

    /// <summary>
    /// Call this from button's onClick event to cycle through modes
    /// </summary>
    public void CycleFullscreenMode()
    {
        if (GameSettingsManager.Instance == null)
        {
            Debug.LogWarning("FullscreenModeButton: GameSettingsManager not found!");
            return;
        }

        // Get current mode and cycle to next
        int currentMode = GameSettingsManager.Instance.GetFullscreenMode();
        int nextMode = (currentMode + 1) % 4;

        // Apply new mode
        GameSettingsManager.Instance.SetFullscreenMode(nextMode);

        // Update display
        UpdateModeText();
    }

    /// <summary>
    /// Updates the text to show current fullscreen mode
    /// </summary>
    private void UpdateModeText()
    {
        if (_modeText == null || GameSettingsManager.Instance == null)
            return;

        int currentMode = GameSettingsManager.Instance.GetFullscreenMode();
        
        // Use custom name if provided, otherwise use default
        if (_customModeNames.Length > currentMode && !string.IsNullOrEmpty(_customModeNames[currentMode]))
        {
            _modeText.text = _customModeNames[currentMode];
        }
        else if (currentMode >= 0 && currentMode < _defaultModeNames.Length)
        {
            _modeText.text = _defaultModeNames[currentMode];
        }
        else
        {
            _modeText.text = "Unknown";
        }
    }

    /// <summary>
    /// Call this to refresh the display (useful when returning to settings menu)
    /// </summary>
    public void RefreshDisplay()
    {
        UpdateModeText();
    }

    private void OnEnable()
    {
        // Update display when panel is enabled
        UpdateModeText();
    }
}
