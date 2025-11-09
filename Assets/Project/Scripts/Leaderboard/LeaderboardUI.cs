using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardUI : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject _gameOverPanel;

    [SerializeField] private GameObject _leaderboardPanel;

    [Header("Game Over Panel UI")]
    [SerializeField] private TextMeshProUGUI _gameOverScoreText;

    [SerializeField] private TMP_InputField _playerNameInput;
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _viewLeaderboardButton;
    [SerializeField] private Button _restartFromGameOverButton;
    [SerializeField] private Button _mainMenuFromGameOverButton;

    [Header("Leaderboard Panel UI")]
    [SerializeField] private Transform _entriesContainer;

    [SerializeField] private GameObject _entryPrefab;
    [SerializeField] private Button _restartFromLeaderboardButton;
    [SerializeField] private Button _backFromLeaderboardButton;

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

        if (_viewLeaderboardButton != null)
            _viewLeaderboardButton.onClick.AddListener(OnViewLeaderboardClicked);

        if (_restartFromGameOverButton != null)
            _restartFromGameOverButton.onClick.AddListener(OnRestartClicked);

        if (_mainMenuFromGameOverButton != null)
            _mainMenuFromGameOverButton.onClick.AddListener(OnMainMenuClicked);

        if (_restartFromLeaderboardButton != null)
            _restartFromLeaderboardButton.onClick.AddListener(OnRestartClicked);

        if (_backFromLeaderboardButton != null)
            _backFromLeaderboardButton.onClick.AddListener(OnBackFromLeaderboardClicked);
    }

    private void Start() => HideAllPanels();

    private void OnDestroy()
    {
        // Clean up listeners
        if (_saveButton != null)
            _saveButton.onClick.RemoveListener(OnSaveButtonClicked);
        if (_viewLeaderboardButton != null)
            _viewLeaderboardButton.onClick.RemoveListener(OnViewLeaderboardClicked);
        if (_restartFromGameOverButton != null)
            _restartFromGameOverButton.onClick.RemoveListener(OnRestartClicked);
        if (_mainMenuFromGameOverButton != null)
            _mainMenuFromGameOverButton.onClick.RemoveListener(OnMainMenuClicked);
        if (_restartFromLeaderboardButton != null)
            _restartFromLeaderboardButton.onClick.RemoveListener(OnRestartClicked);
        if (_backFromLeaderboardButton != null)
            _backFromLeaderboardButton.onClick.RemoveListener(OnBackFromLeaderboardClicked);
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
            _gameOverScoreText.text = $"Score: {finalScore}";

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
                buttonText.text = _scoreSaved ? "Update" : "Save";
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

        UpdateSaveButtonState();
    }

    private void OnViewLeaderboardClicked()
    {
        if (!_scoreSaved)
            OnSaveButtonClicked();

        ShowLeaderboard();
    }

    private void OnBackFromLeaderboardClicked() => ShowGameOver(_currentScore);

    private void OnRestartClicked()
    {
        Debug.Log("<color=cyan>[LeaderboardUI] Restart button clicked</color>");
        HideAllPanels();
        UnityEngine.SceneManagement.SceneManager.LoadScene(GameConstants.Scene.Game);
    }

    private void OnMainMenuClicked()
    {
        Debug.Log("<color=cyan>[LeaderboardUI] Main Menu button clicked</color>");
        HideAllPanels();
        UnityEngine.SceneManagement.SceneManager.LoadScene(GameConstants.Scene.MainMenu);
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