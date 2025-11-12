using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Manages game settings including audio, graphics, and display options.
/// Settings persist across scenes using PlayerPrefs.
/// </summary>
public class GameSettingsManager : MonoBehaviour
{
    public static GameSettingsManager Instance { get; private set; }

    [Header("Audio Settings")]
    [SerializeField] private AudioMixer _audioMixer;

    [Tooltip("Name of the Master volume parameter in the Audio Mixer (usually 'Master' or 'MasterVolume')")]
    [SerializeField] private string _masterVolumeParameter = "Master";

    [Tooltip("Name of the Music volume parameter in the Audio Mixer")]
    [SerializeField] private string _musicVolumeParameter = "Music";

    [Tooltip("Name of the SFX volume parameter in the Audio Mixer")]
    [SerializeField] private string _sfxVolumeParameter = "SFX";

    [Header("FPS Display")]
    [SerializeField] private bool _showFPSOnStart = false;

    // PlayerPrefs keys
    private const string MASTER_VOLUME_KEY = "MasterVolume";

    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string VSYNC_KEY = "VSync";
    private const string FULLSCREEN_MODE_KEY = "FullscreenMode";
    private const string SHOW_FPS_KEY = "ShowFPS";

    // Default values
    private const float DEFAULT_VOLUME = 0.75f;

    private const int DEFAULT_VSYNC = 1;
    private const int DEFAULT_FULLSCREEN_MODE = 0; // FullScreenMode.ExclusiveFullScreen

    // Current settings
    private bool _showFPS;

    private FPSCounter _fpsCounter;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize FPS counter
        InitializeFPSCounter();

        // Load and apply settings
        LoadSettings();
    }

    private void InitializeFPSCounter()
    {
        // Create FPS counter component if it doesn't exist
        _fpsCounter = GetComponent<FPSCounter>();
        if (_fpsCounter == null)
        {
            _fpsCounter = gameObject.AddComponent<FPSCounter>();
        }
    }

    /// <summary>
    /// Loads all settings from PlayerPrefs and applies them
    /// </summary>
    public void LoadSettings()
    {
        // Load audio settings
        float masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, DEFAULT_VOLUME);
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, DEFAULT_VOLUME);
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, DEFAULT_VOLUME);

        SetMasterVolume(masterVolume);
        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);

        // Load V-Sync setting
        int vsync = PlayerPrefs.GetInt(VSYNC_KEY, DEFAULT_VSYNC);
        SetVSync(vsync);

        // Load fullscreen mode
        int fullscreenMode = PlayerPrefs.GetInt(FULLSCREEN_MODE_KEY, DEFAULT_FULLSCREEN_MODE);
        SetFullscreenMode(fullscreenMode);

        // Load FPS display setting
        int showFPS = PlayerPrefs.GetInt(SHOW_FPS_KEY, _showFPSOnStart ? 1 : 0);
        SetShowFPS(showFPS == 1);

        Debug.Log("GameSettingsManager: Settings loaded and applied");
    }

    /// <summary>
    /// Saves all current settings to PlayerPrefs
    /// </summary>
    public void SaveSettings()
    {
        PlayerPrefs.Save();
        Debug.Log("GameSettingsManager: Settings saved");
    }

    #region Audio Settings

    /// <summary>
    /// Sets the master volume (0.0 to 1.0)
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        SetMixerVolume(_masterVolumeParameter, volume);
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, volume);
    }

    /// <summary>
    /// Sets the music volume (0.0 to 1.0)
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        SetMixerVolume(_musicVolumeParameter, volume);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, volume);
    }

    /// <summary>
    /// Sets the SFX volume (0.0 to 1.0)
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        SetMixerVolume(_sfxVolumeParameter, volume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);
    }

    /// <summary>
    /// Gets the current master volume (0.0 to 1.0)
    /// </summary>
    public float GetMasterVolume()
    {
        return PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, DEFAULT_VOLUME);
    }

    /// <summary>
    /// Gets the current music volume (0.0 to 1.0)
    /// </summary>
    public float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, DEFAULT_VOLUME);
    }

    /// <summary>
    /// Gets the current SFX volume (0.0 to 1.0)
    /// </summary>
    public float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat(SFX_VOLUME_KEY, DEFAULT_VOLUME);
    }

    /// <summary>
    /// Internal method to set mixer volume with proper dB conversion
    /// </summary>
    private void SetMixerVolume(string parameterName, float normalizedVolume)
    {
        if (_audioMixer == null)
        {
            Debug.LogWarning("GameSettingsManager: AudioMixer is not assigned!");
            return;
        }

        // Convert normalized volume (0-1) to decibels (-80 to 0)
        // Using logarithmic scale for natural volume perception
        float volumeDB = normalizedVolume > 0 ? 20f * Mathf.Log10(normalizedVolume) : -80f;
        volumeDB = Mathf.Clamp(volumeDB, -80f, 0f);

        _audioMixer.SetFloat(parameterName, volumeDB);
    }

    #endregion Audio Settings

    #region Graphics Settings

    /// <summary>
    /// Sets V-Sync count (0 = off, 1 = every frame, 2 = every other frame)
    /// </summary>
    public void SetVSync(int vSyncCount)
    {
        vSyncCount = Mathf.Clamp(vSyncCount, 0, 2);
        QualitySettings.vSyncCount = vSyncCount;
        PlayerPrefs.SetInt(VSYNC_KEY, vSyncCount);
    }

    /// <summary>
    /// Gets the current V-Sync setting
    /// </summary>
    public int GetVSync()
    {
        return PlayerPrefs.GetInt(VSYNC_KEY, DEFAULT_VSYNC);
    }

    /// <summary>
    /// Sets the fullscreen mode
    /// 0 = ExclusiveFullScreen (Fullscreen)
    /// 1 = FullScreenWindow (Borderless Fullscreen)
    /// 2 = MaximizedWindow (Maximized Windowed)
    /// 3 = Windowed
    /// </summary>
    public void SetFullscreenMode(int mode)
    {
        mode = Mathf.Clamp(mode, 0, 3);
        FullScreenMode fullScreenMode = (FullScreenMode)mode;
        Screen.fullScreenMode = fullScreenMode;
        PlayerPrefs.SetInt(FULLSCREEN_MODE_KEY, mode);

        Debug.Log($"GameSettingsManager: Fullscreen mode set to {fullScreenMode}");
    }

    /// <summary>
    /// Gets the current fullscreen mode as an integer
    /// </summary>
    public int GetFullscreenMode()
    {
        return PlayerPrefs.GetInt(FULLSCREEN_MODE_KEY, DEFAULT_FULLSCREEN_MODE);
    }

    /// <summary>
    /// Gets the fullscreen mode as a readable string
    /// </summary>
    public string GetFullscreenModeString()
    {
        int mode = GetFullscreenMode();
        switch (mode)
        {
            case 0:
                return "Fullscreen";

            case 1:
                return "Borderless Fullscreen";

            case 2:
                return "Maximized Windowed";

            case 3:
                return "Windowed";

            default:
                return "Fullscreen";
        }
    }

    #endregion Graphics Settings

    #region FPS Display

    /// <summary>
    /// Sets whether to show the FPS counter
    /// </summary>
    public void SetShowFPS(bool show)
    {
        _showFPS = show;
        PlayerPrefs.SetInt(SHOW_FPS_KEY, show ? 1 : 0);

        if (_fpsCounter != null)
        {
            _fpsCounter.gameObject.SetActive(show);
        }
    }

    /// <summary>
    /// Toggles the FPS counter display
    /// </summary>
    public void ToggleFPS()
    {
        SetShowFPS(!_showFPS);
    }

    /// <summary>
    /// Gets whether the FPS counter is shown
    /// </summary>
    public bool GetShowFPS()
    {
        return PlayerPrefs.GetInt(SHOW_FPS_KEY, _showFPSOnStart ? 1 : 0) == 1;
    }

    #endregion FPS Display

    #region Utility Methods

    /// <summary>
    /// Resets all settings to default values
    /// </summary>
    public void ResetToDefaults()
    {
        SetMasterVolume(DEFAULT_VOLUME);
        SetMusicVolume(DEFAULT_VOLUME);
        SetSFXVolume(DEFAULT_VOLUME);
        SetVSync(DEFAULT_VSYNC);
        SetFullscreenMode(DEFAULT_FULLSCREEN_MODE);
        SetShowFPS(false);
        SaveSettings();

        Debug.Log("GameSettingsManager: Settings reset to defaults");
    }

    /// <summary>
    /// Clears all saved settings from PlayerPrefs
    /// </summary>
    public void ClearSettings()
    {
        PlayerPrefs.DeleteKey(MASTER_VOLUME_KEY);
        PlayerPrefs.DeleteKey(MUSIC_VOLUME_KEY);
        PlayerPrefs.DeleteKey(SFX_VOLUME_KEY);
        PlayerPrefs.DeleteKey(VSYNC_KEY);
        PlayerPrefs.DeleteKey(FULLSCREEN_MODE_KEY);
        PlayerPrefs.DeleteKey(SHOW_FPS_KEY);
        PlayerPrefs.Save();

        Debug.Log("GameSettingsManager: All settings cleared");
    }

    #endregion Utility Methods
}