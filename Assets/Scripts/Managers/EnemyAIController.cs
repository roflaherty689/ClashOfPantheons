using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class EnemyAIController : MonoBehaviour
{
    private static readonly UnitRole[] Roles =
    {
        UnitRole.Melee, UnitRole.Archer, UnitRole.Cavalry, UnitRole.Siege, UnitRole.Mythic
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
        gameManager = FindAnyObjectByType<GameManager>();
        foreach (WorkerManager candidate in FindObjectsByType<WorkerManager>())
        {
            if (candidate.Team == team)
            {
                economy = candidate;
                break;
            }
        }

        difficulty = FactionSelectionSession.Difficulty;
        if (economy != null)
        {
            economy.SetStartingGoldBonus(GetStartingGoldBonus(difficulty));
        }
    }

    private IEnumerator Start()
    {
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
        for (int i = 0; i < Roles.Length; i++)
        {
            if (CanAffordRole(Roles[i])) choices.Add(i);
        }

        if (choices.Count == 0) return false;
        int choice = choices[Random.Range(0, choices.Count)];
        return choice < 0 ? BuyWorker() : BuyRole(Roles[choice]);
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

        UnitRole bestRole = UnitRole.Melee;
        int bestScore = int.MinValue;
        foreach (UnitRole role in Roles)
        {
            if (!CanAffordRole(role)) continue;
            int tier = gameManager.GetProductionTier(team, role);
            int score = prioritizeVariety && tier == 0 ? 100 : 30 - tier * 10;
            score += Random.Range(0, difficulty == GameDifficulty.Hard ? 8 : 25);
            if (score > bestScore)
            {
                bestScore = score;
                bestRole = role;
            }
        }

        return bestScore != int.MinValue && BuyRole(bestRole);
    }

    private int GetUnlockedProductionCount()
    {
        int count = 0;
        foreach (UnitRole role in Roles)
        {
            if (gameManager.GetProductionTier(team, role) > 0) count++;
        }
        return count;
    }

    private bool CanAffordRole(UnitRole role)
    {
        if (gameManager.GetProductionTier(team, role) >= GameManager.MaximumProductionTier) return false;
        if (role == UnitRole.Mythic && gameManager.GetProductionTier(team, role) == 0)
        {
            return TryGetAffordableMythics(null) > 0;
        }
        return gameManager.TryGetProductionData(team, role, out UnitData data) &&
               data != null && economy.CurrentGold >= data.Cost;
    }

    private bool BuyRole(UnitRole role)
    {
        bool success;
        if (role == UnitRole.Mythic && gameManager.GetProductionTier(team, role) == 0)
        {
            List<BaseUnit> choices = new List<BaseUnit>();
            TryGetAffordableMythics(choices);
            if (choices.Count == 0) return false;
            success = gameManager.TrySelectAndPurchaseMythic(team, choices[Random.Range(0, choices.Count)], economy);
        }
        else
        {
            success = gameManager.TryPurchaseProduction(team, role, economy);
        }

        if (success) SetDecision($"Purchased {role} tier {gameManager.GetProductionTier(team, role)}");
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
