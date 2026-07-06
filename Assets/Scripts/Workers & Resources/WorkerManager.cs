using System.Collections.Generic;
using UnityEngine;

public class WorkerManager : MonoBehaviour
{
    [Header("Team")]
    [SerializeField] private Team team;

    [Header("Worker Setup")]
    [SerializeField] private WorkerUnit workerPrefab;
    [SerializeField] private Transform workerSpawnPoint;
    [SerializeField] private Transform dropOffPoint;
    [SerializeField] private GoldVein goldVein;

    [Header("Worker Counts")]
    [SerializeField] private int startingWorkers = 1;
    [SerializeField] private int maxWorkers = 5;

    [Header("Resources")]
    [SerializeField] private int startingGold = 0;

    private GameManager gameManager;

    private readonly List<WorkerUnit> workers = new();
    private int currentGold;

    public Team Team => team;
    public int CurrentGold => currentGold;
    public int WorkerCount => workers.Count;
    public int MaxWorkers => maxWorkers;

    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    private void Start()
    {
        currentGold = startingGold;

        for (int i = 0; i < startingWorkers; i++)
        {
            SpawnWorker();
        }
    }

    public bool TryBuyWorker(int cost)
    {
        if (gameManager != null && gameManager.IsGameOver) return false;
        if (workers.Count >= maxWorkers) return false;
        if (currentGold < cost) return false;

        currentGold -= cost;
        SpawnWorker();

        return true;
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
    }

    private void SpawnWorker()
    {
        if (workerPrefab == null || workerSpawnPoint == null || dropOffPoint == null || goldVein == null)
        {
            Debug.LogWarning($"{name}: WorkerManager is missing references.");
            return;
        }

        if (workers.Count >= maxWorkers) return;

        WorkerUnit worker = Instantiate(
            workerPrefab,
            workerSpawnPoint.position,
            Quaternion.identity
        );

        workers.Add(worker);
        worker.Initialize(this, goldVein, dropOffPoint);
    }
}