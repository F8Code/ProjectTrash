using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

namespace ProjectTrash.UI
{
    /// <summary>
    /// Triggers a UnityEvent when a VideoPlayer finishes playing
    /// Useful for transitioning scenes, showing UI elements, or any action after video completion
    /// </summary>
    [RequireComponent(typeof(VideoPlayer))]
    public class VideoPlayerEventTrigger : MonoBehaviour
    {
        [Header("Video Player Settings")]
        [Tooltip("Reference to the VideoPlayer component (auto-assigned if not set)")]
        [SerializeField] private VideoPlayer _videoPlayer;

        [Header("Events")]
        [Tooltip("Event triggered when the video finishes playing")]
        [SerializeField] private UnityEvent _onVideoFinished;

        [Tooltip("Event triggered when the video starts playing")]
        [SerializeField] private UnityEvent _onVideoStarted;

        [Tooltip("Event triggered when the video is paused")]
        [SerializeField] private UnityEvent _onVideoPaused;

        [Header("Settings")]
        [Tooltip("If true, stop the video when it finishes (useful for looping videos)")]
        [SerializeField] private bool _stopOnFinish = true;

        [Tooltip("If true, disable the GameObject when video finishes")]
        [SerializeField] private bool _disableOnFinish = false;

        [Header("Debug")]
        [SerializeField] private bool _showDebugLogs = true;

        private bool _hasFinished = false;

        private void Awake()
        {
            // Auto-assign VideoPlayer if not set
            if (_videoPlayer == null)
            {
                _videoPlayer = GetComponent<VideoPlayer>();
            }

            if (_videoPlayer == null)
            {
                Debug.LogError($"[VideoPlayerEventTrigger] No VideoPlayer component found on {gameObject.name}!");
                enabled = false;
                return;
            }
        }

        private void OnEnable()
        {
            if (_videoPlayer != null)
            {
                // Subscribe to VideoPlayer events
                _videoPlayer.loopPointReached += OnVideoFinished;
                _videoPlayer.started += OnVideoStarted;
                _videoPlayer.prepareCompleted += OnVideoPrepared;

                _hasFinished = false;
            }
        }

        private void OnDisable()
        {
            if (_videoPlayer != null)
            {
                // Unsubscribe from VideoPlayer events
                _videoPlayer.loopPointReached -= OnVideoFinished;
                _videoPlayer.started -= OnVideoStarted;
                _videoPlayer.prepareCompleted -= OnVideoPrepared;
            }
        }

        /// <summary>
        /// Called when the video finishes playing
        /// </summary>
        private void OnVideoFinished(VideoPlayer vp)
        {
            if (_hasFinished) return;

            _hasFinished = true;

            if (_showDebugLogs)
            {
                Debug.Log($"[VideoPlayerEventTrigger] Video finished on {gameObject.name}");
            }

            // Invoke the UnityEvent
            _onVideoFinished?.Invoke();

            if (_stopOnFinish && !_videoPlayer.isLooping)
            {
                _videoPlayer.Stop();
            }

            if (_disableOnFinish)
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Called when the video starts playing
        /// </summary>
        private void OnVideoStarted(VideoPlayer vp)
        {
            _hasFinished = false;

            if (_showDebugLogs)
            {
                Debug.Log($"[VideoPlayerEventTrigger] Video started on {gameObject.name}");
            }

            _onVideoStarted?.Invoke();
        }

        /// <summary>
        /// Called when the video is prepared and ready to play
        /// </summary>
        private void OnVideoPrepared(VideoPlayer vp)
        {
            if (_showDebugLogs)
            {
                Debug.Log($"[VideoPlayerEventTrigger] Video prepared on {gameObject.name}");
            }
        }

        /// <summary>
        /// Manually play the video
        /// </summary>
        [ContextMenu("Play Video")]
        public void PlayVideo()
        {
            if (_videoPlayer != null)
            {
                _hasFinished = false;
                _videoPlayer.Play();

                if (_showDebugLogs)
                {
                    Debug.Log($"[VideoPlayerEventTrigger] Playing video on {gameObject.name}");
                }
            }
        }

        /// <summary>
        /// Manually stop the video
        /// </summary>
        [ContextMenu("Stop Video")]
        public void StopVideo()
        {
            if (_videoPlayer != null)
            {
                _videoPlayer.Stop();

                if (_showDebugLogs)
                {
                    Debug.Log($"[VideoPlayerEventTrigger] Stopping video on {gameObject.name}");
                }
            }
        }

        /// <summary>
        /// Manually pause the video
        /// </summary>
        public void PauseVideo()
        {
            if (_videoPlayer != null)
            {
                _videoPlayer.Pause();
                _onVideoPaused?.Invoke();

                if (_showDebugLogs)
                {
                    Debug.Log($"[VideoPlayerEventTrigger] Pausing video on {gameObject.name}");
                }
            }
        }

        /// <summary>
        /// Reset the video to the beginning
        /// </summary>
        public void ResetVideo()
        {
            if (_videoPlayer != null)
            {
                _videoPlayer.time = 0;
                _hasFinished = false;

                if (_showDebugLogs)
                {
                    Debug.Log($"[VideoPlayerEventTrigger] Resetting video on {gameObject.name}");
                }
            }
        }

        /// <summary>
        /// Get the current video progress (0-1)
        /// </summary>
        public float GetVideoProgress()
        {
            if (_videoPlayer != null && _videoPlayer.length > 0)
            {
                return (float)(_videoPlayer.time / _videoPlayer.length);
            }
            return 0f;
        }

        /// <summary>
        /// Check if the video is currently playing
        /// </summary>
        public bool IsPlaying()
        {
            return _videoPlayer != null && _videoPlayer.isPlaying;
        }
    }
}
