#if UNITY_EDITOR

using ProjectTrash.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Custom editor for VideoPlayerEventTrigger component
/// Provides a cleaner inspector with helpful buttons and preview info
/// </summary>
[CustomEditor(typeof(VideoPlayerEventTrigger))]
public class VideoPlayerEventTriggerEditor : Editor
{
    private SerializedProperty _videoPlayerProp;
    private SerializedProperty _onVideoFinishedProp;
    private SerializedProperty _onVideoStartedProp;
    private SerializedProperty _onVideoPausedProp;
    private SerializedProperty _stopOnFinishProp;
    private SerializedProperty _disableOnFinishProp;
    private SerializedProperty _showDebugLogsProp;

    private void OnEnable()
    {
        _videoPlayerProp = serializedObject.FindProperty("_videoPlayer");
        _onVideoFinishedProp = serializedObject.FindProperty("_onVideoFinished");
        _onVideoStartedProp = serializedObject.FindProperty("_onVideoStarted");
        _onVideoPausedProp = serializedObject.FindProperty("_onVideoPaused");
        _stopOnFinishProp = serializedObject.FindProperty("_stopOnFinish");
        _disableOnFinishProp = serializedObject.FindProperty("_disableOnFinish");
        _showDebugLogsProp = serializedObject.FindProperty("_showDebugLogs");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        VideoPlayerEventTrigger trigger = (VideoPlayerEventTrigger)target;

        // Header
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Video Player Event Trigger", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Triggers Unity Events when a video finishes, starts, or pauses.\n\n" +
            "Perfect for scene transitions, UI updates, or any action after video playback.", MessageType.Info);

        EditorGUILayout.Space();

        // Video Player Info
        DrawVideoPlayerInfo();

        EditorGUILayout.Space();

        // References
        EditorGUILayout.LabelField("References", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_videoPlayerProp, new GUIContent("Video Player", "VideoPlayer component (auto-assigned if empty)"));

        if (_videoPlayerProp.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("VideoPlayer will be auto-assigned from this GameObject on Awake.", MessageType.Warning);
        }

        EditorGUILayout.Space();

        // Settings
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_stopOnFinishProp, new GUIContent("Stop On Finish", "Stop the video when it finishes (useful for non-looping videos)"));
        EditorGUILayout.PropertyField(_disableOnFinishProp, new GUIContent("Disable On Finish", "Disable this GameObject when video finishes"));

        EditorGUILayout.Space();

        // Events
        EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_onVideoFinishedProp, new GUIContent("On Video Finished"));
        EditorGUILayout.PropertyField(_onVideoStartedProp, new GUIContent("On Video Started"));
        EditorGUILayout.PropertyField(_onVideoPausedProp, new GUIContent("On Video Paused"));

        EditorGUILayout.Space();

        // Debug
        EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_showDebugLogsProp, new GUIContent("Show Debug Logs", "Log video events to console"));

        EditorGUILayout.Space();

        // Action Buttons (only in Play Mode)
        if (Application.isPlaying)
        {
            DrawPlayModeControls(trigger);
        }
        else
        {
            DrawEditModeInfo();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawVideoPlayerInfo()
    {
        VideoPlayer videoPlayer = null;
        
        if (_videoPlayerProp.objectReferenceValue != null)
        {
            videoPlayer = _videoPlayerProp.objectReferenceValue as VideoPlayer;
        }
        else
        {
            VideoPlayerEventTrigger trigger = (VideoPlayerEventTrigger)target;
            videoPlayer = trigger.GetComponent<VideoPlayer>();
        }

        if (videoPlayer != null)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Video Player Info", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Source:", GUILayout.Width(100));
            EditorGUILayout.LabelField(videoPlayer.source.ToString());
            EditorGUILayout.EndHorizontal();

            if (videoPlayer.source == VideoSource.VideoClip && videoPlayer.clip != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Clip:", GUILayout.Width(100));
                EditorGUILayout.LabelField(videoPlayer.clip.name);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Length:", GUILayout.Width(100));
                EditorGUILayout.LabelField($"{videoPlayer.clip.length:F2}s");
                EditorGUILayout.EndHorizontal();
            }
            else if (videoPlayer.source == VideoSource.Url)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("URL:", GUILayout.Width(100));
                EditorGUILayout.LabelField(string.IsNullOrEmpty(videoPlayer.url) ? "Not set" : videoPlayer.url);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Is Looping:", GUILayout.Width(100));
            EditorGUILayout.LabelField(videoPlayer.isLooping ? "Yes" : "No");
            EditorGUILayout.EndHorizontal();

            if (Application.isPlaying)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Is Playing:", GUILayout.Width(100));
                EditorGUILayout.LabelField(videoPlayer.isPlaying ? "Yes" : "No", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();

                if (videoPlayer.length > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Progress:", GUILayout.Width(100));
                    float progress = (float)(videoPlayer.time / videoPlayer.length) * 100f;
                    EditorGUILayout.LabelField($"{progress:F1}%");
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("No VideoPlayer component found on this GameObject.", MessageType.Warning);
        }
    }

    private void DrawPlayModeControls(VideoPlayerEventTrigger trigger)
    {
        EditorGUILayout.LabelField("Playback Controls (Play Mode)", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
        if (GUILayout.Button("? Play", GUILayout.Height(30)))
        {
            trigger.PlayVideo();
        }

        GUI.backgroundColor = new Color(1f, 1f, 0.5f);
        if (GUILayout.Button("? Pause", GUILayout.Height(30)))
        {
            trigger.PauseVideo();
        }

        GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
        if (GUILayout.Button("? Stop", GUILayout.Height(30)))
        {
            trigger.StopVideo();
        }

        GUI.backgroundColor = new Color(0.7f, 0.7f, 1f);
        if (GUILayout.Button("? Reset", GUILayout.Height(30)))
        {
            trigger.ResetVideo();
        }

        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();

        // Progress bar
        EditorGUILayout.Space();
        float progress = trigger.GetVideoProgress();
        EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), progress, $"{progress * 100:F1}%");
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox($"Currently {(trigger.IsPlaying() ? "Playing" : "Not Playing")}", 
            trigger.IsPlaying() ? MessageType.Info : MessageType.None);

        // Auto-refresh in play mode
        if (trigger.IsPlaying())
        {
            Repaint();
        }
    }

    private void DrawEditModeInfo()
    {
        EditorGUILayout.LabelField("Playback Controls", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Enter Play Mode to test video playback controls.", MessageType.Info);
    }
}

#endif
