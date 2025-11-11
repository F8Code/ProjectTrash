using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom editor for ColorblindSettingsManager
/// Provides easy testing buttons in the Inspector
/// </summary>
[CustomEditor(typeof(ColorblindSettingsManager))]
public class ColorblindSettingsManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw default inspector
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Quick Test Controls", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("These buttons allow you to test different colorblind modes in Play Mode.", MessageType.Info);

        ColorblindSettingsManager manager = (ColorblindSettingsManager)target;

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to test colorblind modes.", MessageType.Warning);

            // Show filter count in edit mode
            int filterCount = serializedObject.FindProperty("colorblindFilters").arraySize;
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Filters Configured:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"{filterCount} filter(s)", EditorStyles.largeLabel);
            EditorGUILayout.EndVertical();

            return;
        }

        // Get all filter names
        string[] filterNames = manager.GetColorblindFilterNames();

        if (filterNames == null || filterNames.Length == 0 ||
            (filterNames.Length == 1 && filterNames[0] == "None"))
        {
            EditorGUILayout.HelpBox("No colorblind filters configured! Add filters to the list in the inspector.", MessageType.Warning);
            return;
        }

        EditorGUILayout.Space(5);

        // Create buttons dynamically based on available filters
        int buttonsPerRow = 2;
        for (int i = 0; i < filterNames.Length; i++)
        {
            // Start new horizontal group for each row
            if (i % buttonsPerRow == 0)
            {
                EditorGUILayout.BeginHorizontal();
            }

            // Create button for this filter
            if (GUILayout.Button(filterNames[i], GUILayout.Height(30)))
            {
                manager.ApplyColorblindModeByIndex(i, false);
            }

            // End horizontal group at end of row or last button
            if (i % buttonsPerRow == buttonsPerRow - 1 || i == filterNames.Length - 1)
            {
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.Space(5);

        // Current mode display
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Current Mode:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"{manager.CurrentModeName} (Index: {manager.CurrentModeIndex})", EditorStyles.largeLabel);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(5);

        // Save current mode button
        if (GUILayout.Button("Save Current Mode to PlayerPrefs", GUILayout.Height(25)))
        {
            manager.ApplyColorblindModeByIndex(manager.CurrentModeIndex, true);
            EditorUtility.DisplayDialog("Settings Saved",
      $"Colorblind mode '{manager.CurrentModeName}' has been saved to PlayerPrefs.", "OK");
        }

        // Clear PlayerPrefs button
        if (GUILayout.Button("Clear Saved Settings", GUILayout.Height(25)))
        {
            if (EditorUtility.DisplayDialog("Clear Settings",
          "Are you sure you want to clear saved colorblind settings?", "Yes", "Cancel"))
            {
                PlayerPrefs.DeleteKey("ColorblindMode");
                PlayerPrefs.Save();
                EditorUtility.DisplayDialog("Settings Cleared",
                "Colorblind settings have been cleared from PlayerPrefs.", "OK");
            }
        }

        EditorGUILayout.Space(5);

        // Refresh volume button
        if (GUILayout.Button("Refresh Global Volume", GUILayout.Height(25)))
        {
            manager.RefreshGlobalVolume();
            EditorUtility.DisplayDialog("Volume Refreshed",
                "Global Volume reference has been refreshed.", "OK");
        }
    }
}