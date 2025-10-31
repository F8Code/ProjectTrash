using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager I;

    [Header("Score")]
    public int TotalScore { get; private set; }

    [Header("Countdown & Multiplier")]
    [SerializeField] private float countdownDuration = 5f;    
    [SerializeField] private int startMultiplierAtStreak = 3;

    [Header("UI (opsiyonel—birini ata)")]
    [SerializeField] private TMP_Text scoreTMP;
    [SerializeField] private Text scoreText;
    [Space(4)]
    [SerializeField] private TMP_Text countdownTMP;
    [SerializeField] private Text countdownText;
    [Space(4)]
    [SerializeField] private TMP_Text multiplierTMP;
    [SerializeField] private Text multiplierText;

    private readonly HashSet<int> countedGlobal = new HashSet<int>();

    private int streak;
    private bool countdownActive;
    private float countdownRemaining;
    private Coroutine countdownCo;

    private bool multiplierActive;
    private int multiItems;            
    private int sumTotalsInMultiplier;  

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
    }

    void Start()
    {
        RefreshUI();
        UpdateCountdownUI();
        UpdateMultiplierUI();
    }

   
    public bool HasCounted(int id) => countedGlobal.Contains(id);
    public void MarkCounted(int id) => countedGlobal.Add(id);


    public void RegisterCorrectDeposit(int itemBaseTotal)
    {
        streak++;
        RestartCountdown();

        if (!multiplierActive && streak >= startMultiplierAtStreak)
        {
            multiplierActive = true;
            multiItems = 0;
            sumTotalsInMultiplier = 0;
        }

        if (!multiplierActive)
        {
            AddScore(itemBaseTotal);
        }
        else
        {
            int prevN = multiItems;
            int prevS = sumTotalsInMultiplier;

            multiItems = prevN + 1;
            sumTotalsInMultiplier = prevS + itemBaseTotal;

          
            int delta = (sumTotalsInMultiplier * multiItems) - (prevS * prevN);
            AddScore(delta);
        }
    }

    public void RegisterWrongDeposit()
    {
        StopCountdownAndStreak();
    }

    private void AddScore(int amount)
    {
        TotalScore += amount;
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (scoreTMP) scoreTMP.text = $"Score: {TotalScore}";
        if (scoreText) scoreText.text = $"Score: {TotalScore}";
    }

    private void RestartCountdown()
    {
        countdownRemaining = countdownDuration;
        countdownActive = true;

        if (countdownCo != null) StopCoroutine(countdownCo);
        countdownCo = StartCoroutine(CountdownRoutine());

        UpdateCountdownUI();
        UpdateMultiplierUI();
    }

    private IEnumerator CountdownRoutine()
    {
        while (countdownRemaining > 0f)
        {
            countdownRemaining -= Time.deltaTime;
            UpdateCountdownUI();
            yield return null;
        }
        StopCountdownAndStreak();
    }

    private void StopCountdownAndStreak()
    {
        countdownActive = false;
        countdownRemaining = 0f;
        if (countdownCo != null) StopCoroutine(countdownCo);
        countdownCo = null;

        streak = 0;
        multiplierActive = false;
        multiItems = 0;
        sumTotalsInMultiplier = 0;

        UpdateCountdownUI();
        UpdateMultiplierUI();
    }

    private void UpdateCountdownUI()
    {
        string msg = countdownActive ? $"{countdownRemaining:0.0}s" : "-";
        if (countdownTMP) countdownTMP.text = $"Combo Time: {msg}";
        if (countdownText) countdownText.text = $"Combo Time: {msg}";
    }

    private void UpdateMultiplierUI()
    {
        string msg = multiplierActive ? $"ON (streak {streak}, items {multiItems})" : "OFF";
        if (multiplierTMP) multiplierTMP.text = $"Multiplier: {msg}";
        if (multiplierText) multiplierText.text = $"Multiplier: {msg}";
    }
}
