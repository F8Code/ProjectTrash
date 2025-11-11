using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Plays audio effects on button hover and click events
/// Attach this component to any UI Button to add sound feedback
/// </summary>
[RequireComponent(typeof(Button))]
public class UIButtonSoundEffect : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Hover Sound")]
    [SerializeField] private bool playHoverSound = true;

    [SerializeField] private AudioClip hoverSoundClip;
    [SerializeField, Range(0f, 1f)] private float hoverSoundVolume = 0.5f;

    [Header("Click Sound")]
    [SerializeField] private bool playClickSound = true;

    [SerializeField] private AudioClip clickSoundClip;
    [SerializeField, Range(0f, 1f)] private float clickSoundVolume = 1f;

    [Header("Settings")]
    [SerializeField] private AudioPlaybackContext.PlaybackPriority soundPriority = AudioPlaybackContext.PlaybackPriority.Low;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    /// <summary>
    /// Called when pointer enters the button area
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Only play hover sound if button is interactable
        if (!_button.interactable) return;

        if (playHoverSound && hoverSoundClip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAudio(
                   hoverSoundClip,
         hoverSoundVolume,
        soundPriority
               );
        }
    }

    /// <summary>
    /// Called when pointer clicks on the button
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // Only play click sound if button is interactable
        if (!_button.interactable) return;

        if (playClickSound && clickSoundClip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAudio(
           clickSoundClip,
          clickSoundVolume,
                    soundPriority
           );
        }
    }

    /// <summary>
    /// Manually trigger hover sound (useful for custom interactions)
    /// </summary>
    public void PlayHoverSound()
    {
        if (playHoverSound && hoverSoundClip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAudio(
                    hoverSoundClip,
            hoverSoundVolume,
                 soundPriority
                      );
        }
    }

    /// <summary>
    /// Manually trigger click sound (useful for custom interactions)
    /// </summary>
    public void PlayClickSound()
    {
        if (playClickSound && clickSoundClip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAudio(
            clickSoundClip,
          clickSoundVolume,
             soundPriority
           );
        }
    }
}