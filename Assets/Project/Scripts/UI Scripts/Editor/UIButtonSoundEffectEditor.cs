using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Custom editor for UIButtonSoundEffect to provide a better UI experience
/// </summary>
[CustomEditor(typeof(UIButtonSoundEffect))]
public class UIButtonSoundEffectEditor : Editor
{
    private SerializedProperty playHoverSoundProp;
    private SerializedProperty hoverSoundClipProp;
    private SerializedProperty hoverSoundVolumeProp;
    private SerializedProperty playClickSoundProp;
    private SerializedProperty clickSoundClipProp;
    private SerializedProperty clickSoundVolumeProp;
    private SerializedProperty soundPriorityProp;

    private void OnEnable()
    {
        playHoverSoundProp = serializedObject.FindProperty("playHoverSound");
        hoverSoundClipProp = serializedObject.FindProperty("hoverSoundClip");
        hoverSoundVolumeProp = serializedObject.FindProperty("hoverSoundVolume");
        playClickSoundProp = serializedObject.FindProperty("playClickSound");
        clickSoundClipProp = serializedObject.FindProperty("clickSoundClip");
        clickSoundVolumeProp = serializedObject.FindProperty("clickSoundVolume");
        soundPriorityProp = serializedObject.FindProperty("soundPriority");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        UIButtonSoundEffect buttonSound = (UIButtonSoundEffect)target;
        Button button = buttonSound.GetComponent<Button>();

        // Header
        EditorGUILayout.Space(5);
        GUIStyle headerStyle = new(EditorStyles.boldLabel)
        {
            fontSize = 14,
            alignment = TextAnchor.MiddleCenter
        };
        EditorGUILayout.LabelField("UI Button Sound Effect", headerStyle);
        EditorGUILayout.Space(5);

        // Button Status Info
        if (button != null)
        {
            string status = button.interactable ? "Interactable" : "Not Interactable";
            EditorGUILayout.HelpBox($"Button Status: {status}",
            button.interactable ? MessageType.Info : MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox("No Button component found! This component requires a Button.", MessageType.Error);
        }

        EditorGUILayout.Space(5);

        // Hover Sound Section
        DrawSoundSection(
          "Hover Sound",
            playHoverSoundProp,
            hoverSoundClipProp,
   hoverSoundVolumeProp,
            "Sound played when mouse hovers over the button"
   );

        EditorGUILayout.Space(5);

        // Click Sound Section
        DrawSoundSection(
              "Click Sound",
               playClickSoundProp,
                  clickSoundClipProp,
             clickSoundVolumeProp,
         "Sound played when button is clicked"
              );

        EditorGUILayout.Space(5);

        // Settings
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(soundPriorityProp, new GUIContent("Sound Priority", "Priority level for audio playback"));
        EditorGUI.indentLevel--;

        EditorGUILayout.Space(5);

        // Test Buttons (only in Play Mode)
        if (Application.isPlaying && button != null && button.interactable)
        {
            EditorGUILayout.LabelField("Test Sounds (Play Mode)", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Test Hover Sound"))
            {
                buttonSound.PlayHoverSound();
            }

            if (GUILayout.Button("Test Click Sound"))
            {
                buttonSound.PlayClickSound();
            }

            EditorGUILayout.EndHorizontal();
        }

        // Quick Setup Button
        if (!Application.isPlaying)
        {
            EditorGUILayout.Space(5);
            if (GUILayout.Button("Add to All Buttons in Scene", GUILayout.Height(30)))
            {
                AddToAllButtonsInScene();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSoundSection(string title, SerializedProperty playProp, SerializedProperty clipProp,
        SerializedProperty volumeProp, string tooltip)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();

        // Toggle button with color
        GUI.backgroundColor = playProp.boolValue ? new Color(0.5f, 1f, 0.5f) : Color.white;
        if (GUILayout.Button(playProp.boolValue ? "Enabled" : "Disabled", GUILayout.Width(70), GUILayout.Height(18)))
        {
            playProp.boolValue = !playProp.boolValue;
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        if (playProp.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(clipProp, new GUIContent("Audio Clip", tooltip));

            if (clipProp.objectReferenceValue != null)
            {
                EditorGUILayout.PropertyField(volumeProp, new GUIContent("Volume"));
            }
            else
            {
                EditorGUILayout.HelpBox("No audio clip assigned!", MessageType.Warning);
            }
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
    }

    private void AddToAllButtonsInScene()
    {
        if (EditorUtility.DisplayDialog("Add to All Buttons",
        "This will add UIButtonSoundEffect to all Button components in the scene that don't have it yet. Continue?",
            "Yes", "Cancel"))
        {
            Button[] allButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int addedCount = 0;

            foreach (Button button in allButtons)
            {
                if (button.GetComponent<UIButtonSoundEffect>() == null)
                {
                    Undo.AddComponent<UIButtonSoundEffect>(button.gameObject);
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                EditorUtility.DisplayDialog("Success", $"Added UIButtonSoundEffect to {addedCount} button(s).", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Complete", "All buttons already have UIButtonSoundEffect.", "OK");
            }
        }
    }
}