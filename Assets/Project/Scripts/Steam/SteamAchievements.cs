using System.Collections.Generic;
using UnityEngine;

public class SteamAchievements : MonoBehaviour
{
    static SteamAchievements _instance;
    bool _initialized;
    bool _needStore;
    float _storeRetryAt;
    readonly HashSet<string> _pendingUnlocks = new();

#if STEAMWORKS_NET
    Steamworks.Callback<Steamworks.UserStatsStored_t> _statsStored;
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (_instance != null)
            return;
#if UNITY_WEBGL
        return;
#else
        GameObject host = new("SteamAchievements");
        DontDestroyOnLoad(host);
        _instance = host.AddComponent<SteamAchievements>();
        host.AddComponent<SteamLeaderboards>();
#endif
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        TryInitialize();
    }

    void Update()
    {
#if STEAMWORKS_NET
        if (!_initialized)
            return;

        Steamworks.SteamAPI.RunCallbacks();
        if (_needStore && Time.unscaledTime >= _storeRetryAt)
            TryStoreStats();
#endif
    }

    void OnDestroy()
    {
#if STEAMWORKS_NET
        if (_initialized)
            Steamworks.SteamAPI.Shutdown();
#endif
        if (_instance == this)
            _instance = null;
    }

    public static bool IsReady => _instance != null && _instance._initialized;

    public static void Unlock(string id)
    {
        if (_instance == null || string.IsNullOrEmpty(id))
            return;

        _instance.UnlockInternal(id);
    }

    public static void PushLocalUnlocks(AchievementManager manager)
    {
        if (_instance == null || manager == null)
            return;

        _instance.SyncWith(manager);
    }

    void TryInitialize()
    {
        if (SteamAppId.Value == 0)
            return;

#if STEAMWORKS_NET
#if !UNITY_EDITOR
        if (Steamworks.SteamAPI.RestartAppIfNecessary(new Steamworks.AppId_t(SteamAppId.Value)))
        {
            Application.Quit();
            return;
        }
#endif
        try
        {
            _initialized = Steamworks.SteamAPI.Init();
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning($"[SteamAchievements] SteamAPI.Init failed: {exception.Message}");
            _initialized = false;
        }

        if (!_initialized)
        {
            Debug.LogWarning("[SteamAchievements] Steam is not running or App ID 4798760 could not initialize.");
            return;
        }

        _statsStored = Steamworks.Callback<Steamworks.UserStatsStored_t>.Create(OnUserStatsStored);
        SyncWith(AchievementManager.Instance);
        FlushPending();
#endif
    }

    void UnlockInternal(string id)
    {
        _pendingUnlocks.Add(id);

#if STEAMWORKS_NET
        if (!_initialized)
            return;

        if (!Steamworks.SteamUserStats.SetAchievement(id))
            Debug.LogWarning($"[SteamAchievements] SetAchievement failed for '{id}'.");

        TryStoreStats();
#endif
    }

    void SyncWith(AchievementManager manager)
    {
        if (manager == null)
            return;

#if STEAMWORKS_NET
        if (_initialized)
        {
            foreach (AchievementDefinition definition in manager.Definitions)
            {
                if (Steamworks.SteamUserStats.GetAchievement(definition.Id, out bool achieved) && achieved)
                    manager.ImportUnlocked(definition.Id);
            }
        }
#endif

        foreach (AchievementDefinition definition in manager.Definitions)
        {
            if (manager.IsUnlocked(definition.Id))
                UnlockInternal(definition.Id);
        }
    }

    void FlushPending()
    {
        if (_pendingUnlocks.Count == 0)
            return;

        foreach (string id in new List<string>(_pendingUnlocks))
            UnlockInternal(id);
    }

#if STEAMWORKS_NET
    void TryStoreStats()
    {
        _needStore = !Steamworks.SteamUserStats.StoreStats();
        _storeRetryAt = Time.unscaledTime + 2f;
    }

    void OnUserStatsStored(Steamworks.UserStatsStored_t data)
    {
        if (data.m_eResult == Steamworks.EResult.k_EResultOK)
        {
            _needStore = false;
            return;
        }

        _needStore = true;
        _storeRetryAt = Time.unscaledTime + 2f;
        Debug.LogWarning($"[SteamAchievements] StoreStats result {data.m_eResult}");
    }
#endif
}
