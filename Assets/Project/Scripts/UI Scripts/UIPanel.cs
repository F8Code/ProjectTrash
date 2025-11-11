using System;
using UnityEngine;

[Serializable]
public class UIPanel
{
    public enum AnimationType
    {
        Slide,
        Fade
    }

    public string panelName;
    public GameObject panelObject;
    public bool startActive;

    [Header("Animation Settings")]
    public bool useAnimation = true;

    public AnimationType animationType = AnimationType.Slide;
    public float animationDuration = 0.5f;

    [Header("Slide Animation")]
    public UITweenAnimator.AnimationDirection entryDirection = UITweenAnimator.AnimationDirection.Left;

    public UITweenAnimator.AnimationDirection exitDirection = UITweenAnimator.AnimationDirection.Right;
    public AnimationCurve entryCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve exitCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Fade Animation")]
    public AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Sound Settings")]
    public bool playSound = false;

    public AudioClip entrySoundClip;
    [Range(0f, 1f)] public float entrySoundVolume = 1f;
    public AudioClip exitSoundClip;
    [Range(0f, 1f)] public float exitSoundVolume = 1f;
    public AudioPlaybackContext.PlaybackPriority soundPriority = AudioPlaybackContext.PlaybackPriority.Medium;

    [HideInInspector] public RectTransform rectTransform;
    [HideInInspector] public Canvas canvas;
    [HideInInspector] public RectTransform animatedTransform;
    [HideInInspector] public CanvasGroup canvasGroup;

    public void Initialize()
    {
        if (panelObject != null)
        {
            // Check if this is a Canvas GameObject
            canvas = panelObject.GetComponent<Canvas>();

            if (canvas != null)
            {
                rectTransform = canvas.GetComponent<RectTransform>();

                // Look for a child panel to animate (optional)
                if (canvas.transform.childCount > 0)
                {
                    Transform firstChild = canvas.transform.GetChild(0);
                    animatedTransform = firstChild.GetComponent<RectTransform>();

                    // If no child with RectTransform, animate the canvas itself
                    if (animatedTransform == null)
                        animatedTransform = rectTransform;
                }
                else
                    animatedTransform = rectTransform;
            }
            else
            {
                // This is a regular panel with RectTransform
                rectTransform = panelObject.GetComponent<RectTransform>();
                animatedTransform = rectTransform;

                if (rectTransform == null)
                    Debug.LogWarning($"Panel '{panelName}' doesn't have a RectTransform or Canvas component!");
            }

            // Ensure CanvasGroup exists for fade animations
            if (animationType == AnimationType.Fade)
            {
                canvasGroup = panelObject.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = panelObject.AddComponent<CanvasGroup>();
                }
            }

            // Set initial active state - only control Canvas, NOT GameObject
            if (canvas != null)
                canvas.enabled = startActive;
            else
                panelObject.SetActive(startActive);
        }
    }

    public void SetActive(bool active)
    {
        if (panelObject != null)
        {
            if (canvas != null)
                canvas.enabled = active;
            else
                panelObject.SetActive(active);
        }
    }

    public bool IsActive()
    {
        if (panelObject == null) return false;

        if (canvas != null)
            return canvas.enabled;

        return panelObject.activeSelf;
    }
}