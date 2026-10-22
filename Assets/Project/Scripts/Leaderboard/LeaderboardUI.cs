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

    private void Awake()
    {
        if (_saveButton != null)
            _saveButton.onClick.AddListener(OnSaveButtonClicked);
    }

    private void Start()
    {
        HideAllPanels();

        // Refresh leaderboard display
        RefreshLeaderboardDisplay();
    }

    private void OnDestroy()
    {
        // Clean up listeners
        if (_saveButton != null)
            _saveButton.onClick.RemoveListener(OnSaveButtonClicked);
    }

    /// <summary>
    /// Shows the Game Over panel with score - called when player loses
    /// </summary>
    public void ShowGameOver(int finalScore)
    {
        Debug.Log($"<color=cyan>[LeaderboardUI] ShowGameOver called with score: {finalScore}</color>");

        _currentScore = finalScore;

        if (_currentScore != finalScore || !_scoreSaved)
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

        // Refresh leaderboard display
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
            Debug.Log($"<color=yellow>[LeaderboardUI] Updating score: {_savedEntryName} → {playerName}</color>");

            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.RemoveEntry(_savedEntryName, _currentScore);
                LeaderboardManager.Instance.AddEntry(playerName, _currentScore);
                _savedEntryName = playerName;
                SetPlayerName(playerName);

                Debug.Log($"<color=green>[LeaderboardUI] Score updated successfully!</color>");
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

    private void RefreshLeaderboardDisplay()
    {
        foreach (var entry in _spawnedEntries)
            Destroy(entry);

        _spawnedEntries.Clear();

        List<LeaderboardEntry> entries = LeaderboardManager.Instance.LeaderboardEntries;

        for (int i = 0; i < Mathf.Min(entries.Count, _maxVisibleEntries); i++)
        {
            if (_entryPrefab != null && _entriesContainer != null)
            {
                GameObject entryObj = Instantiate(_entryPrefab, _entriesContainer);
                _spawnedEntries.Add(entryObj);

                if (entryObj.TryGetComponent<LeaderboardEntryUI>(out var entryUI))
                    entryUI.SetData(i + 1, entries[i].PlayerName, entries[i].Score);
            }
        }

        Debug.Log($"<color=green>[LeaderboardUI] Spawned {_spawnedEntries.Count} entry objects</color>");
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