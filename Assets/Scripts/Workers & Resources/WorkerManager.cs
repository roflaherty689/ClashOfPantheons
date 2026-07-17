using System.Collections.Generic;
using UnityEngine;

public class WorkerManager : MonoBehaviour
{
    [Header("Team")]
    [SerializeField] private Team team;

    [Header("Worker Setup")]
    [SerializeField] private WorkerUnit workerPrefab;
    [SerializeField] private Transform workerSpawnPoint;
    [SerializeField] private GoldVein goldVein;

    [Header("Worker Counts")]
    [SerializeField, Min(0)] private int startingWorkers = 1;
    [SerializeField, Min(1)] private int maxWorkers = 5;

    [Header("Resources")]
    [SerializeField] private int startingGold = 200;
    [SerializeField, Min(0)] private int workerCost = 100;
    [SerializeField, Min(0)] private int baseGoldPerTrip = 5;
    [SerializeField, Min(0f)] private float goldUpgradeMultiplier = 1f;

    private GameManager gameManager;
    private Transform dropOffPoint;

    private readonly List<WorkerUnit> workers = new();
    private int currentGold;

    public Team Team => team;
    public int CurrentGold => currentGold;
    public int WorkerCount => workers.Count;
    public int MaxWorkers => maxWorkers;
    public int WorkerCost => workerCost;
    public float GoldUpgradeMultiplier => goldUpgradeMultiplier;
    public int GoldPerWorkerTrip => Mathf.RoundToInt(baseGoldPerTrip * goldUpgradeMultiplier);
    public int TotalGoldPerTrip => Mathf.RoundToInt(baseGoldPerTrip * goldUpgradeMultiplier * WorkerCount);
    public bool HasWorkerCapacity => WorkerCount < maxWorkers;

    public void ApplyWorkerPrefab(WorkerUnit factionWorkerPrefab)
    {
        if (factionWorkerPrefab == null)
        {
            Debug.LogWarning($"{name}: Cannot apply a missing faction worker prefab.", this);
            return;
        }

        workerPrefab = factionWorkerPrefab;
    }

    private void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();

        BasePresentation presentation = GetComponent<BasePresentation>();
        if (presentation != null)
        {
            dropOffPoint = presentation.DropOffPoint;
        }
    }

    private void Start()
    {
        currentGold = Mathf.Max(0, startingGold);

        int workersToSpawn = Mathf.Min(Mathf.Max(0, startingWorkers), Mathf.Max(1, maxWorkers));
        for (int i = 0; i < workersToSpawn; i++)
        {
            if (!TrySpawnWorker())
            {
                break;
            }
        }
    }

    public bool TryBuyWorker()
    {
        return TryBuyWorker(workerCost);
    }

    public bool TryBuyWorker(int cost)
    {
        if (gameManager != null && gameManager.IsGameOver) return false;
        if (cost < 0) return false;
        if (workers.Count >= maxWorkers) return false;
        if (currentGold < cost) return false;
        if (!TrySpawnWorker()) return false;

        currentGold -= cost;

        return true;
    }

    public bool TrySpendGold(int amount)
    {
        if (gameManager != null && gameManager.IsGameOver) return false;
        if (amount < 0 || currentGold < amount) return false;

        currentGold -= amount;
        return true;
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;

        currentGold += amount;
    }

    public void DepositWorkerGold()
    {
        AddGold(GoldPerWorkerTrip);
    }

    internal void UnregisterWorker(WorkerUnit worker)
    {
        workers.Remove(worker);
    }

    private bool TrySpawnWorker()
    {
        if (workerPrefab == null || workerSpawnPoint == null || dropOffPoint == null || goldVein == null)
        {
            Debug.LogWarning($"{name}: WorkerManager is missing references.");
            return false;
        }

        if (workers.Count >= maxWorkers) return false;

        WorkerUnit worker = Instantiate(
            workerPrefab,
            workerSpawnPoint.position,
            Quaternion.identity
        );

        workers.Add(worker);
        worker.Initialize(this, goldVein, dropOffPoint);

        return true;
    }
}
