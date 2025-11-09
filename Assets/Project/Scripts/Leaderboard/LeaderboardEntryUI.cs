using TMPro;
using UnityEngine;

public class LeaderboardEntryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _rankText;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _scoreText;

    /// <summary>
    /// Sets the data for this leaderboard entry
    /// </summary>
    public void SetData(int rank, string playerName, int score)
    {
        if (_rankText != null)
            _rankText.text = rank.ToString();

        if (_nameText != null)
            _nameText.text = playerName;

        if (_scoreText != null)
            _scoreText.text = score.ToString();
    }
}