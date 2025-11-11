using TMPro;
using UnityEngine;

/// <summary>
/// UI component for colorblind settings dropdown
/// Uses TextMeshPro dropdown to control colorblind modes
/// </summary>
[RequireComponent(typeof(TMP_Dropdown))]
public class ColorblindSettingsUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Dropdown colorblindDropdown;

    private void Start() => InitializeDropdown();

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (colorblindDropdown != null)
            colorblindDropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
    }

    /// <summary>
    /// Initialize the dropdown with colorblind mode options
    /// </summary>
    private void InitializeDropdown()
    {
        // Clear existing options
        colorblindDropdown.ClearOptions();

        // Get colorblind filter names from the manager
        string[] filterNames = ColorblindSettingsManager.Instance.GetColorblindFilterNames();

        // Check if we have valid filters
        if (filterNames == null || filterNames.Length == 0 ||
            (filterNames.Length == 1 && filterNames[0] == "None"))
        {
            Debug.LogWarning("[ColorblindSettingsUI] No colorblind filters configured in ColorblindSettingsManager!");
            colorblindDropdown.AddOptions(new System.Collections.Generic.List<string> { "No Filters Available" });
            colorblindDropdown.interactable = false;
            return;
        }

        // Add options to dropdown
        colorblindDropdown.AddOptions(new System.Collections.Generic.List<string>(filterNames));

        // Set current value from settings manager
        int currentModeIndex = ColorblindSettingsManager.Instance.CurrentModeIndex;
        colorblindDropdown.value = currentModeIndex;
        colorblindDropdown.RefreshShownValue();

        // Subscribe to value changed event
        colorblindDropdown.onValueChanged.AddListener(OnDropdownValueChanged);

        Debug.Log($"<color=green>[ColorblindSettingsUI] Dropdown initialized with {filterNames.Length} colorblind filters</color>");
    }

    /// <summary>
    /// Called when dropdown value changes
    /// </summary>
    private void OnDropdownValueChanged(int index)
    {
        if (ColorblindSettingsManager.Instance != null)
        {
            ColorblindSettingsManager.Instance.ApplyColorblindModeByIndex(index);
            Debug.Log($"<color=cyan>[ColorblindSettingsUI] User selected filter at index: {index}</color>");
        }
    }
}