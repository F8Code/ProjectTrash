using UnityEngine;

public class AmbiancePlayer : MonoBehaviour
{
    [Header("Ambiance Settings")]
    [Tooltip("Ambiance AudioClip")]
    public AudioClip AmbianceSound;

    [Tooltip("Ambiance volume multiplier")]
    [SerializeField, Range(0f, 1f)] private float _ambianceVolume = 1f;

    void Start()
    {
        AudioManager.Instance.PlayAudio(
            AmbianceSound,
            _ambianceVolume,
            AudioPlaybackContext.PlaybackPriority.Medium,
            transform.position,
            true);
    }
}
