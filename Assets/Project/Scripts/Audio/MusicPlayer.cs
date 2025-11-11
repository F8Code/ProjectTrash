using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [Header("Music Settings")]
    [Tooltip("Music AudioClip")]
    public AudioClip MusicSound;

    [Tooltip("Music volume multiplier")]
    [SerializeField, Range(0f, 1f)] private float _musicVolume = 0.5f;

    void Start()
    {
        AudioManager.Instance.PlayAudio(
            MusicSound,
            _musicVolume,
            AudioPlaybackContext.PlaybackPriority.Low,
            transform.position,
            true,
            1,
            AudioManager.AudioMixerType.Music);
    }
}
