using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class AchievementDefinition
{
    public string Id;
    public string Title;
    public string Description;
    public Sprite Icon;
}

[Serializable]
public class AchievementSaveData
{
    public List<string> UnlockedIds = new();
    public List<string> CorrectlySortedItemNames = new();
}

public class AchievementManager : MonoBehaviour
{
    public const string BeatTrashbubu = "beat_trashbubu";
    public const string StreakMaster = "streak_master";
    public const string TrashTheTrashbubu = "trash_the_trashbubu";
    public const string FlawlessShift = "flawless_shift";
    public const string Timeless = "timeless";
    public const string TheProfessional = "the_professional";

    const string SaveKey = "ProjectTrash.Achievements";

    public static AchievementManager Instance { get; private set; }

    static readonly string[] RequiredTrashFolders =
    {
        "TrashData/Plastic",
        "TrashData/Paper",
        "TrashData/Metal",
        "TrashData/Glass",
        "TrashData/Hazard",
        "TrashData/Mixed"
    };

    readonly HashSet<string> _unlockedIds = new();
    readonly HashSet<string> _correctlySortedItemNames = new();
    readonly List<AchievementDefinition> _definitions = new();
    readonly List<AchievementDefinition> _unlockedThisRun = new();
    HashSet<string> _requiredItemNames;
    bool _notifiedTimelessThisRun;

    public IReadOnlyList<AchievementDefinition> Definitions => _definitions;
    public IReadOnlyList<AchievementDefinition> UnlockedThisRun => _unlockedThisRun;
    public event Action<AchievementDefinition> AchievementUnlocked;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null)
            return;

        GameObject host = new("AchievementManager");
        host.AddComponent<AchievementManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildDefinitions();
        Load();
        SceneManager.sceneLoaded += OnSceneLoaded;
        SteamAchievements.PushLocalUnlocks(this);
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _notifiedTimelessThisRun = false;
        _correctlySortedItemNames.Clear();
        _unlockedThisRun.Clear();
    }

    void Update()
    {
        if (GameManager.Instance == null)
            return;

        if (!_notifiedTimelessThisRun &&
            GameManager.Instance.CurrentGamemode == GameManager.Gamemode.Timebased &&
            GameManager.Instance.GameTime >= 120f)
        {
            _notifiedTimelessThisRun = true;
            Unlock(Timeless);
        }
    }

    public bool IsUnlocked(string id) => _unlockedIds.Contains(id);

    public void NotifyScore(int delta, int streak, int totalScore)
    {
        if (delta >= 0)
        {
            if (totalScore > 999)
                Unlock(BeatTrashbubu);
            if (streak >= 30)
                Unlock(StreakMaster);
            if (streak >= 20 && GameManager.Instance != null && GameManager.Instance.CurrentGamemode == GameManager.Gamemode.Mistakebased)
                Unlock(FlawlessShift);
        }
    }

    public void NotifyTrashSortedInBin(Trash trash, bool sortedCorrectly)
    {
        if (trash == null || !sortedCorrectly)
            return;

        string name = trash.Name ?? "";
        bool isBubu = name.IndexOf("catbubu", StringComparison.OrdinalIgnoreCase) >= 0 ||
                      name.IndexOf("trashbubu", StringComparison.OrdinalIgnoreCase) >= 0 ||
                      name.IndexOf("bubu", StringComparison.OrdinalIgnoreCase) >= 0;
        if (isBubu && trash.Type == TrashType.Hazard)
            Unlock(TrashTheTrashbubu);

        if (!string.IsNullOrWhiteSpace(name))
        {
            _correctlySortedItemNames.Add(name);
            TryUnlockProfessional();
        }
    }

    public void NotifyGameEnded()
    {
        if (GameManager.Instance == null)
            return;

        if (GameManager.Instance.CurrentGamemode == GameManager.Gamemode.Tutorial)
            return;

        int totalScore = GameManager.Instance.ScoreSystem.Score + (int)GameManager.Instance.ScoreSystem.ScoreCurrentlyMultiplied;
        if (totalScore > 999)
            Unlock(BeatTrashbubu);

        TryUnlockProfessional();
    }

    void TryUnlockProfessional()
    {
        HashSet<string> required = GetRequiredItemNames();
        if (required.Count == 0)
            return;

        foreach (string itemName in required)
        {
            if (!_correctlySortedItemNames.Contains(itemName))
                return;
        }

        Unlock(TheProfessional);
    }

    HashSet<string> GetRequiredItemNames()
    {
        if (_requiredItemNames != null)
            return _requiredItemNames;

        _requiredItemNames = new HashSet<string>();
        foreach (string folder in RequiredTrashFolders)
        {
            TrashData[] items = Resources.LoadAll<TrashData>(folder);
            foreach (TrashData item in items)
            {
                if (item != null && !string.IsNullOrWhiteSpace(item.Name))
                    _requiredItemNames.Add(item.Name);
            }
        }

        return _requiredItemNames;
    }

    void Unlock(string id)
    {
        if (!_unlockedIds.Add(id))
            return;

        Save();
        SteamAchievements.Unlock(id);
        AchievementDefinition definition = _definitions.Find(item => item.Id == id);
        if (definition != null)
        {
            _unlockedThisRun.Add(definition);
            AchievementUnlocked?.Invoke(definition);
        }
    }

    public void ImportUnlocked(string id)
    {
        if (string.IsNullOrEmpty(id) || !_unlockedIds.Add(id))
            return;

        Save();
    }

    void BuildDefinitions()
    {
        _definitions.Clear();
        _definitions.Add(new AchievementDefinition
        {
            Id = BeatTrashbubu,
            Title = "Beat Trashbubu",
            Description = "Score more than 999 points in one run."
        });
        _definitions.Add(new AchievementDefinition
        {
            Id = StreakMaster,
            Title = "Streak Master",
            Description = "Sort 30 items correctly in a row."
        });
        _definitions.Add(new AchievementDefinition
        {
            Id = TrashTheTrashbubu,
            Title = "Trash the bubu",
            Description = "Throw a Trashbubu into a Hazardous bin."
        });
        _definitions.Add(new AchievementDefinition
        {
            Id = FlawlessShift,
            Title = "Flawless Shift",
            Description = "Get a 20-item streak in Play vs Mistakes."
        });
        _definitions.Add(new AchievementDefinition
        {
            Id = Timeless,
            Title = "Timeless",
            Description = "Survive more than 2 minutes in Play vs Time."
        });
        _definitions.Add(new AchievementDefinition
        {
            Id = TheProfessional,
            Title = "The Professional",
            Description = "In one run, sort every trash item into the correct bin at least once."
        });
    }

    void Load()
    {
        _unlockedIds.Clear();
        if (!PlayerPrefs.HasKey(SaveKey))
            return;

        AchievementSaveData data = JsonUtility.FromJson<AchievementSaveData>(PlayerPrefs.GetString(SaveKey));
        if (data?.UnlockedIds == null)
            return;

        foreach (string id in data.UnlockedIds)
            _unlockedIds.Add(id);

    }

    void Save()
    {
        AchievementSaveData data = new()
        {
            UnlockedIds = new List<string>(_unlockedIds)
        };
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }
}
