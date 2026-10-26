using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementUI : MonoBehaviour
{
    const int GridColumns = 3;
    const int GridRows = 2;
    const string FramePath = "Assets/Project/Assets/Prefabs/UI Assets/Menu/Buttons/Icons/Framevideo.png";

    [SerializeField] Transform _content;
    [SerializeField] TMP_FontAsset _font;
    [SerializeField] Sprite _frame;

    RectTransform _grid;
    Canvas _canvas;
    bool _canvasWasEnabled;
    static Sprite _frameSprite;

    void Awake()
    {
        _canvas = GetComponent<Canvas>();
        EnsureFrameSprite();
    }

    void OnEnable()
    {
        Refresh();
        StartCoroutine(RefreshNextFrame());
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.LanguageChanged += Refresh;
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.AchievementUnlocked += OnAchievementUnlocked;
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
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.AchievementUnlocked -= OnAchievementUnlocked;
    }

    void OnAchievementUnlocked(AchievementDefinition _) => Refresh();

    public void Refresh()
    {
        if (AchievementManager.Instance == null)
            return;

        EnsureGrid();
        if (_grid == null)
            return;

        EnsureFrameSprite();

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
            GameObject card = CreateCard(definition, AchievementManager.Instance.IsUnlocked(definition.Id), font, _frameSprite);
            card.transform.SetParent(_grid, false);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_grid);
        LocalizationManager.Instance?.RefreshAll();
        foreach (TMP_Text label in _grid.GetComponentsInChildren<TMP_Text>(true))
        {
            bool isTitle = label.transform.parent != null && label.gameObject.name == "Title";
            label.enableWordWrapping = true;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.overflowMode = isTitle ? TextOverflowModes.Overflow : TextOverflowModes.Ellipsis;
        }
    }

    void EnsureFrameSprite()
    {
        if (_frame != null)
            _frameSprite = _frame;
        if (_frameSprite != null)
            return;

        _frameSprite = Resources.Load<Sprite>("UI/Framevideo");
#if UNITY_EDITOR
        if (_frameSprite == null)
            _frameSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(FramePath);
#endif
        if (_frame != null)
            return;
        _frame = _frameSprite;
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
        _grid.SetSiblingIndex(Mathf.Max(0, panel.childCount - 2));
        Transform backButton = panel.Find("BackButton") ?? panel.Find("MainBackButton");
        if (backButton != null)
            backButton.SetAsLastSibling();
    }

    void ConfigureGrid()
    {
        _grid.anchorMin = new Vector2(0.04f, 0.10f);
        _grid.anchorMax = new Vector2(0.96f, 0.82f);
        _grid.offsetMin = Vector2.zero;
        _grid.offsetMax = Vector2.zero;

        GridLayoutGroup grid = _grid.GetComponent<GridLayoutGroup>();
        if (grid == null)
            grid = _grid.gameObject.AddComponent<GridLayoutGroup>();

        RectTransform parent = _grid.parent as RectTransform;
        float parentWidth = parent != null ? parent.rect.width : _grid.rect.width;
        float parentHeight = parent != null ? parent.rect.height : _grid.rect.height;
        float width = parentWidth * (_grid.anchorMax.x - _grid.anchorMin.x);
        float height = parentHeight * (_grid.anchorMax.y - _grid.anchorMin.y);
        if (width < 8f)
            width = 1600f;
        if (height < 8f)
            height = 720f;

        const float padding = 20f;
        const float spacing = 20f;
        float cellWidth = (width - padding * 2f - spacing * (GridColumns - 1)) / GridColumns;
        float cellHeight = (height - padding * 2f - spacing * (GridRows - 1)) / GridRows;

        grid.padding = new RectOffset((int)padding, (int)padding, (int)padding, (int)padding);
        grid.spacing = new Vector2(spacing, spacing);
        grid.cellSize = new Vector2(cellWidth, cellHeight);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.UpperCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = GridColumns;
    }

    static GameObject CreateCard(AchievementDefinition definition, bool unlocked, TMP_FontAsset font, Sprite frame)
    {
        GameObject card = new($"Achievement_{definition.Id}", typeof(RectTransform), typeof(Image));
        Image background = card.GetComponent<Image>();
        background.color = unlocked ? new Color(0.18f, 0.18f, 0.18f, 0.82f) : new Color(0.10f, 0.10f, 0.10f, 0.88f);
        background.raycastTarget = false;

        VerticalLayoutGroup column = card.AddComponent<VerticalLayoutGroup>();
        column.padding = new RectOffset(16, 16, 14, 12);
        column.spacing = 6f;
        column.childAlignment = TextAnchor.UpperCenter;
        column.childForceExpandHeight = false;
        column.childForceExpandWidth = true;
        column.childControlWidth = true;
        column.childControlHeight = true;

        AddFrameOverlay(card.transform, frame);

        CreateText(card.transform, "Title", definition.Title, 48f, font, FontStyles.Bold, 72f, false);
        CreateIcon(card.transform, definition.GetDisplayIcon(unlocked), frame);
        CreateText(card.transform, "Description", definition.Description, 42f, font, FontStyles.Normal, 78f, true);
        return card;
    }

    static void AddFrameOverlay(Transform parent, Sprite frame)
    {
        if (frame == null)
            return;

        GameObject overlay = new("CardFrame", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        overlay.transform.SetParent(parent, false);
        overlay.GetComponent<LayoutElement>().ignoreLayout = true;

        RectTransform rect = overlay.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.SetAsLastSibling();

        Image image = overlay.GetComponent<Image>();
        image.sprite = frame;
        image.type = Image.Type.Sliced;
        image.fillCenter = false;
        image.pixelsPerUnitMultiplier = 1.1f;
        image.color = Color.white;
        image.raycastTarget = false;
        image.preserveAspect = false;
    }

    static void CreateIcon(Transform parent, Sprite iconSprite, Sprite frame)
    {
        GameObject holder = new("IconHolder", typeof(RectTransform), typeof(LayoutElement));
        holder.transform.SetParent(parent, false);
        LayoutElement holderLayout = holder.GetComponent<LayoutElement>();
        holderLayout.preferredHeight = 118f;
        holderLayout.minHeight = 100f;
        holderLayout.preferredWidth = 118f;
        holderLayout.flexibleHeight = 0f;

        GameObject iconObject = new("Icon", typeof(RectTransform), typeof(Image));
        iconObject.transform.SetParent(holder.transform, false);
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.sizeDelta = new Vector2(96f, 96f);
        iconRect.anchoredPosition = Vector2.zero;
        Image icon = iconObject.GetComponent<Image>();
        icon.raycastTarget = false;
        icon.preserveAspect = true;
        icon.color = Color.white;
        icon.sprite = iconSprite;

        if (frame == null)
            return;

        GameObject frameObject = new("IconFrame", typeof(RectTransform), typeof(Image));
        frameObject.transform.SetParent(holder.transform, false);
        RectTransform frameRect = frameObject.GetComponent<RectTransform>();
        frameRect.anchorMin = new Vector2(0.5f, 0.5f);
        frameRect.anchorMax = new Vector2(0.5f, 0.5f);
        frameRect.sizeDelta = new Vector2(118f, 118f);
        frameRect.anchoredPosition = Vector2.zero;
        Image frameImage = frameObject.GetComponent<Image>();
        frameImage.sprite = frame;
        frameImage.type = Image.Type.Sliced;
        frameImage.fillCenter = false;
        frameImage.pixelsPerUnitMultiplier = 1.4f;
        frameImage.color = Color.white;
        frameImage.raycastTarget = false;
        frameImage.preserveAspect = false;
    }

    static void CreateText(Transform parent, string name, string value, float size, TMP_FontAsset font, FontStyles style, float height, bool flexible)
    {
        GameObject textObject = new(name, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI label = textObject.GetComponent<TextMeshProUGUI>();
        label.text = value;
        label.fontSize = size;
        label.alignment = TextAlignmentOptions.Center;
        label.enableWordWrapping = true;
        label.overflowMode = name == "Title" ? TextOverflowModes.Overflow : TextOverflowModes.Ellipsis;
        label.fontStyle = style;
        label.color = Color.white;
        label.raycastTarget = false;
        if (font != null)
            label.font = font;

        LayoutElement layout = textObject.GetComponent<LayoutElement>();
        layout.preferredHeight = height;
        layout.minHeight = height;
        layout.flexibleHeight = flexible ? 1f : 0f;
        layout.flexibleWidth = 1f;
    }
}
