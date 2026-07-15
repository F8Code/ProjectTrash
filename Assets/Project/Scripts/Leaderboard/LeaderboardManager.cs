using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    private const string LEADERBOARD_KEY = "LeaderboardData";
    private const int MAX_ENTRIES = 10;

    private List<LeaderboardEntry> _leaderboardEntries = new();

    public List<LeaderboardEntry> LeaderboardEntries => _leaderboardEntries;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Instance = this;

        LoadLeaderboard();
    }

    /// <summary>
    /// Loads the leaderboard from PlayerPrefs
    /// </summary>
    private void LoadLeaderboard()
    {
        if (PlayerPrefs.HasKey(LEADERBOARD_KEY))
        {
            string json = PlayerPrefs.GetString(LEADERBOARD_KEY);
            LeaderboardData data = JsonUtility.FromJson<LeaderboardData>(json);
            _leaderboardEntries = data.Entries ?? ResetLeaderboard();
            Debug.Log($"Leaderboard loaded: {_leaderboardEntries.Count} entries");
        }
        else
        {
            _leaderboardEntries = ResetLeaderboard();

            Debug.Log("No saved leaderboard found, starting fresh");
        }
    }

    /// <summary>
    /// Adds a new entry to the leaderboard
    /// </summary>
    public void AddEntry(string playerName, int score)
    {
        if (string.IsNullOrEmpty(playerName))
            playerName = "Anonymous";

        LeaderboardEntry newEntry = new(playerName, score);
        _leaderboardEntries.Add(newEntry);

        // Sort by score descending
        _leaderboardEntries.Sort();

        // Keep only top MAX_ENTRIES
        if (_leaderboardEntries.Count > MAX_ENTRIES)
            _leaderboardEntries = _leaderboardEntries.Take(MAX_ENTRIES).ToList();

        SaveLeaderboard();
        Debug.Log($"Leaderboard: Added entry for {playerName} with score {score}");
    }

    /// <summary>
    /// Removes a specific entry from the leaderboard
    /// </summary>
    public void RemoveEntry(string playerName, int score)
    {
        LeaderboardEntry entryToRemove = _leaderboardEntries.FirstOrDefault(e =>
          e.PlayerName == playerName && e.Score == score);

        if (entryToRemove != null)
        {
            _leaderboardEntries.Remove(entryToRemove);
            SaveLeaderboard();
            Debug.Log($"Leaderboard: Removed entry for {playerName} with score {score}");
        }
        else
        {
            Debug.LogWarning($"Leaderboard: Entry not found for {playerName} with score {score}");
        }
    }

    /// <summary>
    /// Updates an existing entry's name (finds by score and old name)
    /// </summary>
    public void UpdateEntryName(string oldName, int score, string newName)
    {
        LeaderboardEntry entry = _leaderboardEntries.FirstOrDefault(e =>
      e.PlayerName == oldName && e.Score == score);

        if (entry != null)
        {
            // Remove old entry and add new one to maintain sorting
            _leaderboardEntries.Remove(entry);
            AddEntry(newName, score);
            Debug.Log($"Leaderboard: Updated entry from {oldName} to {newName}");
        }
        else
        {
            Debug.LogWarning($"Leaderboard: Entry not found for {oldName} with score {score}");
        }
    }

    /// <summary>
    /// Saves the leaderboard to PlayerPrefs
    /// </summary>
    private void SaveLeaderboard()
    {
        LeaderboardData data = new() { Entries = _leaderboardEntries };
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(LEADERBOARD_KEY, json);
        PlayerPrefs.Save();
        Debug.Log("Leaderboard saved");
    }

    /// <summary>
    /// Clears all leaderboard data
    /// </summary>
    public void ClearLeaderboard()
    {
        _leaderboardEntries.Clear();
        PlayerPrefs.DeleteKey(LEADERBOARD_KEY);
        PlayerPrefs.Save();
        Debug.Log("Leaderboard cleared");
    }

    public List<LeaderboardEntry> ResetLeaderboard()
    {
        return new List<LeaderboardEntry>()
            {
                new LeaderboardEntry("Trashbubu", 999, "2026-05-31 19:11:04"),
                new LeaderboardEntry("Wall-I", 757, "2025-12-04 18:19:43"),
                new LeaderboardEntry("xXx_Pr0_R3cycl3r_xXx", 666, "2026-06-18 07:47:20"),
                new LeaderboardEntry("Trash Bandicoot", 484, "2026-04-13 20:35:47"),
                new LeaderboardEntry("Dumpster diver", 353, "2025-11-13 08:14:09"),
                new LeaderboardEntry("CyberScrap2077", 231, "2025-11-12 13:43:48"),
                new LeaderboardEntry("Trashman", 165, "2025-12-07 09:18:10"),
                new LeaderboardEntry("Klank", 117, "2026-01-24 21:51:13"),
                new LeaderboardEntry("Bepsiman", 59, "2026-01-30 08:33:24"),
                new LeaderboardEntry("6", 7, "2026-05-16 06:15:09")
            };
    }

    /// <summary>
    /// Gets the rank of a specific score (1-based)
    /// </summary>
    public int GetRankForScore(int score) => _leaderboardEntries.FindIndex(e => e.Score <= score) + 1;

    /// <summary>
    /// Checks if a score would make it to the leaderboard
    /// </summary>
    public bool IsHighScore(int score) => _leaderboardEntries.Count < MAX_ENTRIES || _leaderboardEntries.Any(e => score > e.Score);
}

[System.Serializable]
public class LeaderboardData
{
    public List<LeaderboardEntry> Entries;
}