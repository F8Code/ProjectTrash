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
        // Setup Game Over Panel button listeners
        if (_saveButton != null)
            _saveButton.onClick.AddListener(OnSaveButtonClicked);

        if (_viewLeaderboardButton != null)
            _viewLeaderboardButton.onClick.AddListener(OnViewLeaderboardClicked);

        if (_restartFromGameOverButton != null)
            _restartFromGameOverButton.onClick.AddListener(OnRestartClicked);

        if (_mainMenuFromGameOverButton != null)
            _mainMenuFromGameOverButton.onClick.AddListener(OnMainMenuClicked);

        // Setup Leaderboard Panel button listeners
        if (_restartFromLeaderboardButton != null)
            _restartFromLeaderboardButton.onClick.AddListener(OnRestartClicked);

        if (_backFromLeaderboardButton != null)
            _backFromLeaderboardButton.onClick.AddListener(OnBackFromLeaderboardClicked);

        // Validate references
        ValidateReferences();
    }

    private void Start()
    {
        // Hide both panels initially
        HideAllPanels();
    }

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

    private void ValidateReferences()
    {
        bool allValid = true;

        if (_gameOverPanel == null)
        {
            Debug.LogError("<color=red>[LeaderboardUI] Game Over Panel is not assigned!</color>", this);
            allValid = false;
        }

        if (_leaderboardPanel == null)
        {
            Debug.LogError("<color=red>[LeaderboardUI] Leaderboard Panel is not assigned!</color>", this);
            allValid = false;
        }

        if (_entriesContainer == null)
        {
            Debug.LogError("<color=red>[LeaderboardUI] Entries Container is not assigned!</color>", this);
            allValid = false;
        }

        if (_entryPrefab == null)
        {
            Debug.LogError("<color=red>[LeaderboardUI] Entry Prefab is not assigned!</color>", this);
            allValid = false;
        }

        if (allValid)
        {
            Debug.Log("<color=green>[LeaderboardUI] All references validated successfully</color>");
        }
    }

    /// <summary>
    /// Shows the Game Over panel with score - called when player loses
    /// </summary>
    public void ShowGameOver(int finalScore)
    {
        Debug.Log($"<color=cyan>[LeaderboardUI] ShowGameOver called with score: {finalScore}</color>");

        _currentScore = finalScore;

        // Only reset saved state if this is a new score (different from current)
        if (_currentScore != finalScore || !_scoreSaved)
        {
            _scoreSaved = false;
            _savedEntryName = "";
        }

        // Show Game Over panel
        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(true);
            Debug.Log("<color=green>[LeaderboardUI] Game Over panel activated</color>");
        }

        // Hide Leaderboard panel
        if (_leaderboardPanel != null)
        {
            _leaderboardPanel.SetActive(false);
        }

        // Ensure parent Canvas is active
        ActivateParentCanvas();

        // Notify CursorManager to show cursor
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.ShowCursorForUI();
        }

        // Display score
        if (_gameOverScoreText != null)
        {
            _gameOverScoreText.text = $"Score: {finalScore}";
        }

        // Load saved player name into input field
        if (_playerNameInput != null)
        {
            // If we already saved this score, show the saved name
            // Otherwise, load the default saved name
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

        // Hide Game Over panel
        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(false);
        }

        // Show Leaderboard panel
        if (_leaderboardPanel != null)
        {
            _leaderboardPanel.SetActive(true);
            Debug.Log("<color=green>[LeaderboardUI] Leaderboard panel activated</color>");
        }

        // Ensure parent Canvas is active
        ActivateParentCanvas();

        // Notify CursorManager to show cursor (if not already shown)
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.ShowCursorForUI();
        }

        // Refresh leaderboard display
        RefreshLeaderboardDisplay();
    }

    /// <summary>
    /// Hides all panels
    /// </summary>
    public void HideAllPanels()
    {
        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(false);
            Debug.Log("<color=yellow>[LeaderboardUI] Game Over panel hidden</color>");
        }

        if (_leaderboardPanel != null)
        {
            _leaderboardPanel.SetActive(false);
            Debug.Log("<color=yellow>[LeaderboardUI] Leaderboard panel hidden</color>");
        }

        // Notify CursorManager to hide cursor (twice to account for both panels)
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.HideCursorForUI();
            CursorManager.Instance.HideCursorForUI();
        }
    }

    private void ActivateParentCanvas()
    {
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null && !parentCanvas.gameObject.activeInHierarchy)
            parentCanvas.gameObject.SetActive(true);
    }

    private void UpdateSaveButtonState()
    {
        if (_saveButton != null)
        {
            // Always enable the button, but change text based on state
            _saveButton.interactable = true;

            // Update button text if it has a TextMeshProUGUI child
            TextMeshProUGUI buttonText = _saveButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = _scoreSaved ? "Update" : "Save";
        }
    }

    /// <summary>
    /// Called when Save button is clicked
    /// </summary>
    private void OnSaveButtonClicked()
    {
        string playerName = _playerNameInput != null ? _playerNameInput.text.Trim() : "";

        if (string.IsNullOrWhiteSpace(playerName))
            playerName = _defaultPlayerName;

        if (_scoreSaved && !string.IsNullOrEmpty(_savedEntryName))
        {
            // Update existing entry
            Debug.Log($"<color=yellow>[LeaderboardUI] Updating score: {_savedEntryName} → {playerName}</color>");

            if (LeaderboardManager.Instance != null)
            {
                // Remove old entry
                LeaderboardManager.Instance.RemoveEntry(_savedEntryName, _currentScore);

                // Add new entry with updated name
                LeaderboardManager.Instance.AddEntry(playerName, _currentScore);

                // Update saved name
                _savedEntryName = playerName;

                // Save player name for next time
                SetPlayerName(playerName);

                Debug.Log($"<color=green>[LeaderboardUI] Score updated successfully!</color>");
            }
        }
        else
        {
            // Add new entry
            Debug.Log($"<color=green>[LeaderboardUI] Saving score: {playerName} - {_currentScore}</color>");

            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.AddEntry(playerName, _currentScore);
                _scoreSaved = true;
                _savedEntryName = playerName;

                // Save player name for next time
                SetPlayerName(playerName);

                Debug.Log($"<color=green>[LeaderboardUI] Score saved successfully!</color>");
            }
        }

        UpdateSaveButtonState();
    }

    /// <summary>
    /// Called when View Leaderboard button is clicked from Game Over screen
    /// </summary>
    private void OnViewLeaderboardClicked()
    {
        Debug.Log("<color=cyan>[LeaderboardUI] View Leaderboard button clicked</color>");

        // If score hasn't been saved yet, save it automatically
        if (!_scoreSaved)
            OnSaveButtonClicked();

        // Show leaderboard
        ShowLeaderboard();
    }

    /// <summary>
    /// Called when Back button is clicked from Leaderboard screen
    /// </summary>
    private void OnBackFromLeaderboardClicked() => ShowGameOver(_currentScore);

    /// <summary>
    /// Called when Restart button is clicked
    /// </summary>
    private void OnRestartClicked()
    {
        Debug.Log("<color=cyan>[LeaderboardUI] Restart button clicked</color>");

        // Hide panels and reset cursor before loading scene
        HideAllPanels();

        UnityEngine.SceneManagement.SceneManager.LoadScene(GameConstants.Scene.Game);
    }

    /// <summary>
    /// Called when Main Menu button is clicked
    /// </summary>
    private void OnMainMenuClicked()
    {
        Debug.Log("<color=cyan>[LeaderboardUI] Main Menu button clicked</color>");

        // Hide panels and reset cursor before loading scene
        HideAllPanels();

        UnityEngine.SceneManagement.SceneManager.LoadScene(GameConstants.Scene.MainMenu);
    }

    private void RefreshLeaderboardDisplay()
    {
        Debug.Log("<color=cyan>[LeaderboardUI] Refreshing leaderboard display</color>");

        // Clear existing entries
        foreach (var entry in _spawnedEntries)
            Destroy(entry);

        _spawnedEntries.Clear();

        if (LeaderboardManager.Instance == null)
        {
            Debug.LogError("<color=red>[LeaderboardUI] LeaderboardManager.Instance is null!</color>");
            return;
        }

        // Get leaderboard data
        List<LeaderboardEntry> entries = LeaderboardManager.Instance.LeaderboardEntries;
        Debug.Log($"<color=cyan>[LeaderboardUI] Found {entries.Count} leaderboard entries</color>");

        // Spawn new entries
        for (int i = 0; i < Mathf.Min(entries.Count, _maxVisibleEntries); i++)
        {
            if (_entryPrefab != null && _entriesContainer != null)
            {
                GameObject entryObj = Instantiate(_entryPrefab, _entriesContainer);
                _spawnedEntries.Add(entryObj);

                // Set entry data
                if (entryObj.TryGetComponent<LeaderboardEntryUI>(out var entryUI))
                {
                    entryUI.SetData(i + 1, entries[i].PlayerName, entries[i].Score);
                    Debug.Log($"<color=green>[LeaderboardUI] Entry {i + 1}: {entries[i].PlayerName} - {entries[i].Score}</color>");
                }
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