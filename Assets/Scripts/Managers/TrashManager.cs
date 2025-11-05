using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class TrashSpawnStage
{
    [Tooltip("Duration of the trash spawn stage")]
    [Range(1f, 60f)]
    public float DurationSeconds = 15f;
    [Tooltip("Time it takes for a new trash item to spawn")]
    [Range(0.1f, 10f)]
    public float SpawnDelaySeconds = 3f;
}

[System.Serializable]
public class TrashSpawnLastStage
{
    [Tooltip("Time it takes for a new trash item to spawn")]
    [Range(0.1f, 10f)]
    public float SpawnDelaySeconds = 2f;
    [Tooltip("Time it takes for the spawn delay to get shorter")]
    [Range(1f, 60f)]
    public float TimeToReduceSpawnDelaySeconds = 30f;
    [Tooltip("Spawn delay reduction that's applied when the spawn delay reduction timer reaches zero")]
    [Range(0.01f, 1f)]
    public float SpawnDelayReductionSeconds = 0.1f;
    [Tooltip("Spawn delay reduction that's applied when the spawn delay reduction timer reaches zero")]
    [Range(0.1f, 2f)]
    public float MinimumSpawnDelaySeconds = 0.2f;
}

public class TrashManager : MonoBehaviour
{
    public static TrashManager Instance { get; private set; }

    [Header("Path settings")]
    [Tooltip("Path to Assets/Resources/ folders that contain TrashData scriptible object instances")]
    [SerializeField]
    List<string> _trashDataFolders = new() { "TrashData/Plastic", "TrashData/Paper", "TrashData/Metal", "TrashData/Glass", "TrashData/Hazard", "TrashData/Mixed" };

    [Header("References")]
    [Tooltip("A trash prefab with all components required for processing TrashData scriptible object information")]
    [SerializeField] Trash _trashPrefab;
    [Tooltip("Position at which new trash is spawned")]
    [SerializeField] Transform _trashSpawnPosition;
    [Tooltip("Trash cans that award points when correct trash is thrown in them")]
    [SerializeField] List<TrashCan> _trashCans = new();
    [Tooltip("Trash conveyors that move trash from A to B")]
    [SerializeField] List<TrashConveyor> _trashConveyors = new();

    [Header("Trash spawn settings")]
    [Tooltip("Ammount of trash that needs to be despawned for the tutorial stage to finish")]
    [SerializeField] uint _trashToEndTutorial = 3;
    [Tooltip("Trash spawning stages that are processed one after another depending on their duration")]
    [SerializeField] List<TrashSpawnStage> _trashSpawnStages = new();
    [Tooltip("The final trash spawning stage that plays when all others have finished")]
    [SerializeField] TrashSpawnLastStage _lastSpawnStage;

    HashSet<TrashData> _trashDatas = new();
    HashSet<Trash> _trash = new();
    HashSet<Trash> _trashToDespawn = new();
    TrashFactory _trashFactory;

    //Tutorial stage related
    uint _trashDespawned = 0;

    bool _isTutorial => GameManager.Instance.CurrentState is GameTutorialState || (GameManager.Instance.CurrentState is GamePausedState && GameManager.Instance.PreviousState is GameTutorialState);

    int _currentSpawnStageIndex = 0;
    float _spawnTimer = 0f, _lastStageDelayReduceTimer = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        foreach (string folder in _trashDataFolders)
        {
            TrashData[] datas = Resources.LoadAll<TrashData>(folder);
            Debug.Log($"TrashManager: Loaded {datas.Length} TrashData scriptible objects from " + folder);
            foreach (var data in datas)
                _trashDatas.Add(data);
        }
    }

    void OnEnable()
    {
        foreach (TrashCan trashCan in _trashCans)
            trashCan.OnTrashCollected += CollectTrash;
    }

    void Start()
    {
        _trashFactory = new(_trashPrefab, 30, transform);
        SpawnTrash();
    }

    public void CustomUpdate()
    {
        ProgressTrashSpawning();
        MoveConveyors();
        DespawnTrash();
    }

    void ProgressTrashSpawning()
    {
        if (_isTutorial)
            return;

        _spawnTimer += Time.deltaTime;
        if (_spawnTimer >= (_currentSpawnStageIndex < _trashSpawnStages.Count ? _trashSpawnStages[_currentSpawnStageIndex].SpawnDelaySeconds : _lastSpawnStage.SpawnDelaySeconds))
        {
            _spawnTimer = 0f;
            SpawnTrash();
        }
            
        if (_currentSpawnStageIndex < _trashSpawnStages.Count) //Normal stage
        {
            _trashSpawnStages[_currentSpawnStageIndex].DurationSeconds -= Time.deltaTime;
            if (_trashSpawnStages[_currentSpawnStageIndex].DurationSeconds <= 0f)
                _currentSpawnStageIndex++;
        }
        else //Final stage
        {
            _lastStageDelayReduceTimer += Time.deltaTime;
            if (_lastStageDelayReduceTimer >= _lastSpawnStage.TimeToReduceSpawnDelaySeconds)
            {
                _lastStageDelayReduceTimer = 0f;
                _lastSpawnStage.SpawnDelaySeconds = Mathf.Max(_lastSpawnStage.MinimumSpawnDelaySeconds, _lastSpawnStage.SpawnDelaySeconds - _lastSpawnStage.SpawnDelayReductionSeconds);
            }
        }
    }

    void SpawnTrash()
    {
        if (!_trashDatas.Any())
            return;

        TrashData trashData = _trashDatas.RandomElement();
        Trash trash = _trashFactory.SpawnTrash(trashData, _trashSpawnPosition.position);
        trash.OnTrashCollected += CollectTrash;
        _trash.Add(trash);
    }

    void MoveConveyors()
    {
        foreach (TrashConveyor conveyor in _trashConveyors)
            conveyor.CustomUpdate();
    }

    void CollectTrash(Trash trash, int score)
    {
        GameManager.Instance.ModifyScore(score);
        MarkTrashForDespawn(trash);
    }

    void MarkTrashForDespawn(Trash trash)
    {
        _trashToDespawn.Add(trash);
    }

    void DespawnTrash()
    {
        foreach (Trash trash in _trashToDespawn)
        {
            _trash.Remove(trash);
            _trashFactory.DespawnTrash(trash);
            trash.OnTrashCollected -= CollectTrash;

            _trashDespawned++;
            if(_isTutorial)
                ProgressTutorial();
        }

        _trashToDespawn.Clear();
    }

    void ProgressTutorial()
    {
        if (_trashDespawned >= _trashToEndTutorial)
            GameManager.Instance.EndTutorialStage();
        else
            SpawnTrash();
    }

    void OnDisable()
    {
        foreach (TrashCan trashCan in _trashCans)
            trashCan.OnTrashCollected -= CollectTrash;

        foreach (Trash trash in _trash)
            trash.OnTrashCollected -= CollectTrash;
    }
}