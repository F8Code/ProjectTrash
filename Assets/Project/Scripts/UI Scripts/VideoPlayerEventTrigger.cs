using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ProjectTrash.UI
{
    [RequireComponent(typeof(VideoPlayer))]
    public class VideoPlayerEventTrigger : MonoBehaviour
    {
        const float StartTimeoutSeconds = 6f;
        const float StuckFrameSeconds = 2.5f;
        const float SkipInputDelay = 0.35f;

        [SerializeField] VideoPlayer _videoPlayer;
        [SerializeField] UnityEvent _onVideoFinished;
        [SerializeField] UnityEvent _onVideoStarted;
        [SerializeField] UnityEvent _onVideoPaused;
        [SerializeField] bool _stopOnFinish = true;
        [SerializeField] bool _disableOnFinish = false;
        [SerializeField] bool _showDebugLogs = true;

        bool _hasFinished;
        bool _hasStarted;
        long _lastFrame;
        float _lastFrameChangeRealtime;

        void Awake()
        {
            if (_videoPlayer == null)
                _videoPlayer = GetComponent<VideoPlayer>();

            if (_videoPlayer == null)
            {
                Debug.LogError($"[VideoPlayerEventTrigger] No VideoPlayer on {gameObject.name}.");
                enabled = false;
                return;
            }

            _videoPlayer.playOnAwake = false;
            _videoPlayer.waitForFirstFrame = false;
            _videoPlayer.skipOnDrop = true;
            _videoPlayer.sendFrameReadyEvents = false;
        }

        void OnEnable()
        {
            if (_videoPlayer == null)
                return;

            _hasFinished = false;
            _hasStarted = false;
            _videoPlayer.loopPointReached += OnVideoFinished;
            _videoPlayer.started += OnVideoStarted;
            _videoPlayer.prepareCompleted += OnVideoPrepared;
            _videoPlayer.errorReceived += OnVideoError;
            StartCoroutine(PlayAndWatch());
        }

        void OnDisable()
        {
            StopAllCoroutines();
            if (_videoPlayer == null)
                return;

            _videoPlayer.loopPointReached -= OnVideoFinished;
            _videoPlayer.started -= OnVideoStarted;
            _videoPlayer.prepareCompleted -= OnVideoPrepared;
            _videoPlayer.errorReceived -= OnVideoError;
        }

        IEnumerator PlayAndWatch()
        {
            yield return null;

            if (_videoPlayer.clip == null && string.IsNullOrEmpty(_videoPlayer.url))
            {
                Log("No video clip; skipping to next scene.");
                Finish();
                yield break;
            }

            if (_videoPlayer.isPlaying)
                _videoPlayer.Stop();

            _videoPlayer.Prepare();

            float prepareDeadline = Time.realtimeSinceStartup + StartTimeoutSeconds;
            while (!_hasFinished && !_videoPlayer.isPrepared && Time.realtimeSinceStartup < prepareDeadline)
                yield return null;

            if (_hasFinished)
                yield break;

            if (!_videoPlayer.isPrepared)
            {
                Log("Prepare timed out; skipping.");
                Finish();
                yield break;
            }

            _videoPlayer.Play();

            float startedAt = Time.realtimeSinceStartup;
            float maxPlaySeconds = 20f;
            if (_videoPlayer.length > 0.01)
            {
                float speed = Mathf.Max(0.1f, _videoPlayer.playbackSpeed);
                maxPlaySeconds = Mathf.Clamp((float)_videoPlayer.length / speed + 3f, 8f, 45f);
            }

            _lastFrame = -1;
            _lastFrameChangeRealtime = Time.realtimeSinceStartup;

            while (!_hasFinished)
            {
                if (WantsSkip() && Time.realtimeSinceStartup - startedAt >= SkipInputDelay)
                {
                    Log("Skipped by input.");
                    Finish();
                    yield break;
                }

                if (_videoPlayer.isPlaying)
                {
                    long frame = _videoPlayer.frame;
                    if (frame != _lastFrame)
                    {
                        _lastFrame = frame;
                        _lastFrameChangeRealtime = Time.realtimeSinceStartup;
                    }
                    else if (Time.realtimeSinceStartup - _lastFrameChangeRealtime > StuckFrameSeconds
                             && Time.realtimeSinceStartup - startedAt > StartTimeoutSeconds * 0.5f)
                    {
                        Log("Playback froze on one frame; skipping.");
                        Finish();
                        yield break;
                    }
                }

                if (HasReachedEnd())
                {
                    Finish();
                    yield break;
                }

                if (Time.realtimeSinceStartup - startedAt > maxPlaySeconds)
                {
                    Log("Playback exceeded expected length; skipping.");
                    Finish();
                    yield break;
                }

                if (!_hasStarted && !_videoPlayer.isPlaying
                    && Time.realtimeSinceStartup - startedAt > StartTimeoutSeconds)
                {
                    Log("Video never started; skipping.");
                    Finish();
                    yield break;
                }

                yield return null;
            }
        }

        bool HasReachedEnd()
        {
            if (_videoPlayer == null || _hasFinished)
                return false;

            if (_videoPlayer.length > 0.05 && _videoPlayer.time >= _videoPlayer.length - 0.08)
                return true;

            if (_hasStarted && !_videoPlayer.isPlaying && _videoPlayer.frame > 0 && !_videoPlayer.isPaused)
                return true;

            return false;
        }

        static bool WantsSkip()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                return true;
            if (Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame))
                return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space);
#else
            return false;
#endif
        }

        void OnVideoFinished(VideoPlayer vp) => Finish();

        void OnVideoStarted(VideoPlayer vp)
        {
            if (_hasFinished)
                return;

            _hasStarted = true;
            _lastFrameChangeRealtime = Time.realtimeSinceStartup;
            if (_showDebugLogs)
                Debug.Log($"[VideoPlayerEventTrigger] Video started on {gameObject.name}");
            _onVideoStarted?.Invoke();
        }

        void OnVideoPrepared(VideoPlayer vp)
        {
            if (_hasFinished || vp == null)
                return;

            if (_showDebugLogs)
                Debug.Log($"[VideoPlayerEventTrigger] Video prepared on {gameObject.name}");

            if (!vp.isPlaying)
                vp.Play();
        }

        void OnVideoError(VideoPlayer vp, string message)
        {
            Debug.LogWarning($"[VideoPlayerEventTrigger] {message}");
            Finish();
        }

        void Finish()
        {
            if (_hasFinished)
                return;

            _hasFinished = true;
            if (_showDebugLogs)
                Debug.Log($"[VideoPlayerEventTrigger] Video finished on {gameObject.name}");

            _onVideoFinished?.Invoke();

            if (_videoPlayer != null && _stopOnFinish && !_videoPlayer.isLooping)
                _videoPlayer.Stop();

            if (_disableOnFinish)
                gameObject.SetActive(false);

            if ((_onVideoFinished == null || _onVideoFinished.GetPersistentEventCount() == 0)
                && IsIntroSceneStillLoaded())
                SceneManager.LoadScene(GameConstants.Scene.MainMenu);
        }

        bool IsIntroSceneStillLoaded()
        {
            Scene scene = SceneManager.GetActiveScene();
            return scene.IsValid() && scene.name == GameConstants.Scene.Intro;
        }

        void Log(string message)
        {
            if (_showDebugLogs)
                Debug.LogWarning($"[VideoPlayerEventTrigger] {message}");
        }

        [ContextMenu("Play Video")]
        public void PlayVideo()
        {
            if (_videoPlayer == null)
                return;
            _hasFinished = false;
            _hasStarted = false;
            StopAllCoroutines();
            StartCoroutine(PlayAndWatch());
        }

        [ContextMenu("Stop Video")]
        public void StopVideo()
        {
            if (_videoPlayer != null)
                _videoPlayer.Stop();
        }

        public void PauseVideo()
        {
            if (_videoPlayer == null)
                return;
            _videoPlayer.Pause();
            _onVideoPaused?.Invoke();
        }

        public void ResetVideo()
        {
            if (_videoPlayer == null)
                return;
            _videoPlayer.time = 0;
            _hasFinished = false;
        }

        public float GetVideoProgress()
        {
            if (_videoPlayer != null && _videoPlayer.length > 0)
                return (float)(_videoPlayer.time / _videoPlayer.length);
            return 0f;
        }

        public bool IsPlaying() => _videoPlayer != null && _videoPlayer.isPlaying;
    }
}
