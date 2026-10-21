using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementUI : MonoBehaviour
{
    const int GridColumns = 4;
    const int GridRows = 2;

    [SerializeField] Transform _content;
    [SerializeField] TMP_FontAsset _font;

    RectTransform _grid;
    Canvas _canvas;
    bool _canvasWasEnabled;

    void Awake()
    {
        _canvas = GetComponent<Canvas>();
    }

    void OnEnable()
    {
        Refresh();
        StartCoroutine(RefreshNextFrame());
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.LanguageChanged += Refresh;
    }

    void LateUpdate()
    {
        bool canvasEnabled = _canvas != null && _canvas.enabled;
        if (canvasEnabled && !_canvasWasEnabled)
            Refresh();
        _canvasWasEnabled = canvasEnabled;
    }

    IEnumerator RefreshNextFrame()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        Refresh();
    }

    void OnDisable()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.LanguageChanged -= Refresh;
    }

    public void Refresh()
    {
        if (AchievementManager.Instance == null)
            return;

        EnsureGrid();
        if (_grid == null)
            return;

        for (int i = _grid.childCount - 1; i >= 0; i--)
            Destroy(_grid.GetChild(i).gameObject);

        TMP_FontAsset font = _font;
        if (font == null)
        {
            TMP_Text existing = GetComponentInChildren<TMP_Text>(true);
            if (existing != null)
                font = existing.font;
        }

        ConfigureGrid();

        foreach (AchievementDefinition definition in AchievementManager.Instance.Definitions)
        {
            GameObject card = CreateCard(definition, AchievementManager.Instance.IsUnlocked(definition.Id), font);
            card.transform.SetParent(_grid, false);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_grid);
        LocalizationManager.Instance?.RefreshAll();
    }

    void EnsureGrid()
    {
        if (_grid != null)
            return;

        Transform panel = transform.Find("Panel_Achievements");
        if (panel == null && transform.childCount > 0)
            panel = transform.GetChild(0);
        if (panel == null)
            panel = transform;

        Transform existing = panel.Find("AchievementGrid");
        if (existing is RectTransform existingRect)
        {
            _grid = existingRect;
            return;
        }

        Transform leftover = panel.Find("Grid Layout");
        if (leftover != null)
            leftover.gameObject.SetActive(false);

        GameObject gridObject = new("AchievementGrid", typeof(RectTransform), typeof(GridLayoutGroup));
        gridObject.transform.SetParent(panel, false);
        _grid = gridObject.GetComponent<RectTransform>();
        _grid.anchorMin = new Vector2(0.06f, 0.16f);
        _grid.anchorMax = new Vector2(0.94f, 0.78f);
        _grid.offsetMin = Vector2.zero;
        _grid.offsetMax = Vector2.zero;
        _grid.SetSiblingIndex(Mathf.Max(0, panel.childCount - 2));
        Transform backButton = panel.Find("BackButton") ?? panel.Find("MainBackButton");
        if (backButton != null)
            backButton.SetAsLastSibling();
    }

    void ConfigureGrid()
    {
        GridLayoutGroup grid = _grid.GetComponent<GridLayoutGroup>();
        if (grid == null)
            grid = _grid.gameObject.AddComponent<GridLayoutGroup>();

        float width = _grid.rect.width;
        float height = _grid.rect.height;
        if (width < 8f)
            width = 1600f;
        if (height < 8f)
            height = 620f;

        const float padding = 16f;
        const float spacing = 16f;
        float cellWidth = (width - padding * 2f - spacing * (GridColumns - 1)) / GridColumns;
        float cellHeight = (height - padding * 2f - spacing * (GridRows - 1)) / GridRows;

        grid.padding = new RectOffset((int)padding, (int)padding, (int)padding, (int)padding);
        grid.spacing = new Vector2(spacing, spacing);
        grid.cellSize = new Vector2(Mathf.Max(220f, cellWidth), Mathf.Max(180f, cellHeight));
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = GridColumns;
    }

    static GameObject CreateCard(AchievementDefinition definition, bool unlocked, TMP_FontAsset font)
    {
        GameObject card = new($"Achievement_{definition.Id}", typeof(RectTransform), typeof(Image));
        Image background = card.GetComponent<Image>();
        background.color = unlocked ? new Color(1f, 1f, 1f, 0.16f) : new Color(0.15f, 0.15f, 0.15f, 0.72f);
        background.raycastTarget = false;

        VerticalLayoutGroup column = card.AddComponent<VerticalLayoutGroup>();
        column.padding = new RectOffset(12, 12, 12, 12);
        column.spacing = 6f;
        column.childAlignment = TextAnchor.UpperCenter;
        column.childForceExpandHeight = false;
        column.childForceExpandWidth = true;

        GameObject iconObject = new("Icon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        iconObject.transform.SetParent(card.transform, false);
        iconObject.GetComponent<LayoutElement>().preferredHeight = 72f;
        iconObject.GetComponent<LayoutElement>().preferredWidth = 72f;
        Image icon = iconObject.GetComponent<Image>();
        icon.raycastTarget = false;
        icon.color = unlocked ? new Color(0.95f, 0.82f, 0.35f, 1f) : new Color(0.45f, 0.45f, 0.45f, 1f);
        icon.sprite = definition.Icon;

        CreateText(card.transform, definition.Title, 24f, font, FontStyles.Bold);
        CreateText(card.transform, definition.Description, 16f, font, FontStyles.Normal);
        CreateText(card.transform, unlocked ? "Unlocked" : "Locked", 15f, font, FontStyles.Italic);
        return card;
    }

    static void CreateText(Transform parent, string value, float size, TMP_FontAsset font, FontStyles style)
    {
        GameObject textObject = new("Label", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI label = textObject.GetComponent<TextMeshProUGUI>();
        label.text = value;
        label.fontSize = size;
        label.alignment = TextAlignmentOptions.Center;
        label.enableWordWrapping = true;
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.fontStyle = style;
        label.color = Color.white;
        label.raycastTarget = false;
        if (font != null)
            label.font = font;
        textObject.GetComponent<LayoutElement>().flexibleHeight = 1f;
    }
}
