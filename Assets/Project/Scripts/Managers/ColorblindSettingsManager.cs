using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

/// <summary>
/// Manages colorblind accessibility settings across all scenes
/// Persists settings using PlayerPrefs and applies to Global Volume
/// </summary>
public class ColorblindSettingsManager : MonoBehaviour
{
    public static ColorblindSettingsManager Instance { get; private set; }

    [Header("Global Volume Settings")]
    [Tooltip("The Global Volume component in the scene")]
    [SerializeField] private Volume globalVolume;

    [Header("Colorblind Filter Profiles")]
    [Tooltip("List of available colorblind filters")]
    [SerializeField] private List<ColorblindFilter> colorblindFilters = new();

    // PlayerPrefs key for saving settings
    private const string COLORBLIND_MODE_KEY = "ColorblindMode";

    // Current colorblind mode index
    private int currentModeIndex = 0;

    [System.Serializable]
    public class ColorblindFilter
    {
        [Tooltip("Display name of the filter (e.g., 'Default', 'Protanopia', etc.)")]
        public string filterName;

        [Tooltip("The Volume Profile asset for this filter")]
        public VolumeProfile profile;

        public ColorblindFilter(string name, VolumeProfile volumeProfile)
        {
            filterName = name;
            profile = volumeProfile;
        }
    }

    // Public property to get current mode name
    public string CurrentModeName => GetCurrentFilterName();

    // Public property to get current mode index
    public int CurrentModeIndex => currentModeIndex;

    private void Awake()
    {
        // Singleton pattern with DontDestroyOnLoad
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load saved settings
        LoadSettings();
    }

    private void OnEnable() => UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

    private void OnDisable() => UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;

    private void Start() => ApplyColorblindModeByIndex(currentModeIndex, false);

    private void OnValidate()
    {
        // Ensure no duplicate filter names
        if (colorblindFilters != null && colorblindFilters.Count > 1)
        {
            HashSet<string> uniqueNames = new();
            foreach (var filter in colorblindFilters)
            {
                if (!string.IsNullOrEmpty(filter.filterName) && !uniqueNames.Add(filter.filterName))
                    Debug.LogWarning($"[ColorblindSettingsManager] Duplicate filter name detected: '{filter.filterName}'");
            }
        }
    }

    /// <summary>
    /// Load settings from PlayerPrefs
    /// </summary>
    private void LoadSettings()
    {
        if (PlayerPrefs.HasKey(COLORBLIND_MODE_KEY))
        {
            currentModeIndex = PlayerPrefs.GetInt(COLORBLIND_MODE_KEY);

            // Validate the saved index
            if (currentModeIndex < 0 || currentModeIndex >= colorblindFilters.Count)
            {
                Debug.LogWarning($"[ColorblindSettingsManager] Saved index {currentModeIndex} is out of range. Resetting to 0.");
                currentModeIndex = 0;
            }

            Debug.Log($"<color=cyan>[ColorblindSettingsManager] Loaded saved mode: {GetCurrentFilterName()}</color>");
        }
        else
        {
            currentModeIndex = 0;
            Debug.Log("<color=cyan>[ColorblindSettingsManager] No saved settings found, using first filter</color>");
        }
    }

    /// <summary>
    /// Save settings to PlayerPrefs
    /// </summary>
    private void SaveSettings()
    {
        PlayerPrefs.SetInt(COLORBLIND_MODE_KEY, currentModeIndex);
        PlayerPrefs.Save();
        Debug.Log($"<color=green>[ColorblindSettingsManager] Settings saved: {GetCurrentFilterName()}</color>");
    }

    /// <summary>
    /// Apply colorblind mode by index (useful for dropdowns)
    /// </summary>
    public void ApplyColorblindModeByIndex(int index, bool save = true)
    {
        if (colorblindFilters == null || colorblindFilters.Count == 0)
        {
            Debug.LogError("[ColorblindSettingsManager] No colorblind filters configured!");
            return;
        }

        if (index < 0 || index >= colorblindFilters.Count)
        {
            Debug.LogWarning($"[ColorblindSettingsManager] Invalid colorblind mode index: {index}");
            return;
        }

        currentModeIndex = index;

        // Find Global Volume if not assigned or scene changed
        if (globalVolume == null)
        {
            globalVolume = FindFirstObjectByType<Volume>();

            if (globalVolume == null)
            {
                Debug.LogWarning("[ColorblindSettingsManager] Cannot apply colorblind mode - No Global Volume found!");
                if (save) SaveSettings(); // Still save the preference for next scene
                return;
            }
        }

        // Get the filter at the specified index
        ColorblindFilter filter = colorblindFilters[index];

        if (filter.profile != null)
        {
            globalVolume.profile = filter.profile;
            Debug.Log($"<color=green>[ColorblindSettingsManager] Applied '{filter.filterName}' mode</color>");
        }

        // Save settings if requested
        if (save)
            SaveSettings();
    }

    /// <summary>
    /// Get all colorblind filter names as strings (for dropdown options)
    /// </summary>
    public string[] GetColorblindFilterNames()
    {
        if (colorblindFilters == null || colorblindFilters.Count == 0)
        {
            Debug.LogWarning("[ColorblindSettingsManager] No colorblind filters configured!");
            return new string[] { "None" };
        }

        string[] names = new string[colorblindFilters.Count];
        for (int i = 0; i < colorblindFilters.Count; i++)
        {
            names[i] = colorblindFilters[i].filterName;
        }
        return names;
    }

    /// <summary>
    /// Get the current filter name
    /// </summary>
    private string GetCurrentFilterName()
    {
        if (colorblindFilters == null || colorblindFilters.Count == 0 ||
            currentModeIndex < 0 || currentModeIndex >= colorblindFilters.Count)
        {
            return "Unknown";
        }
        return colorblindFilters[currentModeIndex].filterName;
    }

    /// <summary>
    /// Refresh the Global Volume reference (call when scene changes)
    /// </summary>
    public void RefreshGlobalVolume()
    {
        globalVolume = FindFirstObjectByType<Volume>();

        if (globalVolume != null)
        {
            ApplyColorblindModeByIndex(currentModeIndex, false);
            Debug.Log("<color=cyan>[ColorblindSettingsManager] Global Volume refreshed for new scene</color>");
        }
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode) => RefreshGlobalVolume();
}