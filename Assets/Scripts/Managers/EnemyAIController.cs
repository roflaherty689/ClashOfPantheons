using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class EnemyAIController : MonoBehaviour
{
    private static readonly ProductionSlotId[] ProductionSlots =
    {
        ProductionSlotId.Standard0,
        ProductionSlotId.Standard1,
        ProductionSlotId.Standard2,
        ProductionSlotId.Standard3,
        ProductionSlotId.Mythic
    };

    [SerializeField] private Team team = Team.Right;
    [SerializeField] private bool logDecisions;

    private GameManager gameManager;
    private WorkerManager economy;
    private GameDifficulty difficulty;
    private float nextDecisionTime;

    public string LastDecision { get; private set; } = "Waiting";
    public GameDifficulty Difficulty => difficulty;

    private void Awake()
    {
        difficulty = FactionSelectionSession.Difficulty;
    }

    public void Configure(GameManager manager, WorkerManager workerManager)
    {
        gameManager = manager;
        economy = workerManager;
    }

    private void ResolveFallbackDependencies()
    {
        gameManager ??= FindAnyObjectByType<GameManager>();
        if (economy == null)
        {
            foreach (WorkerManager candidate in FindObjectsByType<WorkerManager>())
            {
                if (candidate.Team == team) { economy = candidate; break; }
            }
        }

        if (economy != null)
        {
            economy.SetStartingGoldBonus(GetStartingGoldBonus(difficulty));
        }
    }

    private IEnumerator Start()
    {
        ResolveFallbackDependencies();
        if (gameManager == null || economy == null)
        {
            Debug.LogError($"{name}: Enemy AI requires a GameManager and {team} WorkerManager.", this);
            enabled = false;
            yield break;
        }

        yield return null;
        ScheduleNextDecision();
    }

    private void Update()
    {
        if (gameManager == null || economy == null || gameManager.IsGameOver || Time.time < nextDecisionTime)
        {
            return;
        }

        MakeDecision();
        ScheduleNextDecision();
    }

    private void MakeDecision()
    {
        bool acted = difficulty switch
        {
            GameDifficulty.Easy => MakeEasyDecision(),
            GameDifficulty.Medium => MakeStrategicDecision(3, false),
            GameDifficulty.Hard => MakeStrategicDecision(4, true),
            _ => false
        };

        if (!acted) SetDecision("Saved gold");
    }

    private bool MakeEasyDecision()
    {
        if (Random.value < 0.25f) return false;

        List<int> choices = new List<int>();
        if (economy.HasWorkerCapacity && economy.CurrentGold >= economy.WorkerCost) choices.Add(-1);
        for (int i = 0; i < ProductionSlots.Length; i++)
        {
            if (CanAffordSlot(ProductionSlots[i])) choices.Add(i);
        }

        if (choices.Count == 0) return false;
        int choice = choices[Random.Range(0, choices.Count)];
        return choice < 0 ? BuyWorker() : BuySlot(ProductionSlots[choice]);
    }

    private bool MakeStrategicDecision(int workerTarget, bool prioritizeVariety)
    {
        int unlockedProduction = GetUnlockedProductionCount();
        int requiredProductionForWorker = prioritizeVariety
            ? Mathf.Min(3, economy.WorkerCount + 1)
            : Mathf.Min(2, economy.WorkerCount);

        if (economy.WorkerCount < workerTarget && economy.HasWorkerCapacity &&
            economy.CurrentGold >= economy.WorkerCost &&
            unlockedProduction >= requiredProductionForWorker)
        {
            return BuyWorker();
        }

        ProductionSlotId bestSlot = ProductionSlotId.Standard0;
        int bestScore = int.MinValue;
        foreach (ProductionSlotId slotId in ProductionSlots)
        {
            if (!CanAffordSlot(slotId)) continue;
            int tier = gameManager.GetProductionTier(team, slotId);
            int score = prioritizeVariety && tier == 0 ? 100 : 30 - tier * 10;
            score += Random.Range(0, difficulty == GameDifficulty.Hard ? 8 : 25);
            if (score > bestScore)
            {
                bestScore = score;
                bestSlot = slotId;
            }
        }

        return bestScore != int.MinValue && BuySlot(bestSlot);
    }

    private int GetUnlockedProductionCount()
    {
        int count = 0;
        foreach (ProductionSlotId slotId in ProductionSlots)
        {
            if (gameManager.GetProductionTier(team, slotId) > 0) count++;
        }
        return count;
    }

    private bool CanAffordSlot(ProductionSlotId slotId)
    {
        if (gameManager.GetProductionTier(team, slotId) >= GameManager.MaximumProductionTier) return false;
        if (slotId == ProductionSlotId.Mythic &&
            gameManager.GetProductionTier(team, slotId) == 0)
        {
            return TryGetAffordableMythics(null) > 0;
        }
        return gameManager.TryGetProductionData(team, slotId, out UnitData data) &&
               data != null && economy.CurrentGold >= data.Cost;
    }

    private bool BuySlot(ProductionSlotId slotId)
    {
        bool success;
        if (slotId == ProductionSlotId.Mythic &&
            gameManager.GetProductionTier(team, slotId) == 0)
        {
            List<BaseUnit> choices = new List<BaseUnit>();
            TryGetAffordableMythics(choices);
            if (choices.Count == 0) return false;
            success = gameManager.TrySelectAndPurchaseMythic(team, choices[Random.Range(0, choices.Count)], economy);
        }
        else
        {
            success = gameManager.TryPurchaseProduction(team, slotId, economy);
        }

        if (success)
        {
            string label = slotId.ToString();
            if (gameManager.TryGetProductionRole(team, slotId, out UnitRole role))
            {
                label = slotId == ProductionSlotId.Mythic
                    ? role.ToString()
                    : $"{slotId} ({role})";
            }

            SetDecision(
                $"Purchased {label} tier {gameManager.GetProductionTier(team, slotId)}");
        }

        return success;
    }

    private int TryGetAffordableMythics(List<BaseUnit> results)
    {
        if (gameManager.MythicUnitRoster == null) return 0;
        int count = 0;
        foreach (BaseUnit unit in gameManager.MythicUnitRoster.Units)
        {
            if (unit == null || unit.UnitData == null || economy.CurrentGold < unit.UnitData.Cost) continue;
            count++;
            results?.Add(unit);
        }
        return count;
    }

    private bool BuyWorker()
    {
        bool success = economy.TryBuyWorker();
        if (success) SetDecision($"Purchased worker {economy.WorkerCount}/{economy.MaxWorkers}");
        return success;
    }

    private void SetDecision(string decision)
    {
        LastDecision = decision;
        if (logDecisions) Debug.Log($"Enemy AI ({difficulty}): {decision}", this);
    }

    private void ScheduleNextDecision()
    {
        Vector2 delay = difficulty switch
        {
            GameDifficulty.Easy => new Vector2(6f, 10f),
            GameDifficulty.Medium => new Vector2(3f, 5f),
            GameDifficulty.Hard => new Vector2(1.5f, 2.5f),
            _ => new Vector2(6f, 10f)
        };
        nextDecisionTime = Time.time + Random.Range(delay.x, delay.y);
    }

    private static int GetStartingGoldBonus(GameDifficulty value)
    {
        return value switch
        {
            GameDifficulty.Medium => 50,
            GameDifficulty.Hard => 150,
            _ => 0
        };
    }
}
