using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Changes cursor when hovering over UI elements or GameObjects
/// Works great alongside CursorClickHandler for complete cursor control
/// Integrates with new Input System via CursorClickHandler singleton
/// </summary>
public class CursorHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Cursor")]
    [Tooltip("Cursor texture when hovering over this element")]
    [SerializeField] private Texture2D _hoverCursor;

    [Tooltip("Hotspot offset for hover cursor")]
    [SerializeField] private Vector2 _hoverHotspot = Vector2.zero;

    [Tooltip("Use center of texture as hotspot")]
    [SerializeField] private bool _useAutoCenterHotspot = true;

    [Tooltip("Cursor mode")]
    [SerializeField] private CursorMode _cursorMode = CursorMode.Auto;

    [Header("Revert Settings")]
    [Tooltip("Should cursor revert when mouse exits?")]
    [SerializeField] private bool _revertOnExit = true;

    [Tooltip("Cursor to revert to (leave null for system default)")]
    [SerializeField] private Texture2D _normalCursor;

    [Tooltip("Hotspot for normal cursor")]
    [SerializeField] private Vector2 _normalHotspot = Vector2.zero;

    [Header("Integration")]
    [Tooltip("Use CursorClickHandler singleton (recommended)")]
    [SerializeField] private bool _useCursorClickHandler = true;

    [Header("3D Object Support")]
    [Tooltip("Enable hover detection for 3D objects (not just UI)")]
    [SerializeField] private bool _enable3DDetection = false;

    [Header("Debug")]
    [SerializeField] private bool _showDebugLogs = false;

    private bool _isHovering = false;

    private void Start()
    {
        // Auto-calculate hotspots if enabled
        if (_useAutoCenterHotspot)
        {
            if (_hoverCursor != null)
                _hoverHotspot = new Vector2(_hoverCursor.width / 2f, _hoverCursor.height / 2f);

            if (_normalCursor != null)
                _normalHotspot = new Vector2(_normalCursor.width / 2f, _normalCursor.height / 2f);
        }

        if (_showDebugLogs)
            Debug.Log($"<color=cyan>[CursorHoverHandler] Initialized on {gameObject.name}</color>");
    }

    #region UI Event Handlers (for UI elements)

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetHoverCursor();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_revertOnExit)
            SetNormalCursor();
    }

    #endregion

    #region 3D Object Detection (for GameObjects with colliders)

    private void OnMouseEnter()
    {
        if (_enable3DDetection)
        {
            SetHoverCursor();
        }
    }

    private void OnMouseExit()
    {
        if (_enable3DDetection && _revertOnExit)
        {
            SetNormalCursor();
        }
    }

    #endregion

    /// <summary>
    /// Set cursor to hover state
    /// </summary>
    public void SetHoverCursor()
    {
        if (_hoverCursor == null)
        {
            if (_showDebugLogs)
                Debug.LogWarning($"<color=yellow>[CursorHoverHandler] No hover cursor assigned on {gameObject.name}</color>");
            return;
        }

        _isHovering = true;
        Cursor.SetCursor(_hoverCursor, _hoverHotspot, _cursorMode);

        // Update CursorClickHandler if using singleton integration
        if (_useCursorClickHandler && CursorClickHandler.Instance != null)
        {
            CursorClickHandler.Instance.SetNormalCursorTexture(_hoverCursor, _hoverHotspot);
        }

        if (_showDebugLogs)
            Debug.Log($"<color=green>[CursorHoverHandler] Hover cursor set on {gameObject.name}</color>");
    }

    /// <summary>
    /// Set cursor to normal state
    /// </summary>
    public void SetNormalCursor()
    {
        _isHovering = false;

        if (_normalCursor != null)
        {
            Cursor.SetCursor(_normalCursor, _normalHotspot, _cursorMode);

            // Update CursorClickHandler if using singleton integration
            if (_useCursorClickHandler && CursorClickHandler.Instance != null)
            {
                CursorClickHandler.Instance.SetNormalCursorTexture(_normalCursor, _normalHotspot);
            }

            if (_showDebugLogs)
                Debug.Log($"<color=cyan>[CursorHoverHandler] Normal cursor set on {gameObject.name}</color>");
        }
        else
        {
            // Reset to system cursor
            Cursor.SetCursor(null, Vector2.zero, _cursorMode);

            if (_showDebugLogs)
                Debug.Log($"<color=cyan>[CursorHoverHandler] Reset to system cursor on {gameObject.name}</color>");
        }
    }

    /// <summary>
    /// Check if currently hovering
    /// </summary>
    public bool IsHovering() => _isHovering;

    /// <summary>
    /// Change hover cursor at runtime
    /// </summary>
    public void SetHoverCursorTexture(Texture2D texture, Vector2? hotspot = null)
    {
        _hoverCursor = texture;

        if (hotspot.HasValue)
            _hoverHotspot = hotspot.Value;
        else if (_useAutoCenterHotspot && texture != null)
            _hoverHotspot = new Vector2(texture.width / 2f, texture.height / 2f);

        if (_isHovering)
            SetHoverCursor();

        if (_showDebugLogs)
            Debug.Log($"<color=cyan>[CursorHoverHandler] Hover texture changed on {gameObject.name}</color>");
    }

    private void OnDisable()
    {
        // Reset cursor when disabled
        if (_isHovering)
        {
            SetNormalCursor();
        }
    }
}
