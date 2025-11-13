using UnityEngine;
using TMPro;

/// <summary>
/// Displays FPS (Frames Per Second) counter on screen
/// </summary>
public class FPSCounter : MonoBehaviour
{
    [Header("Display Settings")]
    [Tooltip("TextMeshProUGUI component to display the FPS")]
    [SerializeField] private TextMeshProUGUI _fpsText;

    [Tooltip("Update frequency for FPS calculation (in seconds)")]
    [SerializeField, Range(0.1f, 2f)] private float _updateInterval = 0.5f;

    [Header("Color Coding")]
    [Tooltip("Color when FPS is good (60+)")]
    [SerializeField] private Color _goodFPSColor = Color.green;

    [Tooltip("Color when FPS is okay (30-59)")]
    [SerializeField] private Color _okayFPSColor = Color.yellow;

    [Tooltip("Color when FPS is poor (<30)")]
    [SerializeField] private Color _poorFPSColor = Color.red;

    private float _accumulatedTime = 0f;
    private int _frames = 0;
    private float _currentFPS = 0f;
    private float _timeLeft;

    private void Start()
    {
        _timeLeft = _updateInterval;

        if (_fpsText == null)
            Debug.LogWarning("FPSCounter: TMP_Text component is not assigned!");
        else
            _fpsText.gameObject.SetActive(enabled);
    }

    private void OnEnable()
    {
        // Show FPS text when enabled
        if (_fpsText != null)
        {
            _fpsText.gameObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        // Hide FPS text when disabled
        if (_fpsText != null)
        {
            _fpsText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        _timeLeft -= Time.deltaTime;
        _accumulatedTime += Time.timeScale / Time.deltaTime;
        _frames++;

        // Update FPS at specified interval
        if (_timeLeft <= 0f)
        {
            _currentFPS = _accumulatedTime / _frames;
            _timeLeft = _updateInterval;
            _accumulatedTime = 0f;
            _frames = 0;

            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        if (_fpsText == null)
            return;

        // Update text
        _fpsText.text = string.Format("FPS: {0:F1}", _currentFPS);

        // Update color based on FPS
        if (_currentFPS >= 60f)
            _fpsText.color = _goodFPSColor;
        else if (_currentFPS >= 30f)
            _fpsText.color = _okayFPSColor;
        else
            _fpsText.color = _poorFPSColor;
    }
}