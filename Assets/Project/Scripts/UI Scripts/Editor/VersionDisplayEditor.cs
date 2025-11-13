#if UNITY_EDITOR

using ProjectTrash.UI;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom editor for VersionDisplay component
/// Provides a cleaner inspector with helpful buttons and info
/// </summary>
[CustomEditor(typeof(VersionDisplay))]
public class VersionDisplayEditor : Editor
{
    private SerializedProperty _prefixProp;
    private SerializedProperty _suffixProp;
    private SerializedProperty _showBuildInfoProp;
    private SerializedProperty _customFormatProp;
    private SerializedProperty _versionTextProp;
    private SerializedProperty _showDebugLogsProp;

    private void OnEnable()
    {
        _prefixProp = serializedObject.FindProperty("_prefix");
        _suffixProp = serializedObject.FindProperty("_suffix");
        _showBuildInfoProp = serializedObject.FindProperty("_showBuildInfo");
        _customFormatProp = serializedObject.FindProperty("_customFormat");
        _versionTextProp = serializedObject.FindProperty("_versionText");
        _showDebugLogsProp = serializedObject.FindProperty("_showDebugLogs");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        VersionDisplay versionDisplay = (VersionDisplay)target;

        // Header
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Version Display Component", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Automatically displays the application version on a TextMeshProUGUI component.\n\n" +
            "Set the version in: Edit > Project Settings > Player > Version", MessageType.Info);

        EditorGUILayout.Space();

        // Current Version Info
        DrawCurrentVersionInfo();

        EditorGUILayout.Space();

        // Version Display Settings
        EditorGUILayout.LabelField("Version Display Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_prefixProp, new GUIContent("Prefix", "Text before version (e.g., 'v', 'Version: ')"));
        EditorGUILayout.PropertyField(_suffixProp, new GUIContent("Suffix", "Text after version (e.g., ' Beta', ' Alpha')"));
        EditorGUILayout.PropertyField(_showBuildInfoProp, new GUIContent("Show Build Info", "Include build GUID in display"));

        EditorGUILayout.Space();

        // Format Settings
        EditorGUILayout.LabelField("Format Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_customFormatProp, new GUIContent("Custom Format", "Use {0} for version. Example: 'Version {0}'"));

        if (!string.IsNullOrEmpty(_customFormatProp.stringValue))
        {
            EditorGUILayout.HelpBox("Custom format will override prefix/suffix settings.", MessageType.Warning);
        }

        EditorGUILayout.Space();

        // References
        EditorGUILayout.LabelField("References", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_versionTextProp, new GUIContent("Version Text", "TextMeshProUGUI component to display version"));

        EditorGUILayout.Space();

        // Debug
        EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_showDebugLogsProp, new GUIContent("Show Debug Logs", "Log version changes to console"));

        EditorGUILayout.Space();

        // Action Buttons
        DrawActionButtons(versionDisplay);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawCurrentVersionInfo()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Current Application Info", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Version:", GUILayout.Width(100));
        EditorGUILayout.LabelField(Application.version, EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Unity Version:", GUILayout.Width(100));
        EditorGUILayout.LabelField(Application.unityVersion);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Platform:", GUILayout.Width(100));
        EditorGUILayout.LabelField(Application.platform.ToString());
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void DrawActionButtons(VersionDisplay versionDisplay)
    {
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        // Preview Button
        GUI.backgroundColor = new Color(0.5f, 0.8f, 1f);
        if (GUILayout.Button("Preview Version Display", GUILayout.Height(30)))
        {
            versionDisplay.UpdateVersionDisplay();
            EditorUtility.SetDirty(versionDisplay);
        }
        GUI.backgroundColor = Color.white;

        // Open Project Settings Button
        GUI.backgroundColor = new Color(0.8f, 1f, 0.5f);
        if (GUILayout.Button("Edit Version in Project Settings", GUILayout.Height(30)))
        {
            SettingsService.OpenProjectSettings("Project/Player");
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Quick Format Presets
        EditorGUILayout.LabelField("Quick Format Presets", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("v1.0.0"))
        {
            _prefixProp.stringValue = "v";
            _suffixProp.stringValue = "";
            _customFormatProp.stringValue = "";
            serializedObject.ApplyModifiedProperties();
        }
        if (GUILayout.Button("Version 1.0.0"))
        {
            _prefixProp.stringValue = "Version ";
            _suffixProp.stringValue = "";
            _customFormatProp.stringValue = "";
            serializedObject.ApplyModifiedProperties();
        }
        if (GUILayout.Button("1.0.0 Beta"))
        {
            _prefixProp.stringValue = "";
            _suffixProp.stringValue = " Beta";
            _customFormatProp.stringValue = "";
            serializedObject.ApplyModifiedProperties();
        }
        EditorGUILayout.EndHorizontal();
    }
}

#endif