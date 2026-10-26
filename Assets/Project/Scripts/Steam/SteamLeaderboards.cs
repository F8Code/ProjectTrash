using System;
using System.Collections.Generic;
using UnityEngine;

public class SteamLeaderboards : MonoBehaviour
{
    public const string ApiName = "HighScore";
    public const int VisibleEntries = 15;

    public static SteamLeaderboards Instance { get; private set; }
    public static event Action EntriesUpdated;

    readonly List<LeaderboardEntry> _worldEntries = new();
    int _pendingScore = -1;
    bool _downloading;
    string _status = "Steam is offline";

#if STEAMWORKS_NET
    Steamworks.SteamLeaderboard_t _handle;
    bool _hasHandle;
    bool _findRequested;
    Steamworks.CallResult<Steamworks.LeaderboardFindResult_t> _findResult;
    Steamworks.CallResult<Steamworks.LeaderboardScoreUploaded_t> _uploadResult;
    Steamworks.CallResult<Steamworks.LeaderboardScoresDownloaded_t> _downloadResult;
#endif

    public static IReadOnlyList<LeaderboardEntry> WorldEntries =>
        Instance != null ? Instance._worldEntries : Array.Empty<LeaderboardEntry>();

    public static bool IsDownloading => Instance != null && Instance._downloading;

    public static string Status => Instance != null ? Instance._status : "Steam is offline";

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        TryFindLeaderboard();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public static void UploadScore(int score)
    {
        if (score < 0)
            return;

        if (Instance == null)
            return;

        Instance.UploadInternal(score);
    }

    public static void RequestWorldEntries()
    {
        Instance?.DownloadInternal();
    }

    void UploadInternal(int score)
    {
        if (!SteamManager.Initialized)
        {
            _pendingScore = Mathf.Max(_pendingScore, score);
            _status = "Steam is offline";
            return;
        }

#if STEAMWORKS_NET
        _pendingScore = Mathf.Max(_pendingScore, score);
        TryFindLeaderboard();
        if (!_hasHandle)
            return;

        Steamworks.SteamAPICall_t call = Steamworks.SteamUserStats.UploadLeaderboardScore(
            _handle,
            Steamworks.ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest,
            _pendingScore,
            null,
            0);
        _uploadResult ??= Steamworks.CallResult<Steamworks.LeaderboardScoreUploaded_t>.Create();
        _uploadResult.Set(call, OnScoreUploaded);
#else
        _pendingScore = Mathf.Max(_pendingScore, score);
        _status = "Steam is offline";
#endif
    }

    void DownloadInternal()
    {
        if (!SteamManager.Initialized)
        {
            _status = "Steam is offline";
            _worldEntries.Clear();
            _downloading = false;
            EntriesUpdated?.Invoke();
            return;
        }

#if STEAMWORKS_NET
        TryFindLeaderboard();
        if (!_hasHandle)
        {
            _status = "Connecting to Steam...";
            _downloading = true;
            EntriesUpdated?.Invoke();
            return;
        }

        _downloading = true;
        _status = "Loading...";
        Steamworks.SteamAPICall_t call = Steamworks.SteamUserStats.DownloadLeaderboardEntries(
            _handle,
            Steamworks.ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal,
            1,
            VisibleEntries);
        _downloadResult ??= Steamworks.CallResult<Steamworks.LeaderboardScoresDownloaded_t>.Create();
        _downloadResult.Set(call, OnScoresDownloaded);
        EntriesUpdated?.Invoke();
#else
        _status = "Steam is offline";
        _worldEntries.Clear();
        _downloading = false;
        EntriesUpdated?.Invoke();
#endif
    }

    void TryFindLeaderboard()
    {
#if STEAMWORKS_NET
        if (_hasHandle || _findRequested || !SteamManager.Initialized)
            return;

        _findRequested = true;
        _status = "Connecting to Steam...";
        Steamworks.SteamAPICall_t call = Steamworks.SteamUserStats.FindOrCreateLeaderboard(
            ApiName,
            Steamworks.ELeaderboardSortMethod.k_ELeaderboardSortMethodDescending,
            Steamworks.ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric);
        _findResult ??= Steamworks.CallResult<Steamworks.LeaderboardFindResult_t>.Create();
        _findResult.Set(call, OnLeaderboardFound);
#endif
    }

#if STEAMWORKS_NET
    void OnLeaderboardFound(Steamworks.LeaderboardFindResult_t result, bool ioFailure)
    {
        if (ioFailure || result.m_bLeaderboardFound == 0)
        {
            _findRequested = false;
            _status = "Steam is offline";
            _downloading = false;
            EntriesUpdated?.Invoke();
            return;
        }

        _handle = result.m_hSteamLeaderboard;
        _hasHandle = true;
        _status = string.Empty;

        if (_pendingScore >= 0)
            UploadInternal(_pendingScore);

        if (_downloading)
            DownloadInternal();
        else
            EntriesUpdated?.Invoke();
    }

    void OnScoreUploaded(Steamworks.LeaderboardScoreUploaded_t result, bool ioFailure)
    {
        if (ioFailure || result.m_bSuccess == 0)
        {
            Debug.LogWarning("[SteamLeaderboards] Upload failed.");
            return;
        }

        _pendingScore = -1;
        Debug.Log($"[SteamLeaderboards] Uploaded score {result.m_nScore}, rank {result.m_nGlobalRankNew}.");
        DownloadInternal();
    }

    void OnScoresDownloaded(Steamworks.LeaderboardScoresDownloaded_t result, bool ioFailure)
    {
        _downloading = false;
        _worldEntries.Clear();

        if (ioFailure)
        {
            _status = "Steam is offline";
            EntriesUpdated?.Invoke();
            return;
        }

        int count = Mathf.Min(result.m_cEntryCount, VisibleEntries);
        for (int i = 0; i < count; i++)
        {
            if (!Steamworks.SteamUserStats.GetDownloadedLeaderboardEntry(
                    result.m_hSteamLeaderboardEntries,
                    i,
                    out Steamworks.LeaderboardEntry_t entry,
                    null,
                    0))
                continue;

            string playerName = Steamworks.SteamFriends.GetFriendPersonaName(entry.m_steamIDUser);
            if (string.IsNullOrWhiteSpace(playerName) || playerName == "[unknown]")
            {
                Steamworks.SteamFriends.RequestUserInformation(entry.m_steamIDUser, true);
                playerName = "Player";
            }

            _worldEntries.Add(new LeaderboardEntry(playerName, entry.m_nScore, rank: entry.m_nGlobalRank));
        }

        _status = _worldEntries.Count == 0 ? "No world scores yet" : string.Empty;
        EntriesUpdated?.Invoke();
    }
#endif
}
