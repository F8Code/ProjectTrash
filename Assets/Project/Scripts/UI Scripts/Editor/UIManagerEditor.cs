using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UIManager))]
public class UIManagerEditor : Editor
{
    private SerializedProperty panelsProperty;

    private bool showPanelsSection = true;
    private readonly Dictionary<int, bool> panelFoldouts = new();

    private void OnEnable() => panelsProperty = serializedObject.FindProperty("panels");

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Header
        EditorGUILayout.Space(10);
        GUIStyle headerStyle = new(EditorStyles.boldLabel)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter
        };
        EditorGUILayout.LabelField("UI Manager", headerStyle);
        EditorGUILayout.Space(5);

        DrawHelpBox();

        EditorGUILayout.Space(10);

        // Panels Section
        DrawPanelsSection();

        EditorGUILayout.Space(10);

        // Utility Buttons
        DrawUtilityButtons();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawHelpBox()
    {
        EditorGUILayout.HelpBox(
   "Drag and drop Canvas UI panels here. Each panel can have custom animations with directional control. " +
 "Use the '+' button to add new panels or 'Auto-Find Canvas' to detect them automatically.",
      MessageType.Info
        );
    }

    private void DrawPanelsSection()
    {
        // Section Header
        Rect headerRect = EditorGUILayout.GetControlRect();
        showPanelsSection = EditorGUI.Foldout(
          new Rect(headerRect.x, headerRect.y, headerRect.width - 60, headerRect.height),
    showPanelsSection,
       $"Panels ({panelsProperty.arraySize})",
       true,
         EditorStyles.foldoutHeader
           );

        // Add Panel Button
        if (GUI.Button(new Rect(headerRect.x + headerRect.width - 55, headerRect.y, 55, headerRect.height), "+ Add"))
            AddNewPanel();

        if (!showPanelsSection) return;

        EditorGUI.indentLevel++;

        for (int i = 0; i < panelsProperty.arraySize; i++)
            DrawPanelElement(i);

        if (panelsProperty.arraySize == 0)
            EditorGUILayout.HelpBox("No panels added yet. Click '+ Add' or 'Auto-Find Canvas' to add panels.", MessageType.Warning);

        EditorGUI.indentLevel--;
    }

    private void DrawPanelElement(int index)
    {
        SerializedProperty panelProp = panelsProperty.GetArrayElementAtIndex(index);
        SerializedProperty nameProp = panelProp.FindPropertyRelative("panelName");
        SerializedProperty objectProp = panelProp.FindPropertyRelative("panelObject");
        SerializedProperty startActiveProp = panelProp.FindPropertyRelative("startActive");
        SerializedProperty useAnimationProp = panelProp.FindPropertyRelative("useAnimation");
        SerializedProperty animationTypeProp = panelProp.FindPropertyRelative("animationType");
        SerializedProperty durationProp = panelProp.FindPropertyRelative("animationDuration");
        SerializedProperty entryDirectionProp = panelProp.FindPropertyRelative("entryDirection");
        SerializedProperty exitDirectionProp = panelProp.FindPropertyRelative("exitDirection");
        SerializedProperty entryCurveProp = panelProp.FindPropertyRelative("entryCurve");
        SerializedProperty exitCurveProp = panelProp.FindPropertyRelative("exitCurve");
        SerializedProperty fadeInCurveProp = panelProp.FindPropertyRelative("fadeInCurve");
        SerializedProperty fadeOutCurveProp = panelProp.FindPropertyRelative("fadeOutCurve");

        // Sound properties
        SerializedProperty playSoundProp = panelProp.FindPropertyRelative("playSound");
        SerializedProperty entrySoundClipProp = panelProp.FindPropertyRelative("entrySoundClip");
        SerializedProperty entrySoundVolumeProp = panelProp.FindPropertyRelative("entrySoundVolume");
        SerializedProperty exitSoundClipProp = panelProp.FindPropertyRelative("exitSoundClip");
        SerializedProperty exitSoundVolumeProp = panelProp.FindPropertyRelative("exitSoundVolume");
        SerializedProperty soundPriorityProp = panelProp.FindPropertyRelative("soundPriority");

        // Initialize foldout state
        if (!panelFoldouts.ContainsKey(index))
        {
            panelFoldouts[index] = false;
        }

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        // Panel Header
        EditorGUILayout.BeginHorizontal();

        string displayName = string.IsNullOrEmpty(nameProp.stringValue)
        ? $"Panel {index}"
            : nameProp.stringValue;

        panelFoldouts[index] = EditorGUILayout.Foldout(panelFoldouts[index], displayName, true);

        GUILayout.FlexibleSpace();

        // Start Active Button - Removed "?" prefix
        GUI.backgroundColor = startActiveProp.boolValue ? new Color(0.5f, 1f, 0.5f) : Color.white;
        if (GUILayout.Button(startActiveProp.boolValue ? "Active" : "Inactive",
           GUILayout.Width(70), GUILayout.Height(18)))
        {
            startActiveProp.boolValue = !startActiveProp.boolValue;
        }
        GUI.backgroundColor = Color.white;

        // Spacer for better separation
        GUILayout.Space(10);

        // Remove Button - Changed from ? to X
        GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
        if (GUILayout.Button("X", GUILayout.Width(25), GUILayout.Height(18)))
        {
            RemovePanel(index);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();

        // Panel Details (when expanded)
        if (panelFoldouts[index])
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(nameProp, new GUIContent("Panel Name"));
            EditorGUILayout.PropertyField(objectProp, new GUIContent("Panel GameObject"));

            // Info box for Canvas setup
            if (objectProp.objectReferenceValue != null)
            {
                GameObject panelObj = objectProp.objectReferenceValue as GameObject;

                if (panelObj.TryGetComponent<Canvas>(out var _))
                    EditorGUILayout.HelpBox("Canvas detected - will animate canvas enable/disable", MessageType.Info);
                else
                    EditorGUILayout.HelpBox("No Canvas component found - Add Canvas component to this GameObject", MessageType.Warning);
            }

            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(useAnimationProp, new GUIContent("Use Animation"));

            if (useAnimationProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(animationTypeProp, new GUIContent("Animation Type"));
                EditorGUILayout.PropertyField(durationProp, new GUIContent("Duration"));

                EditorGUILayout.Space(3);

                // Show settings based on animation type
                if (animationTypeProp.enumValueIndex == 0) // Slide
                {
                    EditorGUILayout.LabelField("Slide Animation Settings", EditorStyles.miniBoldLabel);

                    EditorGUILayout.Space(3);
                    EditorGUILayout.LabelField("Entry Animation", EditorStyles.miniBoldLabel);
                    EditorGUILayout.PropertyField(entryDirectionProp, new GUIContent("Entry Direction", "Direction from which the panel enters (to center)"));
                    EditorGUILayout.PropertyField(entryCurveProp, new GUIContent("Entry Curve"));

                    EditorGUILayout.Space(3);
                    EditorGUILayout.LabelField("Exit Animation", EditorStyles.miniBoldLabel);
                    EditorGUILayout.PropertyField(exitDirectionProp, new GUIContent("Exit Direction", "Direction to which the panel exits (from center)"));
                    EditorGUILayout.PropertyField(exitCurveProp, new GUIContent("Exit Curve"));
                }
                else if (animationTypeProp.enumValueIndex == 1) // Fade
                {
                    EditorGUILayout.LabelField("Fade Animation Settings", EditorStyles.miniBoldLabel);

                    // Check for CanvasGroup
                    if (objectProp.objectReferenceValue != null)
                    {
                        GameObject panelObj = objectProp.objectReferenceValue as GameObject;
                        if (panelObj.GetComponent<CanvasGroup>() == null)
                        {
                            EditorGUILayout.HelpBox("Fade animation requires a CanvasGroup component. It will be added automatically at runtime.", MessageType.Info);
                        }
                    }

                    EditorGUILayout.Space(3);
                    EditorGUILayout.PropertyField(fadeInCurveProp, new GUIContent("Fade In Curve", "Controls the fade in alpha animation"));

                    EditorGUILayout.Space(3);
                    EditorGUILayout.PropertyField(fadeOutCurveProp, new GUIContent("Fade Out Curve", "Controls the fade out alpha animation"));
                }

                EditorGUI.indentLevel--;
            }

            // Sound Settings Section
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(playSoundProp, new GUIContent("Play Sound"));

            if (playSoundProp.boolValue)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.LabelField("Sound Settings", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(soundPriorityProp, new GUIContent("Sound Priority"));

                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField("Entry Sound", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(entrySoundClipProp, new GUIContent("Entry Sound Clip", "Sound to play when panel appears"));
                if (entrySoundClipProp.objectReferenceValue != null)
                {
                    EditorGUILayout.PropertyField(entrySoundVolumeProp, new GUIContent("Entry Volume"));
                }

                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField("Exit Sound", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(exitSoundClipProp, new GUIContent("Exit Sound Clip", "Sound to play when panel disappears"));
                if (exitSoundClipProp.objectReferenceValue != null)
                {
                    EditorGUILayout.PropertyField(exitSoundVolumeProp, new GUIContent("Exit Volume"));
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }

    private void DrawUtilityButtons()
    {
        EditorGUILayout.LabelField("Utilities", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Auto-Find Canvas"))
            AutoFindCanvasPanels();

        if (GUILayout.Button("Clear All Panels"))
        {
            if (EditorUtility.DisplayDialog("Clear All Panels", "Are you sure you want to remove all panels?", "Yes", "Cancel"))
                panelsProperty.ClearArray();
        }

        EditorGUILayout.EndHorizontal();

        // Preview buttons (only in Play Mode)
        if (Application.isPlaying)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Animation Preview (Play Mode)", EditorStyles.boldLabel);

            UIManager manager = (UIManager)target;

            EditorGUILayout.BeginHorizontal();

            for (int i = 0; i < panelsProperty.arraySize; i++)
            {
                SerializedProperty panelProp = panelsProperty.GetArrayElementAtIndex(i);
                SerializedProperty nameProp = panelProp.FindPropertyRelative("panelName");
                string panelName = nameProp.stringValue;

                if (!string.IsNullOrEmpty(panelName))
                {
                    if (GUILayout.Button($"Show {panelName}"))
                        manager.ShowPanel(panelName);
                }

                // Create new row every 2 buttons
                if ((i + 1) % 2 == 0)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Hide All Panels"))
                manager.HideAllPanels();
        }
    }

    private void AddNewPanel()
    {
        panelsProperty.InsertArrayElementAtIndex(panelsProperty.arraySize);
        SerializedProperty newPanel = panelsProperty.GetArrayElementAtIndex(panelsProperty.arraySize - 1);

        newPanel.FindPropertyRelative("panelName").stringValue = $"Panel_{panelsProperty.arraySize}";
        newPanel.FindPropertyRelative("panelObject").objectReferenceValue = null;
        newPanel.FindPropertyRelative("startActive").boolValue = false;
        newPanel.FindPropertyRelative("useAnimation").boolValue = true;
        newPanel.FindPropertyRelative("animationType").enumValueIndex = 0; // Slide
        newPanel.FindPropertyRelative("animationDuration").floatValue = 0.5f;
        newPanel.FindPropertyRelative("entryDirection").enumValueIndex = 0; // Left
        newPanel.FindPropertyRelative("exitDirection").enumValueIndex = 1;  // Right
        newPanel.FindPropertyRelative("entryCurve").animationCurveValue = AnimationCurve.EaseInOut(0, 0, 1, 1);
        newPanel.FindPropertyRelative("exitCurve").animationCurveValue = AnimationCurve.EaseInOut(0, 0, 1, 1);
        newPanel.FindPropertyRelative("fadeInCurve").animationCurveValue = AnimationCurve.EaseInOut(0, 0, 1, 1);
        newPanel.FindPropertyRelative("fadeOutCurve").animationCurveValue = AnimationCurve.EaseInOut(0, 0, 1, 1);

        // Initialize sound settings
        newPanel.FindPropertyRelative("playSound").boolValue = false;
        newPanel.FindPropertyRelative("entrySoundClip").objectReferenceValue = null;
        newPanel.FindPropertyRelative("entrySoundVolume").floatValue = 1f;
        newPanel.FindPropertyRelative("exitSoundClip").objectReferenceValue = null;
        newPanel.FindPropertyRelative("exitSoundVolume").floatValue = 1f;
        newPanel.FindPropertyRelative("soundPriority").enumValueIndex = 1; // Medium

        panelFoldouts[panelsProperty.arraySize - 1] = true;
    }

    private void RemovePanel(int index)
    {
        panelsProperty.DeleteArrayElementAtIndex(index);
        panelFoldouts.Remove(index);
    }

    private void AutoFindCanvasPanels()
    {
        UIManager _ = (UIManager)target;

        // Find all Canvas components in the scene
        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        int addedCount = 0;
        foreach (Canvas canvas in allCanvases)
        {
            // Skip if this canvas is already added
            bool exists = false;
            for (int i = 0; i < panelsProperty.arraySize; i++)
            {
                SerializedProperty panelProp = panelsProperty.GetArrayElementAtIndex(i);
                if (panelProp.FindPropertyRelative("panelObject").objectReferenceValue == canvas.gameObject)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                panelsProperty.InsertArrayElementAtIndex(panelsProperty.arraySize);
                SerializedProperty newPanel = panelsProperty.GetArrayElementAtIndex(panelsProperty.arraySize - 1);

                newPanel.FindPropertyRelative("panelName").stringValue = canvas.gameObject.name;
                newPanel.FindPropertyRelative("panelObject").objectReferenceValue = canvas.gameObject;
                newPanel.FindPropertyRelative("startActive").boolValue = canvas.enabled;
                newPanel.FindPropertyRelative("useAnimation").boolValue = true;
                newPanel.FindPropertyRelative("animationType").enumValueIndex = 0; // Slide
                newPanel.FindPropertyRelative("animationDuration").floatValue = 0.5f;
                newPanel.FindPropertyRelative("entryDirection").enumValueIndex = 0; // Left
                newPanel.FindPropertyRelative("exitDirection").enumValueIndex = 1;  // Right
                newPanel.FindPropertyRelative("entryCurve").animationCurveValue = AnimationCurve.EaseInOut(0, 0, 1, 1);
                newPanel.FindPropertyRelative("exitCurve").animationCurveValue = AnimationCurve.EaseInOut(0, 0, 1, 1);
                newPanel.FindPropertyRelative("fadeInCurve").animationCurveValue = AnimationCurve.EaseInOut(0, 0, 1, 1);
                newPanel.FindPropertyRelative("fadeOutCurve").animationCurveValue = AnimationCurve.EaseInOut(0, 0, 1, 1);

                // Initialize sound settings
                newPanel.FindPropertyRelative("playSound").boolValue = false;
                newPanel.FindPropertyRelative("entrySoundClip").objectReferenceValue = null;
                newPanel.FindPropertyRelative("entrySoundVolume").floatValue = 1f;
                newPanel.FindPropertyRelative("exitSoundClip").objectReferenceValue = null;
                newPanel.FindPropertyRelative("exitSoundVolume").floatValue = 1f;
                newPanel.FindPropertyRelative("soundPriority").enumValueIndex = 1; // Medium

                addedCount++;
            }
        }

        if (addedCount > 0)
        {
            EditorUtility.DisplayDialog("Auto-Find Complete",
           $"Added {addedCount} Canvas panel(s).", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Auto-Find Complete",
               "No new Canvas panels found in the scene.", "OK");
        }
    }
}