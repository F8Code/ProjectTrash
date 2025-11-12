using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI controller for game settings panel
/// Connects UI elements to GameSettingsManager
/// </summary>
public class GameSettingsUI : MonoBehaviour
{
    [Header("Audio Sliders")]
    [SerializeField] private Slider _masterVolumeSlider;

    [SerializeField] private Slider _musicVolumeSlider;
    [SerializeField] private Slider _sfxVolumeSlider;

    [Header("Audio Volume Displays (Optional)")]
    [SerializeField] private TMP_Text _masterVolumeText;

    [SerializeField] private TMP_Text _musicVolumeText;
    [SerializeField] private TMP_Text _sfxVolumeText;

    [Header("Graphics Toggles")]
    [SerializeField] private Toggle _showFPSToggle;

    [SerializeField] private Toggle _vSyncToggle;

    [Header("Fullscreen Dropdown/Cycle")]
    [SerializeField] private TMP_Dropdown _fullscreenDropdown;

    [SerializeField] private TMP_Text _fullscreenModeText;

    private void OnEnable()
    {
        // Hook up slider events
        if (_masterVolumeSlider != null)
            _masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        if (_musicVolumeSlider != null)
            _musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        if (_sfxVolumeSlider != null)
            _sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        // Hook up toggle events
        if (_showFPSToggle != null)
            _showFPSToggle.onValueChanged.AddListener(OnShowFPSToggled);
        if (_vSyncToggle != null)
            _vSyncToggle.onValueChanged.AddListener(OnVSyncToggled);

        // Hook up dropdown event
        if (_fullscreenDropdown != null)
            _fullscreenDropdown.onValueChanged.AddListener(OnFullscreenModeChanged);
    }

    private void Start()
    {
        // Load current values
        LoadCurrentSettings();
    }

    private void OnDisable()
    {
        // Remove listeners
        if (_masterVolumeSlider != null)
            _masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        if (_musicVolumeSlider != null)
            _musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        if (_sfxVolumeSlider != null)
            _sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        if (_showFPSToggle != null)
            _showFPSToggle.onValueChanged.RemoveListener(OnShowFPSToggled);
        if (_vSyncToggle != null)
            _vSyncToggle.onValueChanged.RemoveListener(OnVSyncToggled);
        if (_fullscreenDropdown != null)
            _fullscreenDropdown.onValueChanged.RemoveListener(OnFullscreenModeChanged);
    }

    private void LoadCurrentSettings()
    {
        if (GameSettingsManager.Instance == null)
        {
            Debug.LogWarning("GameSettingsUI: GameSettingsManager instance not found!");
            return;
        }

        // Load audio settings
        if (_masterVolumeSlider != null)
        {
            _masterVolumeSlider.SetValueWithoutNotify(GameSettingsManager.Instance.GetMasterVolume());
            UpdateVolumeText(_masterVolumeText, _masterVolumeSlider.value);
        }
        if (_musicVolumeSlider != null)
        {
            _musicVolumeSlider.SetValueWithoutNotify(GameSettingsManager.Instance.GetMusicVolume());
            UpdateVolumeText(_musicVolumeText, _musicVolumeSlider.value);
        }
        if (_sfxVolumeSlider != null)
        {
            _sfxVolumeSlider.SetValueWithoutNotify(GameSettingsManager.Instance.GetSFXVolume());
            UpdateVolumeText(_sfxVolumeText, _sfxVolumeSlider.value);
        }

        // Load graphics settings
        if (_showFPSToggle != null)
            _showFPSToggle.SetIsOnWithoutNotify(GameSettingsManager.Instance.GetShowFPS());

        if (_vSyncToggle != null)
            _vSyncToggle.SetIsOnWithoutNotify(GameSettingsManager.Instance.GetVSync() > 0);

        // Load fullscreen mode
        int fullscreenMode = GameSettingsManager.Instance.GetFullscreenMode();
        if (_fullscreenDropdown != null)
            _fullscreenDropdown.SetValueWithoutNotify(fullscreenMode);

        UpdateFullscreenModeText();
    }

    #region Audio Callbacks

    private void OnMasterVolumeChanged(float value)
    {
        if (GameSettingsManager.Instance != null)
        {
            GameSettingsManager.Instance.SetMasterVolume(value);
            UpdateVolumeText(_masterVolumeText, value);
            GameSettingsManager.Instance.SaveSettings();
        }
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (GameSettingsManager.Instance != null)
        {
            GameSettingsManager.Instance.SetMusicVolume(value);
            UpdateVolumeText(_musicVolumeText, value);
            GameSettingsManager.Instance.SaveSettings();
        }
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (GameSettingsManager.Instance != null)
        {
            GameSettingsManager.Instance.SetSFXVolume(value);
            UpdateVolumeText(_sfxVolumeText, value);
            GameSettingsManager.Instance.SaveSettings();
        }
    }

    private void UpdateVolumeText(TMP_Text text, float value)
    {
        if (text != null)
        {
            text.text = $"{Mathf.RoundToInt(value * 100)}%";
        }
    }

    #endregion Audio Callbacks

    #region Graphics Callbacks

    private void OnShowFPSToggled(bool isOn)
    {
        if (GameSettingsManager.Instance != null)
        {
            GameSettingsManager.Instance.SetShowFPS(isOn);
            GameSettingsManager.Instance.SaveSettings();
        }
    }

    private void OnVSyncToggled(bool isOn)
    {
        if (GameSettingsManager.Instance != null)
        {
            GameSettingsManager.Instance.SetVSync(isOn ? 1 : 0);
            GameSettingsManager.Instance.SaveSettings();
        }
    }

    private void OnFullscreenModeChanged(int modeIndex)
    {
        if (GameSettingsManager.Instance != null)
        {
            GameSettingsManager.Instance.SetFullscreenMode(modeIndex);
            UpdateFullscreenModeText();
            GameSettingsManager.Instance.SaveSettings();
        }
    }

    private void UpdateFullscreenModeText()
    {
        if (_fullscreenModeText != null && GameSettingsManager.Instance != null)
        {
            _fullscreenModeText.text = GameSettingsManager.Instance.GetFullscreenModeString();
        }
    }

    #endregion Graphics Callbacks

    #region Public Methods

    /// <summary>
    /// Saves all current settings
    /// </summary>
    public void SaveSettings()
    {
        if (GameSettingsManager.Instance != null)
        {
            GameSettingsManager.Instance.SaveSettings();
        }
    }

    /// <summary>
    /// Resets all settings to default values
    /// </summary>
    public void ResetToDefaults()
    {
        if (GameSettingsManager.Instance != null)
        {
            GameSettingsManager.Instance.ResetToDefaults();
            LoadCurrentSettings();
        }
    }

    /// <summary>
    /// Refreshes UI to match current settings
    /// </summary>
    public void RefreshUI()
    {
        LoadCurrentSettings();
    }

    #endregion Public Methods
}