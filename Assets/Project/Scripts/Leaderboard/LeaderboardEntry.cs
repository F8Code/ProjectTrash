using System;
using UnityEngine.ProBuilder.MeshOperations;

[Serializable]
public class LeaderboardEntry : IComparable<LeaderboardEntry>
{
    public string PlayerName;
    public int Score;
    public string Timestamp;

    public LeaderboardEntry(string playerName, int score, string timestamp = "")
    {
        PlayerName = playerName;
        Score = score;
        Timestamp = string.IsNullOrWhiteSpace(timestamp) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : timestamp;
    }

    // Compare by score descending (higher scores first)
    public int CompareTo(LeaderboardEntry other)
    {
        if (other == null)
            return 1;

        return other.Score.CompareTo(this.Score);
    }
}