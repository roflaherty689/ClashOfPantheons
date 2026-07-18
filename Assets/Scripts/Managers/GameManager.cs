using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public enum SpawnPattern
{
    GlobalInterval,
    PerUnitInterval
}

public class GameManager : MonoBehaviour
{
    public const int MaximumProductionTier = ProductionTierRules.MaximumTier;

    private static readonly UnitRole[] SpawnRoles =
    {
        UnitRole.Melee,
        UnitRole.Archer,
        UnitRole.Cavalry,
        UnitRole.Siege,
        UnitRole.Mythic
    };

    [Header("Team Factions")]
    [SerializeField] private FactionData leftFactionData;
    [SerializeField] private FactionData rightFactionData;

    [Header("Mythic Selection")]
    [SerializeField] private MythicUnitRoster mythicUnitRoster;

    [Header("Faction Presentation")]
    [SerializeField] private Image leftCastleIcon;
    [SerializeField] private Image rightCastleIcon;

    [Header("Spawn Points")]
    [SerializeField] private Transform leftSpawnPoint;
    [SerializeField] private Transform rightSpawnPoint;

    [Header("Targets")]
    [SerializeField] private Transform leftTargetPoint;
    [SerializeField] private Transform rightTargetPoint;

    [Header("Spawn Settings")]
    [SerializeField] private SpawnPattern spawnPattern = SpawnPattern.GlobalInterval;
    [SerializeField, Min(0.1f)] private float spawnInterval = 3f;
    [FormerlySerializedAs("maxMeleeUnitsPerTeam")]
    [SerializeField, Min(1)] private int maxUnitsPerTeam = 5;
    [SerializeField] private bool randomiseSpawns;

    [Header("Game Settings")]
    [SerializeField, Min(0f)] private float gameSpeed = 1f;
    [SerializeField, Min(1f)] private float matchDurationSeconds = 300f;
    [SerializeField] private bool setTeamColour = true;

    private readonly ProductionStateController productionState = new ProductionStateController();
    private readonly MatchStateController matchState = new MatchStateController();
    private FactionTeamInitializer factionTeamInitializer;
    private UnitSpawnController unitSpawnController;
    private Base leftBase;
    private Base rightBase;

    public bool IsGameOver => matchState.IsGameOver;
    public bool HasWinner => matchState.HasWinner;
    public Team WinningTeam => matchState.WinningTeam;
    public MatchEndReason EndReason => matchState.EndReason;
    public float TimeRemaining => matchState.TimeRemaining;
    public bool SetTeamColour => setTeamColour;
    public MythicUnitRoster MythicUnitRoster => mythicUnitRoster;

    private void Awake()
    {
        if (mythicUnitRoster == null)
        {
            mythicUnitRoster = Resources.Load<MythicUnitRoster>("MythicUnitRoster");
        }

        factionTeamInitializer = new FactionTeamInitializer(
            leftFactionData,
            rightFactionData,
            leftCastleIcon,
            rightCastleIcon);
        factionTeamInitializer.Initialize();
        SynchronizeFactionInitialization();
        unitSpawnController = new UnitSpawnController(
            leftFactionData,
            rightFactionData,
            leftSpawnPoint,
            rightSpawnPoint,
            leftTargetPoint,
            rightTargetPoint,
            productionState,
            maxUnitsPerTeam,
            randomiseSpawns);

        EnemyAIController enemyAI = FindAnyObjectByType<EnemyAIController>();
        if (enemyAI == null) enemyAI = gameObject.AddComponent<EnemyAIController>();
        enemyAI.Configure(this, factionTeamInitializer.RightEconomy);
    }

    private void Start()
    {
        Time.timeScale = Mathf.Max(0f, gameSpeed);
        matchState.Start(matchDurationSeconds);
        ValidateConfiguration();
    }

    private void Update()
    {
        if (matchState.IsGameOver) return;

        if (spawnPattern == SpawnPattern.PerUnitInterval)
        {
            UpdatePerUnitSpawns(Team.Left, leftFactionData);
            UpdatePerUnitSpawns(Team.Right, rightFactionData);
            return;
        }

        unitSpawnController?.UpdateGlobalSpawns(Time.deltaTime, spawnInterval);
    }

    private void LateUpdate()
    {
        if (matchState.AdvanceClock(Time.deltaTime))
        {
            ResolveTimeout();
        }
    }

    public void EndGame(Team winningTeam)
    {
        matchState.CompleteWithWinner(winningTeam, MatchEndReason.StrongholdDestroyed);
        SoundManager.Play(winningTeam == Team.Left ? SoundCue.Victory : SoundCue.Defeat);
    }

    public void RegisterUnitDeath(Team team, UnitRole role, int unitCost)
    {
        matchState.RegisterUnitDeath(team, role, unitCost);
    }

    public int GetUnitLossCount(Team team, UnitRole role)
    {
        return matchState.GetUnitLossCount(team, role);
    }

    public int GetTotalUnitLossValue(Team team)
    {
        return matchState.GetTotalUnitLossValue(team);
    }

    public int GetProductionTier(Team team, UnitRole role)
    {
        return productionState.GetTier(team, role);
    }

    public bool TryGetProductionData(Team team, UnitRole role, out UnitData data)
    {
        if (role == UnitRole.Mythic)
        {
            BaseUnit selectedMythic = GetSelectedMythicUnit(team);
            if (selectedMythic != null && selectedMythic.UnitData != null)
            {
                data = selectedMythic.UnitData;
                return true;
            }
        }

        FactionData factionData = GetFactionData(team);
        if (factionData != null && factionData.TryGetUnitData(role, out data))
        {
            return true;
        }

        data = null;
        return false;
    }

    public bool TryPurchaseProduction(Team team, UnitRole role, WorkerManager economy)
    {
        if (matchState.IsGameOver || economy == null || economy.Team != team) return false;

        int roleIndex = GetRoleIndex(role);
        if (roleIndex < 0) return false;

        if (!ProductionTierRules.TryAdvance(
                productionState.GetTier(team, role),
                role == UnitRole.Mythic,
                out int nextTier))
        {
            return false;
        }

        if (!TryGetProductionData(team, role, out UnitData data)) return false;
        if (!economy.TrySpendGold(data.Cost)) return false;

        productionState.SetTier(team, role, nextTier);
        if (team == Team.Left) SoundManager.Play(SoundCue.Purchase);

        if (nextTier == 1)
        {
            productionState.ResetTimer(team, role);
        }

        return true;
    }

    public BaseUnit GetSelectedMythicUnit(Team team)
    {
        return productionState.GetSelectedMythic(team);
    }

    public bool HasMythicChoices(Team team)
    {
        if (matchState.IsGameOver || mythicUnitRoster == null ||
            GetProductionTier(team, UnitRole.Mythic) != 0)
        {
            return false;
        }

        foreach (BaseUnit candidate in mythicUnitRoster.Units)
        {
            if (candidate != null && candidate.UnitData != null)
            {
                return true;
            }
        }

        return false;
    }

    public bool TrySelectAndPurchaseMythic(Team team, BaseUnit prefab, WorkerManager economy)
    {
        if (matchState.IsGameOver || economy == null || economy.Team != team || prefab == null ||
            mythicUnitRoster == null || !mythicUnitRoster.Contains(prefab))
        {
            return false;
        }

        int roleIndex = GetRoleIndex(UnitRole.Mythic);
        if (roleIndex < 0 ||
            !ProductionTierRules.TryUnlockSelectedOption(
                productionState.GetTier(team, UnitRole.Mythic),
                out int nextTier) ||
            prefab.UnitData == null ||
            !economy.TrySpendGold(prefab.UnitData.Cost))
        {
            return false;
        }

        productionState.SetSelectedMythic(team, prefab);
        productionState.SetTier(team, UnitRole.Mythic, nextTier);
        productionState.ResetTimer(team, UnitRole.Mythic);
        if (team == Team.Left) SoundManager.Play(SoundCue.Purchase);
        return true;
    }

    public void RestartMatch()
    {
        Time.timeScale = 1f;
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);
    }

    private void ResolveTimeout()
    {
        if (leftBase == null || rightBase == null)
        {
            ResolveBases();
        }

        if (leftBase == null || rightBase == null)
        {
            Debug.LogError($"{name}: Cannot resolve match timeout without both team strongholds.", this);
            matchState.CompleteDraw(MatchEndReason.TimeoutDraw);
            return;
        }

        MatchResult result = MatchResultResolver.ResolveTimeout(
            leftBase.CurrentHealth,
            rightBase.CurrentHealth,
            GetTotalUnitLossValue(Team.Left),
            GetTotalUnitLossValue(Team.Right));

        if (result.HasWinner)
        {
            matchState.CompleteWithWinner(result.Winner, result.Reason);
            SoundManager.Play(result.Winner == Team.Left ? SoundCue.Victory : SoundCue.Defeat);
            return;
        }

        matchState.CompleteDraw(result.Reason);
        SoundManager.Play(SoundCue.Draw);
    }

    private void ResolveBases()
    {
        factionTeamInitializer?.ResolveBases();
        SynchronizeFactionInitialization();
    }

    private void SynchronizeFactionInitialization()
    {
        if (factionTeamInitializer == null) return;

        leftFactionData = factionTeamInitializer.LeftFaction;
        rightFactionData = factionTeamInitializer.RightFaction;
        leftBase = factionTeamInitializer.LeftBase;
        rightBase = factionTeamInitializer.RightBase;
    }

    private static int GetRoleIndex(UnitRole role)
    {
        return ProductionStateController.GetRoleIndex(role);
    }

    private void UpdatePerUnitSpawns(Team team, FactionData factionData)
    {
        if (factionData == null) return;

        productionState.UpdateSpawns(team, Time.deltaTime, unitSpawnController);
    }

    private FactionData GetFactionData(Team team)
    {
        return team == Team.Left ? leftFactionData : rightFactionData;
    }

    private void ValidateConfiguration()
    {
        ValidateTeamConfiguration(Team.Left, leftFactionData, leftSpawnPoint, leftTargetPoint);
        ValidateTeamConfiguration(Team.Right, rightFactionData, rightSpawnPoint, rightTargetPoint);
    }

    private void ValidateTeamConfiguration(
        Team team,
        FactionData factionData,
        Transform spawnPoint,
        Transform targetPoint)
    {
        if (factionData == null || spawnPoint == null || targetPoint == null)
        {
            Debug.LogError($"{name}: {team} team spawn configuration is incomplete.", this);
            return;
        }

        foreach (UnitRole role in SpawnRoles)
        {
            if (!factionData.TryGetUnitData(role, out _))
            {
                Debug.LogError(
                    $"{name}: Faction '{factionData.FactionName}' has no valid {role} unit configuration.",
                    factionData);
            }
        }
    }
}
