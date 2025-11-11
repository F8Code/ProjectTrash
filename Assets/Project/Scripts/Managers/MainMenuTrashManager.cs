using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MainMenuTrashManager : MonoBehaviour
{
    public static MainMenuTrashManager Instance { get; private set; }

    [Header("Path settings")]
    [Tooltip("Path to Assets/Resources/ folders that contain TrashData scriptible object instances")]
    [SerializeField]
    List<string> _trashDataFolders = new() { "TrashData/Plastic", "TrashData/Paper", "TrashData/Metal", "TrashData/Glass", "TrashData/Hazard", "TrashData/Mixed" };

    [Header("References")]
    [Tooltip("A trash prefab with all components required for processing TrashData scriptible object information")]
    [SerializeField] Trash _trashPrefab;
    [Tooltip("Position at which new trash is spawned")]
    [SerializeField] Transform _trashSpawnPosition;
    [Tooltip("Trash conveyors that move trash from A to B")]
    [SerializeField] List<TrashConveyor> _trashConveyors = new();

    [Header("Trash spawn settings")]
    [Tooltip("The final trash spawning stage that plays when all others have finished")]
    [SerializeField] TrashSpawnLastStage _lastSpawnStage;

    HashSet<TrashData> _trashDatas = new();
    HashSet<Trash> _trash = new();
    HashSet<Trash> _trashToDespawn = new();
    TrashFactory _trashFactory;

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
            foreach (TrashData data in datas)
                _trashDatas.Add(data);
        }
    }

    void Start()
    {
        _trashFactory = new(_trashPrefab, 10, transform);
        SpawnTrash();
    }

    public void Update()
    {
        ProgressTrashSpawning();
        MoveConveyors();
        DespawnTrash();
    }

    void ProgressTrashSpawning()
    {
        _spawnTimer += Time.deltaTime;
        if (_spawnTimer >= _lastSpawnStage.SpawnDelaySeconds)
        {
            _spawnTimer = 0f;
            SpawnTrash();
        }
            
        _lastStageDelayReduceTimer += Time.deltaTime;
        if (_lastStageDelayReduceTimer >= _lastSpawnStage.TimeToReduceSpawnDelaySeconds)
        {
            _lastStageDelayReduceTimer = 0f;
            _lastSpawnStage.SpawnDelaySeconds = Mathf.Max(_lastSpawnStage.MinimumSpawnDelaySeconds, _lastSpawnStage.SpawnDelaySeconds - _lastSpawnStage.SpawnDelayReductionSeconds);
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

            foreach (TrashConveyor conveyor in _trashConveyors)
                conveyor.StopIgnoringTrash(trash);
        }

        _trashToDespawn.Clear();
    }

    void OnDisable()
    {
        foreach (Trash trash in _trash)
            trash.OnTrashCollected -= CollectTrash;
    }
}