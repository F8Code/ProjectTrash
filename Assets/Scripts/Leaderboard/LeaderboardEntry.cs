using System;

[Serializable]
public class LeaderboardEntry : IComparable<LeaderboardEntry>
{
    public string PlayerName;
    public int Score;
    public string Timestamp;

    public LeaderboardEntry(string playerName, int score)
    {
        PlayerName = playerName;
        Score = score;
        Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    // Compare by score descending (higher scores first)
    public int CompareTo(LeaderboardEntry other)
    {
        if (other == null)
            return 1;

        return other.Score.CompareTo(this.Score);
    }
}