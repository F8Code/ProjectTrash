using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Editor window utility to batch configure UI button sounds
/// Access via: Tools > UI > Button Sound Manager
/// </summary>
public class UIButtonSoundManager : EditorWindow
{
    private AudioClip defaultHoverClip;
    private float defaultHoverVolume = 0.5f;
    private AudioClip defaultClickClip;
    private float defaultClickVolume = 1f;
    private int defaultPriorityIndex = 0; // 0=Low, 1=Medium, 2=High, 3=Critical

    private readonly string[] priorityOptions = new string[] { "Low", "Medium", "High", "Critical" };

    private bool playHoverSound = true;
    private bool playClickSound = true;

    private Vector2 scrollPosition;
    private readonly List<Button> foundButtons = new();
    private bool showFoundButtons = false;

    [MenuItem("Tools/UI/Button Sound Manager")]
    public static void ShowWindow()
    {
        UIButtonSoundManager window = GetWindow<UIButtonSoundManager>("Button Sound Manager");
        window.minSize = new Vector2(400, 500);
        window.Show();
    }

    private void OnGUI()
    {
        DrawHeader();
        EditorGUILayout.Space(10);
        DrawDefaultSettings();
        EditorGUILayout.Space(10);
        DrawActionButtons();
        EditorGUILayout.Space(10);
        DrawFoundButtons();
    }

    private void DrawHeader()
    {
        EditorGUILayout.Space(10);
        GUIStyle headerStyle = new(EditorStyles.boldLabel)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter
        };
        EditorGUILayout.LabelField("UI Button Sound Manager", headerStyle);
        EditorGUILayout.Space(5);

        EditorGUILayout.HelpBox(
    "Configure default sound settings and apply them to multiple buttons at once. " +
 "This tool helps you quickly set up audio feedback for all UI buttons in your scene.",
      MessageType.Info
        );
    }

    private void DrawDefaultSettings()
    {
        EditorGUILayout.LabelField("Default Sound Settings", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.Space(5);

        // Hover Sound Settings
        EditorGUILayout.LabelField("Hover Sound", EditorStyles.miniBoldLabel);
        playHoverSound = EditorGUILayout.Toggle("Enable Hover Sound", playHoverSound);
        if (playHoverSound)
        {
            EditorGUI.indentLevel++;
            defaultHoverClip = (AudioClip)EditorGUILayout.ObjectField("Hover Clip", defaultHoverClip, typeof(AudioClip), false);
            defaultHoverVolume = EditorGUILayout.Slider("Hover Volume", defaultHoverVolume, 0f, 1f);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(5);

        // Click Sound Settings
        EditorGUILayout.LabelField("Click Sound", EditorStyles.miniBoldLabel);
        playClickSound = EditorGUILayout.Toggle("Enable Click Sound", playClickSound);
        if (playClickSound)
        {
            EditorGUI.indentLevel++;
            defaultClickClip = (AudioClip)EditorGUILayout.ObjectField("Click Clip", defaultClickClip, typeof(AudioClip), false);
            defaultClickVolume = EditorGUILayout.Slider("Click Volume", defaultClickVolume, 0f, 1f);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(5);

        // Priority
        defaultPriorityIndex = EditorGUILayout.Popup("Sound Priority", defaultPriorityIndex, priorityOptions);

        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }

    private void DrawActionButtons()
    {
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

        if (GUILayout.Button("Find All Buttons in Scene", GUILayout.Height(30)))
        {
            FindAllButtons();
        }

        EditorGUI.BeginDisabledGroup(foundButtons.Count == 0);

        if (GUILayout.Button($"Add Sound Effect to Found Buttons ({foundButtons.Count})", GUILayout.Height(30)))
        {
            AddSoundEffectToButtons();
        }

        if (GUILayout.Button($"Update Existing Sound Effects ({foundButtons.Count})", GUILayout.Height(30)))
        {
            UpdateExistingSoundEffects();
        }

        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Remove All Sound Effects from Scene", GUILayout.Height(30)))
        {
            RemoveAllSoundEffects();
        }
    }

    private void DrawFoundButtons()
    {
        if (foundButtons.Count == 0) return;

        EditorGUILayout.Space(5);
        showFoundButtons = EditorGUILayout.Foldout(showFoundButtons, $"Found Buttons ({foundButtons.Count})", true, EditorStyles.foldoutHeader);

        if (showFoundButtons)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));

            foreach (Button button in foundButtons)
            {
                if (button == null) continue;

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                EditorGUILayout.ObjectField(button, typeof(Button), true);

                bool hasSoundEffect = button.GetComponent<UIButtonSoundEffect>() != null;
                GUI.backgroundColor = hasSoundEffect ? new Color(0.5f, 1f, 0.5f) : Color.yellow;
                GUILayout.Label(hasSoundEffect ? "? Has Sound" : "? No Sound", GUILayout.Width(100));
                GUI.backgroundColor = Color.white;

                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.activeGameObject = button.gameObject;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }
    }

    private void FindAllButtons()
    {
        foundButtons.Clear();
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foundButtons.AddRange(allButtons);
        showFoundButtons = true;
        Debug.Log($"<color=cyan>[Button Sound Manager] Found {foundButtons.Count} buttons in scene</color>");
    }

    private void AddSoundEffectToButtons()
    {
        if (foundButtons.Count == 0)
        {
            EditorUtility.DisplayDialog("No Buttons", "No buttons found. Click 'Find All Buttons' first.", "OK");
            return;
        }

        int addedCount = 0;
        foreach (Button button in foundButtons)
        {
            if (button == null) continue;

            if (!button.TryGetComponent<UIButtonSoundEffect>(out var soundEffect))
            {
                soundEffect = Undo.AddComponent<UIButtonSoundEffect>(button.gameObject);
                ConfigureSoundEffect(soundEffect);
                addedCount++;
            }
        }

        EditorUtility.DisplayDialog("Complete", $"Added UIButtonSoundEffect to {addedCount} button(s).", "OK");
        Debug.Log($"<color=green>[Button Sound Manager] Added sound effects to {addedCount} buttons</color>");
    }

    private void UpdateExistingSoundEffects()
    {
        if (foundButtons.Count == 0)
        {
            EditorUtility.DisplayDialog("No Buttons", "No buttons found. Click 'Find All Buttons' first.", "OK");
            return;
        }

        int updatedCount = 0;
        foreach (Button button in foundButtons)
        {
            if (button == null) continue;

            if (button.TryGetComponent<UIButtonSoundEffect>(out var soundEffect))
            {
                Undo.RecordObject(soundEffect, "Update Button Sound Effect");
                ConfigureSoundEffect(soundEffect);
                EditorUtility.SetDirty(soundEffect);
                updatedCount++;
            }
        }

        EditorUtility.DisplayDialog("Complete", $"Updated {updatedCount} existing sound effect(s).", "OK");
        Debug.Log($"<color=green>[Button Sound Manager] Updated {updatedCount} sound effects</color>");
    }

    private void RemoveAllSoundEffects()
    {
        if (EditorUtility.DisplayDialog("Remove All Sound Effects",
    "This will remove UIButtonSoundEffect from ALL buttons in the scene. This cannot be undone. Continue?",
     "Yes", "Cancel"))
        {
            Button[] allButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int removedCount = 0;

            foreach (Button button in allButtons)
            {
                if (button.TryGetComponent<UIButtonSoundEffect>(out var soundEffect))
                {
                    Undo.DestroyObjectImmediate(soundEffect);
                    removedCount++;
                }
            }

            EditorUtility.DisplayDialog("Complete", $"Removed UIButtonSoundEffect from {removedCount} button(s).", "OK");
            Debug.Log($"<color=yellow>[Button Sound Manager] Removed {removedCount} sound effects</color>");

            FindAllButtons(); // Refresh the list
        }
    }

    private void ConfigureSoundEffect(UIButtonSoundEffect soundEffect)
    {
        SerializedObject so = new(soundEffect);

        so.FindProperty("playHoverSound").boolValue = playHoverSound;
        so.FindProperty("hoverSoundClip").objectReferenceValue = defaultHoverClip;
        so.FindProperty("hoverSoundVolume").floatValue = defaultHoverVolume;

        so.FindProperty("playClickSound").boolValue = playClickSound;
        so.FindProperty("clickSoundClip").objectReferenceValue = defaultClickClip;
        so.FindProperty("clickSoundVolume").floatValue = defaultClickVolume;

        so.FindProperty("soundPriority").enumValueIndex = defaultPriorityIndex;

        so.ApplyModifiedProperties();
    }
}