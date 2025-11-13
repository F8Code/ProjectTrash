using UnityEngine;

/// <summary>
/// Scrolls credits upward automatically when enabled
/// Resets and restarts the animation every time the canvas is enabled
/// </summary>
public class CreditsScroller : MonoBehaviour
{
    [Header("Scroll Settings")]
    [SerializeField] private float _scrollSpeed = 50f;

    [Tooltip("The RectTransform that contains the credits text")]
    [SerializeField] private RectTransform _creditsContent;

    [Header("Auto Reset Settings")]
    [SerializeField] private float _resetDelay = 0f;

    [Tooltip("Optional: Reset when credits reach this Y position")]
    [SerializeField] private float _resetAtYPosition = 2000f;

    [SerializeField] private bool _useAutoReset = false;

    private Vector2 _startPosition;
    private bool _isScrolling = false;
    private Canvas _canvas;
    private bool _wasCanvasEnabled = false;

    private void Awake()
    {
        if (_creditsContent == null)
            _creditsContent = GetComponent<RectTransform>();

        // Get the Canvas component
        _canvas = GetComponent<Canvas>();
        if (_canvas == null)
            Debug.LogWarning("CreditsScroller: No Canvas component found on this GameObject!");

        // Store the initial position
        _startPosition = _creditsContent.anchoredPosition;
    }

    private void Update()
    {
        // Check if Canvas component state has changed
        if (_canvas != null)
        {
            bool isCanvasEnabled = _canvas.enabled;

            // Detect when canvas becomes enabled
            if (isCanvasEnabled && !_wasCanvasEnabled)
                ResetAndStart();
            else if (!isCanvasEnabled && _wasCanvasEnabled)
                _isScrolling = false;

            _wasCanvasEnabled = isCanvasEnabled;
        }

        if (_isScrolling)
        {
            // Move credits upward
            _creditsContent.anchoredPosition += _scrollSpeed * Time.deltaTime * Vector2.up;

            // Optional: Auto-reset when reaching a certain position
            if (_useAutoReset && _creditsContent.anchoredPosition.y >= _resetAtYPosition)
                ResetAndStart();
        }
    }

    /// <summary>
    /// Resets the credits to starting position and begins scrolling
    /// </summary>
    private void ResetAndStart()
    {
        if (_resetDelay > 0)
        {
            _isScrolling = false;
            _creditsContent.anchoredPosition = _startPosition;
            Invoke(nameof(StartScrolling), _resetDelay);
        }
        else
        {
            _creditsContent.anchoredPosition = _startPosition;
            StartScrolling();
        }
    }

    /// <summary>
    /// Starts the scrolling animation
    /// </summary>
    private void StartScrolling() => _isScrolling = true;
}