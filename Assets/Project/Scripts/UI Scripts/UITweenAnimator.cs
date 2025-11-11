using System.Collections;
using UnityEngine;

public class UITweenAnimator : MonoBehaviour
{
    public enum AnimationDirection
    {
        Left,
        Right,
        Top,
        Bottom
    }

    private static UITweenAnimator _instance;

    public static UITweenAnimator Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new("UITweenAnimator");
                _instance = go.AddComponent<UITweenAnimator>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else if (_instance != this)
            Destroy(gameObject);
    }

    /// <summary>
    /// Animates a panel entry (from direction to center)
    /// </summary>
    public void AnimatePanelEntry(RectTransform rectTransform, AnimationDirection direction, float duration, AnimationCurve curve, System.Action onComplete = null)
    {
        if (rectTransform == null)
        {
            Debug.LogWarning("RectTransform is null!");
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(AnimatePanelCoroutine(rectTransform, direction, duration, curve, true, onComplete));
    }

    /// <summary>
    /// Animates a panel exit (from center to direction)
    /// </summary>
    public void AnimatePanelExit(RectTransform rectTransform, AnimationDirection direction, float duration, AnimationCurve curve, System.Action onComplete = null)
    {
        if (rectTransform == null)
        {
            Debug.LogWarning("RectTransform is null!");
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(AnimatePanelCoroutine(rectTransform, direction, duration, curve, false, onComplete));
    }

    /// <summary>
    /// Animates fade in effect
    /// </summary>
    public void AnimateFadeIn(CanvasGroup canvasGroup, float duration, AnimationCurve curve, System.Action onComplete = null)
    {
        if (canvasGroup == null)
        {
            Debug.LogWarning("CanvasGroup is null!");
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(AnimateFadeCoroutine(canvasGroup, duration, curve, true, onComplete));
    }

    /// <summary>
    /// Animates fade out effect
    /// </summary>
    public void AnimateFadeOut(CanvasGroup canvasGroup, float duration, AnimationCurve curve, System.Action onComplete = null)
    {
        if (canvasGroup == null)
        {
            Debug.LogWarning("CanvasGroup is null!");
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(AnimateFadeCoroutine(canvasGroup, duration, curve, false, onComplete));
    }

    private IEnumerator AnimatePanelCoroutine(RectTransform rectTransform, AnimationDirection direction, float duration, AnimationCurve curve, bool isEntry, System.Action onComplete)
    {
        Vector2 startPos;
        Vector2 endPos;

        if (isEntry)
        {
            // Entry: FROM direction TO center
            startPos = GetPositionFromDirection(rectTransform, direction);
            endPos = Vector2.zero;
        }
        else
        {
            // Exit: FROM center TO direction
            startPos = Vector2.zero;
            endPos = GetPositionFromDirection(rectTransform, direction);
        }

        rectTransform.anchoredPosition = startPos;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Apply the curve
            float curveValue = curve.Evaluate(t);

            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, curveValue);

            yield return null;
        }

        rectTransform.anchoredPosition = endPos;
        onComplete?.Invoke();
    }

    private IEnumerator AnimateFadeCoroutine(CanvasGroup canvasGroup, float duration, AnimationCurve curve, bool isFadeIn, System.Action onComplete)
    {
        float startAlpha = isFadeIn ? 0f : 1f;
        float endAlpha = isFadeIn ? 1f : 0f;

        canvasGroup.alpha = startAlpha;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Apply the curve
            float curveValue = curve.Evaluate(t);

            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, curveValue);

            yield return null;
        }

        canvasGroup.alpha = endAlpha;
        onComplete?.Invoke();
    }

    private Vector2 GetPositionFromDirection(RectTransform rectTransform, AnimationDirection direction)
    {
        Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
        RectTransform canvasRect = canvas != null ? canvas.GetComponent<RectTransform>() : null;

        float screenWidth = canvasRect != null ? canvasRect.rect.width : Screen.width;
        float screenHeight = canvasRect != null ? canvasRect.rect.height : Screen.height;

        return direction switch
        {
            AnimationDirection.Left => new Vector2(-screenWidth, 0),
            AnimationDirection.Right => new Vector2(screenWidth, 0),
            AnimationDirection.Top => new Vector2(0, screenHeight),
            AnimationDirection.Bottom => new Vector2(0, -screenHeight),
            _ => Vector2.zero,
        };
    }
}