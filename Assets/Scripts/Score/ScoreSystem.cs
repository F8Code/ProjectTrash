using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ScoreSystem
{
    [Header("Score settings")]
    [Tooltip("Score multipliers applied when combo thresholds are reached")]
    [SerializeField] float[] _scoreMultiplierLevels = { 1.25f, 1.5f, 1.75f, 2f };
    [Tooltip("Minimum combo count required to activate the score multiplier")]
    [SerializeField, Range(1, 5)] uint _scoreMultiplierRequiredComboInclusiveSeconds = 3;
    [Tooltip("How long the multiplier stays active for after a successful combo in seconds")]
    [SerializeField, Range(0.1f, 10f)] float _scoreMultiplierDuration = 5f;
    [Tooltip("If enabled, logs score info to the console every frame")]
    [SerializeField] bool _displayScoreInformation = false;

    [Header("Mistakes settings")]
    [Tooltip("How many mistakes are allowed before the game ends")]
    [SerializeField, Range(1, 10)] uint _mistakesAllowed = 5;

    [Header("Audio Settings")]
    [Tooltip("Sound played when sorting correctly")]
    public AudioClip SuccessSound;
    [Tooltip("Sound played when making a mistake")]
    public AudioClip MistakeSound;
    [Tooltip("Correct sort volume multiplier")]
    [SerializeField, Range(0f, 1f)] private float _successVolume = 0.75f;
    [Tooltip("Mistake sound volume multiplier")]
    [SerializeField, Range(0f, 1f)] private float _mistakeVolume = 0.75f;
    [Tooltip("Score multiplier pitch strength multiplier")]
    [SerializeField, Range(0f, 1f)] private float _multiplierPitchStrength = 1f;

    uint _score = 0;
    float _scoreMultiplier = 0f;
    float _multiplierDurationLeft = 0f;
    List<int> _scores = new();

    public int Score => (int)_score;
    public float ScoreMultiplier => _scoreMultiplier;
    public float ScoreMultiplierRemainingDuration => _multiplierDurationLeft;
    public float ScoreCurrentlyMultiplied => GetCurrentlyMultipliedScore();
    public int LivesRemaining => (int)_mistakesAllowed;

    public void CustomUpdate()
    {
        if (_displayScoreInformation)
            Debug.Log("Lives: " + LivesRemaining + ", Score: " + Score + ", Multiplier: " + ScoreMultiplier + ", RemaingDuration: " + ScoreMultiplierRemainingDuration + ", PendingScore: " + ScoreCurrentlyMultiplied);
            
        if (_multiplierDurationLeft <= 0f)
            return;

        _multiplierDurationLeft -= Time.deltaTime;
        if(_multiplierDurationLeft <= 0f)
        {
            _score += GetCurrentlyMultipliedScore();
            _scoreMultiplier = _multiplierDurationLeft = 0f;
            _scores.Clear();
        }
    }

    public void AddScore(int score)
    {
        if (score >= 0) //Score!
        {
            _scores.Add(score);
            _multiplierDurationLeft = _scoreMultiplierDuration;
            if (GetCurrentlyMultipliedScore() == 0)
                _score += (uint)score;

            float audioPitch01 = Mathf.InverseLerp(_scoreMultiplierLevels[_scoreMultiplierLevels.Length - 1], 0f, _scoreMultiplier) * _multiplierPitchStrength;
            AudioManager.Instance.PlayAudio(SuccessSound, _successVolume, AudioPlaybackContext.PlaybackPriority.Medium, GameManager.Instance.transform.position, false, audioPitch01);
        }
        else //Mistake
        {
            _score += GetCurrentlyMultipliedScore();
            _scores.Clear();
            _scoreMultiplier = 0f;
            if (--_mistakesAllowed == 0)
                GameManager.Instance.EndGame();

            AudioManager.Instance.PlayAudio(MistakeSound, _mistakeVolume, AudioPlaybackContext.PlaybackPriority.Medium, GameManager.Instance.transform.position);
        }
    }
    
    uint GetCurrentlyMultipliedScore()
    {
        if (_scores.Count < _scoreMultiplierRequiredComboInclusiveSeconds) 
            return 0;

        int pendingScore = 0;
        for (int i = (int)_scoreMultiplierRequiredComboInclusiveSeconds - 1; i < _scores.Count; i++)
            pendingScore += _scores[i];

        _scoreMultiplier = _scoreMultiplierLevels[(int)Mathf.Min(_scores.Count - _scoreMultiplierRequiredComboInclusiveSeconds, _scoreMultiplierLevels.Length - 1)];
        return (uint)(pendingScore * _scoreMultiplier);
    }
}
