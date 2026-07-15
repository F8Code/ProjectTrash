using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public enum AudioMixerType
    {
        Master,
        Music,
        SFX
    }

    [SerializeField] private AudioSource _audioSourcePrefab;
    [SerializeField, Range(1, 50)] private int _audioSourcePoolInitialCapacity = 10;
    [SerializeField, Range(0f, 1f)] private float _masterVolumeMultiplier = 1f;
    
    [SerializeField] private AudioMixerGroup _masterMixerGroup;
    [SerializeField] private AudioMixerGroup _musicMixerGroup;
    [SerializeField] private AudioMixerGroup _sfxMixerGroup;


    [Header("Ending Timer (time based gamemode)")]
    [SerializeField] private AudioSource _endingTimer;
    [SerializeField] private float _fadeDuration = 2f;
    [SerializeField, Range(0f, 10f)] private float _endTime = 7f;

    private bool warningPlaying = false;
    private Coroutine fadeCoroutine;


    private AudioPlaybackOrchestrator _orchestrator;
    private IAudioSourcePoolStrategy _poolStrategy;
    private Dictionary<AudioPlaybackContext.PlaybackPriority, IAudioPlaybackBehavior> _behaviorRegistry;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeAudioSubsystems();
    }

    private void InitializeAudioSubsystems()
    {
        _poolStrategy = new DynamicAudioSourcePoolStrategy(
            new AudioSourceFactory(_audioSourcePrefab, transform),
            _audioSourcePoolInitialCapacity
        );

        _behaviorRegistry = new Dictionary<AudioPlaybackContext.PlaybackPriority, IAudioPlaybackBehavior>
        {
            { AudioPlaybackContext.PlaybackPriority.Low, new StandardAudioPlaybackBehavior() },
            { AudioPlaybackContext.PlaybackPriority.Medium, new StandardAudioPlaybackBehavior() },
            { AudioPlaybackContext.PlaybackPriority.High, new PriorityAudioPlaybackBehavior() },
            { AudioPlaybackContext.PlaybackPriority.Critical, new InterruptiveAudioPlaybackBehavior() }
        };

        _orchestrator = new AudioPlaybackOrchestrator(
            _poolStrategy,
            _behaviorRegistry,
            new AudioVolumeModulationStrategy(_masterVolumeMultiplier)
        );
    }

    /// <summary>
    /// Executes audio playback through the stratified orchestration pipeline
    /// </summary>
    public void PlayAudio(AudioClip clip, float volumeMultiplier = 1f, AudioPlaybackContext.PlaybackPriority priority = AudioPlaybackContext.PlaybackPriority.Medium, Vector3? spatialPosition = null, bool loopAudioClip = false, float pitch = 1f, AudioMixerType mixerType = AudioMixerType.SFX)
    {
        if (clip == null) return;

        var context = AudioPlaybackContextFactory.CreateContext(
            clip,
            volumeMultiplier,
            priority,
            spatialPosition.HasValue,
            spatialPosition ?? Vector3.zero,
            GetMixerGroup(mixerType)
        );

        _orchestrator.ExecutePlaybackRequest(context, loopAudioClip, pitch);
    }

    private AudioMixerGroup GetMixerGroup(AudioMixerType mixerType)
    {
        return mixerType switch
        {
            AudioMixerType.Master => _masterMixerGroup,
            AudioMixerType.Music => _musicMixerGroup,
            AudioMixerType.SFX => _sfxMixerGroup,
            _ => _sfxMixerGroup
        };
    }

    public void UpdateEndingTimer(float timeRemaining)
    {
        if (timeRemaining <= _endTime && !warningPlaying)
        {
            warningPlaying = true;

            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            _endingTimer.volume = 0f;
            _endingTimer.Play();

            fadeCoroutine = StartCoroutine(FadeAudio(1f));
        }
        else if (timeRemaining > _endTime && warningPlaying)
        {
            warningPlaying = false;

            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeOutAndStop());
        }
    }

    public void StopEndingTimer()
    {
        StartCoroutine(FadeOutAndStop());
    }

    IEnumerator FadeAudio(float targetVolume)
    {
        float startVolume = _endingTimer.volume;
        float elapsed = 0f;

        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            _endingTimer.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / _fadeDuration);
            yield return null;
        }

        _endingTimer.volume = targetVolume;
    }

    IEnumerator FadeOutAndStop()
    {
        yield return FadeAudio(0f);
        _endingTimer.Stop();
    }

    private void OnDestroy()
    {
        _orchestrator?.Dispose();
    }
}

#region Audio Playback Context System

public class AudioPlaybackContext
{
    public enum PlaybackPriority
    { Low = 0, Medium = 1, High = 2, Critical = 3 }

    public AudioClip Clip { get; }
    public float VolumeMultiplier { get; }
    public PlaybackPriority Priority { get; }
    public bool IsSpatial { get; }
    public Vector3 SpatialPosition { get; }
    public float Timestamp { get; }
    public UnityEngine.Audio.AudioMixerGroup MixerGroup { get; }

    private AudioPlaybackContext(AudioClip clip, float volumeMultiplier, PlaybackPriority priority,
        bool isSpatial, Vector3 spatialPosition, UnityEngine.Audio.AudioMixerGroup mixerGroup)
    {
        Clip = clip;
        VolumeMultiplier = volumeMultiplier;
        Priority = priority;
        IsSpatial = isSpatial;
        SpatialPosition = spatialPosition;
        Timestamp = Time.time;
        MixerGroup = mixerGroup;
    }

    public static class Builder
    {
        public static AudioPlaybackContext Build(AudioClip clip, float volumeMultiplier,
PlaybackPriority priority, bool isSpatial, Vector3 spatialPosition, UnityEngine.Audio.AudioMixerGroup mixerGroup)
  => new(clip, volumeMultiplier, priority, isSpatial, spatialPosition, mixerGroup);
    }
}

public static class AudioPlaybackContextFactory
{
    public static AudioPlaybackContext CreateContext(AudioClip clip, float volumeMultiplier,
        AudioPlaybackContext.PlaybackPriority priority, bool isSpatial, Vector3 spatialPosition, UnityEngine.Audio.AudioMixerGroup mixerGroup)
=> AudioPlaybackContext.Builder.Build(clip, volumeMultiplier, priority, isSpatial, spatialPosition, mixerGroup);
}

#endregion Audio Playback Context System

#region Audio Source Pool Management System

public interface IAudioSourcePoolStrategy
{
    AudioSource AcquireAudioSource();

    void ReleaseAudioSource(AudioSource source);

    void DisposePool();
}

public class DynamicAudioSourcePoolStrategy : IAudioSourcePoolStrategy
{
    private readonly Queue<AudioSource> _availablePool;
    private readonly HashSet<AudioSource> _activePool;
    private readonly IAudioSourceFactory _factory;
    private readonly int _initialCapacity;

    public DynamicAudioSourcePoolStrategy(IAudioSourceFactory factory, int initialCapacity)
    {
        _factory = factory;
        _initialCapacity = initialCapacity;
        _availablePool = new Queue<AudioSource>(initialCapacity);
        _activePool = new HashSet<AudioSource>();

        PrewarmPool();
    }

    private void PrewarmPool()
    {
        for (int i = 0; i < _initialCapacity; i++)
            _availablePool.Enqueue(_factory.CreateAudioSource());
    }

    public AudioSource AcquireAudioSource()
    {
        var source = _availablePool.Count > 0
            ? _availablePool.Dequeue()
            : _factory.CreateAudioSource();

        _activePool.Add(source);
        return source;
    }

    public void ReleaseAudioSource(AudioSource source)
    {
        if (_activePool.Remove(source))
        {
            source.Stop();
            source.clip = null;
            _availablePool.Enqueue(source);
        }
    }

    public void DisposePool()
    {
        foreach (var source in _activePool.Concat(_availablePool).Where(s => s != null))
            UnityEngine.Object.Destroy(source.gameObject);

        _availablePool.Clear();
        _activePool.Clear();
    }
}

public interface IAudioSourceFactory
{
    AudioSource CreateAudioSource();
}

public class AudioSourceFactory : IAudioSourceFactory
{
    private readonly AudioSource _prefab;
    private readonly Transform _parent;

    public AudioSourceFactory(AudioSource prefab, Transform parent)
    {
        _prefab = prefab;
        _parent = parent;
    }

    public AudioSource CreateAudioSource()
    {
        var instance = UnityEngine.Object.Instantiate(_prefab, _parent);
        instance.playOnAwake = false;
        return instance;
    }
}

#endregion Audio Source Pool Management System

#region Audio Playback Behavior Strategy System

public interface IAudioPlaybackBehavior
{
    void ExecutePlayback(AudioSource source, AudioPlaybackContext context,
        IAudioVolumeModulationStrategy volumeStrategy);
}

public class StandardAudioPlaybackBehavior : IAudioPlaybackBehavior
{
    public void ExecutePlayback(AudioSource source, AudioPlaybackContext context,
        IAudioVolumeModulationStrategy volumeStrategy)
    {
        ConfigureAudioSource(source, context, volumeStrategy);
        source.Play();
    }

    protected void ConfigureAudioSource(AudioSource source, AudioPlaybackContext context,
        IAudioVolumeModulationStrategy volumeStrategy)
    {
      source.clip = context.Clip;
        source.volume = volumeStrategy.CalculateModulatedVolume(context.VolumeMultiplier);
        source.spatialBlend = context.IsSpatial ? 1f : 0f;
        source.outputAudioMixerGroup = context.MixerGroup;

        if (context.IsSpatial)
source.transform.position = context.SpatialPosition;
    }
}

public class PriorityAudioPlaybackBehavior : StandardAudioPlaybackBehavior
{
    public new void ExecutePlayback(AudioSource source, AudioPlaybackContext context,
        IAudioVolumeModulationStrategy volumeStrategy)
    {
        ConfigureAudioSource(source, context, volumeStrategy);
        source.priority = 64; // Higher priority in Unity's system (lower number = higher priority)
        source.Play();
    }
}

public class InterruptiveAudioPlaybackBehavior : StandardAudioPlaybackBehavior
{
    public new void ExecutePlayback(AudioSource source, AudioPlaybackContext context,
        IAudioVolumeModulationStrategy volumeStrategy)
    {
        if (source.isPlaying)
            source.Stop();

        ConfigureAudioSource(source, context, volumeStrategy);
        source.priority = 0; // Highest priority
        source.Play();
    }
}

#endregion Audio Playback Behavior Strategy System

#region Volume Modulation System

public interface IAudioVolumeModulationStrategy
{
    float CalculateModulatedVolume(float volumeMultiplier);
}

public class AudioVolumeModulationStrategy : IAudioVolumeModulationStrategy
{
    private readonly float _masterVolume;

    public AudioVolumeModulationStrategy(float masterVolume)
    {
        _masterVolume = Mathf.Clamp01(masterVolume);
    }

    public float CalculateModulatedVolume(float volumeMultiplier)
        => Mathf.Clamp01(_masterVolume * volumeMultiplier);
}

#endregion Volume Modulation System

#region Audio Playback Orchestration System

public class AudioPlaybackOrchestrator : IDisposable
{
    private readonly IAudioSourcePoolStrategy _poolStrategy;
    private readonly Dictionary<AudioPlaybackContext.PlaybackPriority, IAudioPlaybackBehavior> _behaviorRegistry;
    private readonly IAudioVolumeModulationStrategy _volumeStrategy;
    private readonly Dictionary<AudioSource, AudioPlaybackMonitor> _activePlaybacks;

    public AudioPlaybackOrchestrator(
        IAudioSourcePoolStrategy poolStrategy,
        Dictionary<AudioPlaybackContext.PlaybackPriority, IAudioPlaybackBehavior> behaviorRegistry,
        IAudioVolumeModulationStrategy volumeStrategy)
    {
        _poolStrategy = poolStrategy;
        _behaviorRegistry = behaviorRegistry;
        _volumeStrategy = volumeStrategy;
        _activePlaybacks = new Dictionary<AudioSource, AudioPlaybackMonitor>();
    }

    public void ExecutePlaybackRequest(AudioPlaybackContext context, bool loopAudioClip, float pitch)
    {
        var source = _poolStrategy.AcquireAudioSource();
        source.loop = loopAudioClip;
        source.pitch = pitch;

        var behavior = _behaviorRegistry[context.Priority];

        behavior.ExecutePlayback(source, context, _volumeStrategy);

        var monitor = new AudioPlaybackMonitor(source, context,
                () => OnPlaybackCompleted(source));

        _activePlaybacks[source] = monitor;
        monitor.StartMonitoring();
    }

    private void OnPlaybackCompleted(AudioSource source)
    {
        if (_activePlaybacks.TryGetValue(source, out var monitor))
        {
            monitor.Dispose();
            _activePlaybacks.Remove(source);
            _poolStrategy.ReleaseAudioSource(source);
        }
    }

    public void Dispose()
    {
        foreach (var monitor in _activePlaybacks.Values)
            monitor?.Dispose();

        _activePlaybacks.Clear();
        _poolStrategy?.DisposePool();
    }
}

public class AudioPlaybackMonitor : IDisposable
{
    private readonly AudioSource _source;
    private readonly AudioPlaybackContext _context;
    private readonly Action _onCompleted;
    private MonoBehaviourCoroutineRunner _coroutineRunner;

    public AudioPlaybackMonitor(AudioSource source, AudioPlaybackContext context, Action onCompleted)
    {
        _source = source;
        _context = context;
        _onCompleted = onCompleted;
    }

    public void StartMonitoring()
    {
        _coroutineRunner = _source.gameObject.AddComponent<MonoBehaviourCoroutineRunner>();
        _coroutineRunner.StartCoroutine(MonitorPlaybackRoutine());
    }

    private System.Collections.IEnumerator MonitorPlaybackRoutine()
    {
        yield return new WaitWhile(() => _source.isPlaying);
        _onCompleted?.Invoke();
    }

    public void Dispose()
    {
        if (_coroutineRunner != null)
            UnityEngine.Object.Destroy(_coroutineRunner);
    }
}

public class MonoBehaviourCoroutineRunner : MonoBehaviour
{ }

#endregion Audio Playback Orchestration System