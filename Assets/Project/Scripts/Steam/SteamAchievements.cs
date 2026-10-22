using UnityEngine;

public class SteamAchievements : MonoBehaviour
{
    static SteamAchievements _instance;
    bool _initialized;

#if STEAMWORKS_NET
    Steamworks.Callback<Steamworks.UserStatsReceived_t> _statsReceived;
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
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
        if (_initialized)
            Steamworks.SteamAPI.RunCallbacks();
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

        foreach (AchievementDefinition definition in manager.Definitions)
        {
            if (manager.IsUnlocked(definition.Id))
                _instance.UnlockInternal(definition.Id);
        }
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
            return;

        _statsReceived = Steamworks.Callback<Steamworks.UserStatsReceived_t>.Create(OnUserStatsReceived);
        //Steamworks.SteamUserStats.RequestCurrentStats();
#endif
    }

    void UnlockInternal(string id)
    {
#if STEAMWORKS_NET
        if (!_initialized)
            return;

        Steamworks.SteamUserStats.SetAchievement(id);
        Steamworks.SteamUserStats.StoreStats();
#endif
    }

#if STEAMWORKS_NET
    void OnUserStatsReceived(Steamworks.UserStatsReceived_t data)
    {
        if (AchievementManager.Instance == null)
            return;

        foreach (AchievementDefinition definition in AchievementManager.Instance.Definitions)
        {
            if (Steamworks.SteamUserStats.GetAchievement(definition.Id, out bool achieved) && achieved)
                AchievementManager.Instance.ImportUnlocked(definition.Id);
            else if (AchievementManager.Instance.IsUnlocked(definition.Id))
                UnlockInternal(definition.Id);
        }
    }
#endif
}
