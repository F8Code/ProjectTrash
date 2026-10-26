using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardUI : MonoBehaviour
{
    [Header("Game Over Panel UI")]
    [SerializeField] private TextMeshProUGUI _gameOverScoreText;

    [SerializeField] private TMP_InputField _playerNameInput;
    [SerializeField] private Button _saveButton;

    [Header("Leaderboard Panel UI")]
    [SerializeField] private Transform _entriesContainer;

    [SerializeField] private GameObject _entryPrefab;

    [Header("Display Settings")]
    [SerializeField] private int _maxVisibleEntries = 10;

    [Header("Player Name Settings")]
    [SerializeField] private string _playerNameKey = "PlayerName";

    [SerializeField] private string _defaultPlayerName = "Anonymous";
    [SerializeField] private int _maxNameLength = 20;

    private readonly List<GameObject> _spawnedEntries = new();
    private int _currentScore;
    private bool _scoreSaved = false;
    private string _savedEntryName = ""; // Track the name that was saved
    private bool _showWorld;
    private Button _sourceToggle;
    private TextMeshProUGUI _sourceToggleLabel;
    private TextMeshProUGUI _boardTitle;
    private TextMeshProUGUI _statusLabel;

    private void Awake()
    {
        if (_saveButton != null)
            _saveButton.onClick.AddListener(OnSaveButtonClicked);
    }

    private void OnEnable()
    {
        SteamLeaderboards.EntriesUpdated += OnWorldEntriesUpdated;
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.LanguageChanged += OnLanguageChanged;
    }

    private void OnDisable()
    {
        SteamLeaderboards.EntriesUpdated -= OnWorldEntriesUpdated;
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.LanguageChanged -= OnLanguageChanged;
    }

    private void Start()
    {
        HideAllPanels();
        EnsureSourceToggle();
        RefreshLeaderboardDisplay();
    }

    private void OnDestroy()
    {
        if (_saveButton != null)
            _saveButton.onClick.RemoveListener(OnSaveButtonClicked);
        if (_sourceToggle != null)
            _sourceToggle.onClick.RemoveListener(OnSourceToggleClicked);
    }

    /// <summary>
    /// Shows the Game Over panel with score - called when player loses
    /// </summary>
    public void ShowGameOver(int finalScore)
    {
        Debug.Log($"<color=cyan>[LeaderboardUI] ShowGameOver called with score: {finalScore}</color>");

        bool newRun = _currentScore != finalScore || !_scoreSaved;
        _currentScore = finalScore;

        if (newRun)
        {
            _scoreSaved = false;
            _savedEntryName = "";
        }

        if (UIManager.Instance != null)
            UIManager.Instance.ShowPanel(GameConstants.Canvas.GameOverPanel);

        if (CursorManager.Instance != null)
            CursorManager.Instance.ShowCursorForUI();

        if (_gameOverScoreText != null)
        {
            string prefix = LocalizationManager.Instance != null ? LocalizationManager.Instance.Get("Score:") : "Score:";
            _gameOverScoreText.text = $"{prefix} {finalScore}";
        }

        MenuUiBootstrap.FixMenuHoverIcons();
        RefreshGameOverAchievements();

        // Load saved player name into input field
        if (_playerNameInput != null)
        {
            string nameToShow = _scoreSaved && !string.IsNullOrEmpty(_savedEntryName)
                  ? _savedEntryName
            : GetPlayerName();

            _playerNameInput.text = nameToShow;
            _playerNameInput.characterLimit = _maxNameLength;
        }

        // Update save button state and text
        UpdateSaveButtonState();
    }

    /// <summary>
    /// Shows the Leaderboard panel - can be called from Game Over or directly
    /// </summary>
    public void ShowLeaderboard()
    {
        Debug.Log("<color=cyan>[LeaderboardUI] ShowLeaderboard called</color>");

        // Use UIManager singleton
        if (UIManager.Instance != null)
            UIManager.Instance.ShowPanel(GameConstants.Canvas.LeaderboardPanel);

        if (CursorManager.Instance != null)
            CursorManager.Instance.ShowCursorForUI();

        EnsureSourceToggle();
        if (_showWorld)
            SteamLeaderboards.RequestWorldEntries();
        RefreshLeaderboardDisplay();
    }

    /// <summary>
    /// Shows the Leaderboard panel - can be called from Game Over or directly
    /// </summary>
    public void ShowGameover()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.ShowPanel(GameConstants.Canvas.GameOverPanel);

        if (CursorManager.Instance != null)
            CursorManager.Instance.ShowCursorForUI();
    }

    /// <summary>
    /// Hides all panels
    /// </summary>
    public void HideAllPanels()
    {
        // Use UIManager singleton
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HidePanel(GameConstants.Canvas.GameOverPanel);
            UIManager.Instance.HidePanel(GameConstants.Canvas.LeaderboardPanel);
            Debug.Log("<color=yellow>[LeaderboardUI] All panels hidden via UIManager</color>");
        }

        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.HideCursorForUI();
            CursorManager.Instance.HideCursorForUI();
        }
    }

    private void UpdateSaveButtonState()
    {
        if (_saveButton != null)
        {
            _saveButton.interactable = true;

            TextMeshProUGUI buttonText = _saveButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                string source = _scoreSaved ? "Update" : "Save";
                buttonText.text = LocalizationManager.Instance != null ? LocalizationManager.Instance.Get(source) : source;
                LocalizationManager.Instance?.ApplyFont(buttonText);
            }
        }
    }

    private void OnSaveButtonClicked()
    {
        string playerName = _playerNameInput != null ? _playerNameInput.text.Trim() : "";

        if (string.IsNullOrWhiteSpace(playerName))
            playerName = _defaultPlayerName;

        if (_scoreSaved && !string.IsNullOrEmpty(_savedEntryName))
        {
            Debug.Log($"<color=yellow>[LeaderboardUI] Renaming entry: {_savedEntryName} → {playerName}</color>");

            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.UpdateEntryName(_savedEntryName, _currentScore, playerName);
                _savedEntryName = playerName;
                SetPlayerName(playerName);
            }
        }
        else
        {
            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.AddEntry(playerName, _currentScore);
                _scoreSaved = true;
                _savedEntryName = playerName;
                SetPlayerName(playerName);

                Debug.Log($"<color=green>[LeaderboardUI] Score saved successfully!</color>");
            }
        }

        ViewLeaderboard();
    }

    private void ViewLeaderboard()
    {
        if (!_scoreSaved)
            OnSaveButtonClicked();

        ShowLeaderboard();
    }

    private void OnWorldEntriesUpdated()
    {
        if (_showWorld)
            RefreshLeaderboardDisplay();
    }

    private void OnLanguageChanged() => RefreshSourceLabels();

    private void OnSourceToggleClicked()
    {
        _showWorld = !_showWorld;
        if (_showWorld)
            SteamLeaderboards.RequestWorldEntries();
        RefreshLeaderboardDisplay();
    }

    private void RefreshLeaderboardDisplay()
    {
        EnsureSourceToggle();
        RefreshSourceLabels();

        foreach (var entry in _spawnedEntries)
            Destroy(entry);

        _spawnedEntries.Clear();

        IReadOnlyList<LeaderboardEntry> entries = _showWorld
            ? SteamLeaderboards.WorldEntries
            : LeaderboardManager.Instance != null
                ? LeaderboardManager.Instance.LeaderboardEntries
                : null;

        bool hideRows = _showWorld && (SteamLeaderboards.IsDownloading || entries == null || entries.Count == 0);
        if (!hideRows && entries != null)
        {
            int visible = LeaderboardManager.MaxVisibleEntries;
            FitEntryList();
            for (int i = 0; i < Mathf.Min(entries.Count, visible); i++)
            {
                if (_entryPrefab != null && _entriesContainer != null)
                {
                    GameObject entryObj = Instantiate(_entryPrefab, _entriesContainer);
                    _spawnedEntries.Add(entryObj);

                    if (!entryObj.TryGetComponent(out LayoutElement layoutElement))
                        layoutElement = entryObj.AddComponent<LayoutElement>();
                    layoutElement.minHeight = 28f;
                    layoutElement.preferredHeight = 32f;

                    if (entryObj.TryGetComponent<LeaderboardEntryUI>(out var entryUI))
                    {
                        int rank = entries[i].Rank > 0 ? entries[i].Rank : i + 1;
                        entryUI.SetData(rank, entries[i].PlayerName, entries[i].Score);
                    }
                }
            }
        }

        Debug.Log($"<color=green>[LeaderboardUI] Spawned {_spawnedEntries.Count} entry objects</color>");
    }

    void FitEntryList()
    {
        if (_entriesContainer == null)
            return;

        if (_entriesContainer.TryGetComponent(out VerticalLayoutGroup layout))
        {
            layout.spacing = 4f;
            layout.padding = new RectOffset(10, 10, 8, 0);
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
        }
    }

    void EnsureSourceToggle()
    {
        Transform panel = FindNamedTransform("Panel_Leaderboard");
        if (panel == null)
            return;

        if (_boardTitle == null)
        {
            Transform title = FindNamedTransform("Txt_Leaderboard");
            if (title != null)
                _boardTitle = title.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (_sourceToggle == null)
        {
            Transform existing = panel.Find("LeaderboardSourceToggle");
            if (existing != null)
                DestroyImmediate(existing.gameObject);

            Transform back = panel.Find("BackButton");
            GameObject toggleObject = back != null
                ? Instantiate(back.gameObject, panel)
                : new GameObject("LeaderboardSourceToggle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));

            toggleObject.name = "LeaderboardSourceToggle";
            RectTransform rect = toggleObject.GetComponent<RectTransform>();
            rect.SetParent(panel, false);
            rect.anchorMin = new Vector2(0.016f, 0.03f);
            rect.anchorMax = new Vector2(0.424f, 0.166f);
            rect.pivot = new Vector2(0f, 0f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;

            Transform logo = toggleObject.transform.Find("Logo");
            if (logo != null)
                logo.gameObject.SetActive(false);

            _sourceToggle = toggleObject.GetComponent<Button>();
            if (_sourceToggle != null)
            {
                _sourceToggle.onClick = new Button.ButtonClickedEvent();
                _sourceToggle.onClick.AddListener(OnSourceToggleClicked);
            }

            _sourceToggleLabel = toggleObject.GetComponentInChildren<TextMeshProUGUI>(true);
            if (_sourceToggleLabel != null)
            {
                RectTransform labelRect = _sourceToggleLabel.rectTransform;
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;
                _sourceToggleLabel.alignment = TextAlignmentOptions.Center;
                _sourceToggleLabel.enableAutoSizing = true;
                _sourceToggleLabel.fontSizeMin = 28f;
                _sourceToggleLabel.fontSizeMax = 72f;
                _sourceToggleLabel.enableWordWrapping = false;
                _sourceToggleLabel.overflowMode = TextOverflowModes.Overflow;
            }
        }

        if (_statusLabel == null)
        {
            Transform existingStatus = panel.Find("LeaderboardStatus");
            if (existingStatus != null)
                DestroyImmediate(existingStatus.gameObject);

            GameObject statusObject = new("LeaderboardStatus", typeof(RectTransform), typeof(TextMeshProUGUI));
            statusObject.transform.SetParent(panel, false);
            RectTransform statusRect = statusObject.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.28f, 0.16f);
            statusRect.anchorMax = new Vector2(0.72f, 0.28f);
            statusRect.offsetMin = Vector2.zero;
            statusRect.offsetMax = Vector2.zero;
            _statusLabel = statusObject.GetComponent<TextMeshProUGUI>();
            _statusLabel.alignment = TextAlignmentOptions.Center;
            _statusLabel.enableWordWrapping = true;
            _statusLabel.fontSize = 28f;
            _statusLabel.raycastTarget = false;
            if (_sourceToggleLabel != null)
            {
                _statusLabel.font = _sourceToggleLabel.font;
                _statusLabel.color = _sourceToggleLabel.color;
            }
            else if (_gameOverScoreText != null)
            {
                _statusLabel.font = _gameOverScoreText.font;
                _statusLabel.color = _gameOverScoreText.color;
            }
        }
    }

    void RefreshSourceLabels()
    {
        SetLocalized(_sourceToggleLabel, _showWorld ? "Local" : "World");
        SetLocalized(_boardTitle, _showWorld ? "World Leaderboard" : "Leaderboard");

        if (_statusLabel != null)
        {
            bool showStatus = _showWorld && (SteamLeaderboards.IsDownloading || SteamLeaderboards.WorldEntries.Count == 0);
            _statusLabel.gameObject.SetActive(showStatus);
            if (showStatus)
            {
                string status = string.IsNullOrEmpty(SteamLeaderboards.Status) ? "No world scores yet" : SteamLeaderboards.Status;
                SetLocalized(_statusLabel, status);
            }
        }
    }

    static void SetLocalized(TMP_Text label, string englishSource)
    {
        if (label == null)
            return;

        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.SetLabelSource(label, englishSource);
        else
            label.text = englishSource;
    }

    static Transform FindNamedTransform(string objectName)
    {
        Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform transform in transforms)
        {
            if (transform.name == objectName)
                return transform;
        }

        return null;
    }

    void RefreshGameOverAchievements()
    {
        Transform panel = FindGameOverPanel();
        if (panel == null)
            return;

        Transform existing = panel.Find("UnlockedThisRun");
        if (existing != null)
            Destroy(existing.gameObject);

        GameObject root = new("UnlockedThisRun", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        root.transform.SetParent(panel, false);

        RectTransform rect = root.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.28f, 0.32f);
        rect.anchorMax = new Vector2(0.72f, 0.54f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        VerticalLayoutGroup layout = root.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(8, 8, 4, 4);
        layout.spacing = 4f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.childControlHeight = true;
        layout.childControlWidth = true;

        ContentSizeFitter fitter = root.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        TMP_FontAsset font = _gameOverScoreText != null ? _gameOverScoreText.font : null;
        Color color = _gameOverScoreText != null ? _gameOverScoreText.color : Color.white;

        IReadOnlyList<AchievementDefinition> unlocked =
            AchievementManager.Instance != null ? AchievementManager.Instance.UnlockedThisRun : null;

        string heading = unlocked != null && unlocked.Count > 0 ? "Achievements unlocked" : "No new achievements";
        CreateGameOverLabel(root.transform, heading, 28f, font, color, FontStyles.Bold);

        if (unlocked != null)
        {
            foreach (AchievementDefinition definition in unlocked)
                CreateGameOverLabel(root.transform, definition.Title, 24f, font, color, FontStyles.Normal);
        }

        LocalizationManager.Instance?.RefreshAll();
    }

    static Transform FindGameOverPanel()
    {
        Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform transform in transforms)
        {
            if (transform.name == "Panel_GameOver")
                return transform;
        }

        return null;
    }

    static void CreateGameOverLabel(Transform parent, string source, float size, TMP_FontAsset font, Color color, FontStyles style)
    {
        GameObject textObject = new("Label", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI label = textObject.GetComponent<TextMeshProUGUI>();
        label.text = source;
        label.fontSize = size;
        label.alignment = TextAlignmentOptions.Center;
        label.enableWordWrapping = true;
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.fontStyle = style;
        label.color = color;
        label.raycastTarget = false;
        if (font != null)
            label.font = font;
        textObject.GetComponent<LayoutElement>().preferredHeight = size + 8f;
    }

    private string GetPlayerName()
    {
        string playerName = PlayerPrefs.GetString(_playerNameKey, _defaultPlayerName);

        if (string.IsNullOrWhiteSpace(playerName))
            playerName = _defaultPlayerName;

        return playerName.Trim();
    }

    public static void SetPlayerName(string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
            playerName = "Anonymous";

        PlayerPrefs.SetString("PlayerName", playerName.Trim());
        PlayerPrefs.Save();
        Debug.Log($"<color=green>[LeaderboardUI] Player name saved: {playerName}</color>");
    }

    public static string GetCurrentPlayerName()
    {
        string playerName = PlayerPrefs.GetString("PlayerName", "Anonymous");
        return string.IsNullOrWhiteSpace(playerName) ? "Anonymous" : playerName.Trim();
    }
}