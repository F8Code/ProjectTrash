#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom editor for CursorClickHandler component
/// Provides a cleaner inspector with preview and helpful buttons
/// </summary>
[CustomEditor(typeof(CursorClickHandler))]
public class CursorClickHandlerEditor : Editor
{
    private SerializedProperty _normalCursorProp;
    private SerializedProperty _clickedCursorProp;
    private SerializedProperty _normalHotspotProp;
    private SerializedProperty _clickedHotspotProp;
    private SerializedProperty _useAutoCenterHotspotProp;
    private SerializedProperty _cursorModeProp;
    private SerializedProperty _mouseButtonProp;
    private SerializedProperty _enableClickEffectProp;
    private SerializedProperty _showDebugLogsProp;

    private void OnEnable()
    {
        _normalCursorProp = serializedObject.FindProperty("_normalCursor");
        _clickedCursorProp = serializedObject.FindProperty("_clickedCursor");
        _normalHotspotProp = serializedObject.FindProperty("_normalHotspot");
        _clickedHotspotProp = serializedObject.FindProperty("_clickedHotspot");
        _useAutoCenterHotspotProp = serializedObject.FindProperty("_useAutoCenterHotspot");
        _cursorModeProp = serializedObject.FindProperty("_cursorMode");
        _mouseButtonProp = serializedObject.FindProperty("_mouseButton");
        _enableClickEffectProp = serializedObject.FindProperty("_enableClickEffect");
        _showDebugLogsProp = serializedObject.FindProperty("_showDebugLogs");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CursorClickHandler handler = (CursorClickHandler)target;

        // Header
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Cursor Click Handler", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Changes cursor icon when user clicks and releases the mouse button.\n\n" +
            "Perfect for adding visual feedback to mouse interactions.", MessageType.Info);

        EditorGUILayout.Space();

        // Cursor Textures Section
        EditorGUILayout.LabelField("Cursor Textures", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_normalCursorProp, new GUIContent("Normal Cursor", "Cursor shown when not clicking"));
        
        // Show preview of normal cursor
        if (_normalCursorProp.objectReferenceValue != null)
        {
            DrawCursorPreview((Texture2D)_normalCursorProp.objectReferenceValue, "Normal Cursor Preview");
        }

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(_clickedCursorProp, new GUIContent("Clicked Cursor", "Cursor shown when clicking"));
        
        // Show preview of clicked cursor
        if (_clickedCursorProp.objectReferenceValue != null)
        {
            DrawCursorPreview((Texture2D)_clickedCursorProp.objectReferenceValue, "Clicked Cursor Preview");
        }

        EditorGUILayout.Space();

        // Cursor Settings Section
        EditorGUILayout.LabelField("Cursor Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_useAutoCenterHotspotProp, new GUIContent("Auto Center Hotspot", "Automatically center hotspot on texture"));

        if (!_useAutoCenterHotspotProp.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_normalHotspotProp, new GUIContent("Normal Hotspot", "Hotspot offset for normal cursor"));
            EditorGUILayout.PropertyField(_clickedHotspotProp, new GUIContent("Clicked Hotspot", "Hotspot offset for clicked cursor"));
            EditorGUI.indentLevel--;
        }
        else
        {
            EditorGUILayout.HelpBox("Hotspots will be automatically centered on texture.", MessageType.Info);
        }

        EditorGUILayout.PropertyField(_cursorModeProp, new GUIContent("Cursor Mode", "Rendering mode for cursor"));

        EditorGUILayout.Space();

        // Input Settings Section
        EditorGUILayout.LabelField("Input Settings", EditorStyles.boldLabel);
        
        string[] mouseButtonNames = { "Left Click (0)", "Right Click (1)", "Middle Click (2)" };
        int currentButton = _mouseButtonProp.intValue;
        int newButton = EditorGUILayout.Popup(new GUIContent("Mouse Button", "Which mouse button to track"), 
            currentButton, mouseButtonNames);
        
        if (newButton != currentButton)
        {
            _mouseButtonProp.intValue = newButton;
        }

        EditorGUILayout.PropertyField(_enableClickEffectProp, new GUIContent("Enable Click Effect", "Enable/disable cursor change on click"));

        EditorGUILayout.Space();

        // Debug Section
        EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_showDebugLogsProp, new GUIContent("Show Debug Logs", "Log cursor state changes to console"));

        EditorGUILayout.Space();

        // Status Info (Play Mode Only)
        if (Application.isPlaying)
        {
            DrawPlayModeStatus(handler);
        }

        EditorGUILayout.Space();

        // Action Buttons
        DrawActionButtons(handler);

        // Warnings
        DrawWarnings();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawCursorPreview(Texture2D texture, string label)
    {
        if (texture == null) return;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(label, EditorStyles.miniLabel);
        
        Rect previewRect = GUILayoutUtility.GetRect(64, 64, GUILayout.Width(64));
        EditorGUI.DrawPreviewTexture(previewRect, texture);
        
        EditorGUILayout.LabelField($"Size: {texture.width}x{texture.height}", EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();
    }

    private void DrawPlayModeStatus(CursorClickHandler handler)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Status (Play Mode)", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Currently Clicking:", GUILayout.Width(120));
        EditorGUILayout.LabelField(handler.IsClicking() ? "Yes" : "No", 
            handler.IsClicking() ? EditorStyles.boldLabel : EditorStyles.label);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Tracked Button:", GUILayout.Width(120));
        string[] buttonNames = { "Left", "Right", "Middle" };
        EditorGUILayout.LabelField(buttonNames[handler.GetMouseButton()]);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        // Auto-refresh in play mode
        if (handler.IsClicking())
        {
            Repaint();
        }
    }

    private void DrawActionButtons(CursorClickHandler handler)
    {
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

        if (Application.isPlaying)
        {
            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
            if (GUILayout.Button("Set Normal Cursor", GUILayout.Height(30)))
            {
                handler.SetNormalCursor();
            }

            GUI.backgroundColor = new Color(1f, 1f, 0.5f);
            if (GUILayout.Button("Set Clicked Cursor", GUILayout.Height(30)))
            {
                handler.SetClickedCursor();
            }

            GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
            if (GUILayout.Button("Reset to System", GUILayout.Height(30)))
            {
                handler.ResetToSystemCursor();
            }

            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            
            GUI.backgroundColor = new Color(0.7f, 0.7f, 1f);
            if (GUILayout.Button("Toggle Click Effect", GUILayout.Height(25)))
            {
                handler.SetClickEffectEnabled(!_enableClickEffectProp.boolValue);
                serializedObject.Update();
            }

            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.HelpBox("Enter Play Mode to test cursor changes.", MessageType.Info);
        }
    }

    private void DrawWarnings()
    {
        // Check for missing textures
        if (_normalCursorProp.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("No normal cursor texture assigned. System default cursor will be used.", MessageType.Warning);
        }

        if (_clickedCursorProp.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("No clicked cursor texture assigned. Cursor won't change on click.", MessageType.Warning);
        }

        // Check texture import settings
        Texture2D normalTex = _normalCursorProp.objectReferenceValue as Texture2D;
        Texture2D clickedTex = _clickedCursorProp.objectReferenceValue as Texture2D;

        if (normalTex != null && !IsTextureReadable(normalTex))
        {
            EditorGUILayout.HelpBox("Normal cursor texture must have 'Read/Write Enabled' in import settings.", MessageType.Error);
        }

        if (clickedTex != null && !IsTextureReadable(clickedTex))
        {
            EditorGUILayout.HelpBox("Clicked cursor texture must have 'Read/Write Enabled' in import settings.", MessageType.Error);
        }
    }

    private bool IsTextureReadable(Texture2D texture)
    {
        if (texture == null) return false;

        string path = AssetDatabase.GetAssetPath(texture);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        
        return importer != null && importer.isReadable;
    }
}

#endif
