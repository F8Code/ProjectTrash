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

    readonly HashSet<string> _unlockedIds = new();
    readonly List<AchievementDefinition> _definitions = new();
    bool _hadMistakeThisRun;
    bool _hadCorrectSortThisRun;
    bool _notifiedTimelessThisRun;

    public IReadOnlyList<AchievementDefinition> Definitions => _definitions;
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
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _hadMistakeThisRun = false;
        _hadCorrectSortThisRun = false;
        _notifiedTimelessThisRun = false;
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
            _hadCorrectSortThisRun = true;
            if (totalScore > 999)
                Unlock(BeatTrashbubu);
            if (streak >= 20)
                Unlock(StreakMaster);
            if (streak >= 10 && GameManager.Instance != null && GameManager.Instance.CurrentGamemode == GameManager.Gamemode.Mistakebased)
                Unlock(FlawlessShift);
        }
        else
        {
            _hadMistakeThisRun = true;
        }
    }

    public void NotifyTrashSortedInBin(Trash trash)
    {
        if (trash == null)
            return;

        string name = trash.Name ?? "";
        if (name.IndexOf("catbubu", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("trashbubu", StringComparison.OrdinalIgnoreCase) >= 0)
            Unlock(TrashTheTrashbubu);
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

        if (!_hadMistakeThisRun && _hadCorrectSortThisRun)
            Unlock(TheProfessional);
    }

    void Unlock(string id)
    {
        if (!_unlockedIds.Add(id))
            return;

        Save();
        AchievementDefinition definition = _definitions.Find(item => item.Id == id);
        if (definition != null)
            AchievementUnlocked?.Invoke(definition);
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
            Description = "Sort 20 items correctly in a row."
        });
        _definitions.Add(new AchievementDefinition
        {
            Id = TrashTheTrashbubu,
            Title = "Trash the Trashbubu",
            Description = "Throw a Trashbubu into a bin."
        });
        _definitions.Add(new AchievementDefinition
        {
            Id = FlawlessShift,
            Title = "Flawless Shift",
            Description = "Get a 10-item streak in Play vs Mistakes."
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
            Description = "Finish a run with no mistakes."
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
        AchievementSaveData data = new() { UnlockedIds = new List<string>(_unlockedIds) };
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }
}
