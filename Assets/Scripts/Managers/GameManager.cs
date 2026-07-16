using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public enum SpawnPattern
{
    GlobalInterval,
    PerUnitInterval
}

public enum MatchEndReason
{
    None,
    StrongholdDestroyed,
    TimeoutHealth,
    TimeoutUnitLossValue,
    TimeoutDraw
}

public class GameManager : MonoBehaviour
{
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

    private readonly float[] leftUnitSpawnTimers = new float[SpawnRoles.Length];
    private readonly float[] rightUnitSpawnTimers = new float[SpawnRoles.Length];
    private readonly int[,] unitLossCounts = new int[2, SpawnRoles.Length];
    private readonly int[] totalUnitLossValues = new int[2];

    private float globalSpawnTimer;
    private int leftReadySpawnIndex;
    private int rightReadySpawnIndex;
    private int leftSpawnIndex;
    private int rightSpawnIndex;
    private bool gameOver;
    private bool hasWinner;
    private Team winningTeam;
    private MatchEndReason endReason;
    private float timeRemaining;
    private Base leftBase;
    private Base rightBase;

    public bool IsGameOver => gameOver;
    public bool HasWinner => hasWinner;
    public Team WinningTeam => winningTeam;
    public MatchEndReason EndReason => endReason;
    public float TimeRemaining => Mathf.Max(0f, timeRemaining);
    public bool SetTeamColour => setTeamColour;

    private void Start()
    {
        Time.timeScale = Mathf.Max(0f, gameSpeed);
        timeRemaining = Mathf.Max(1f, matchDurationSeconds);
        ResolveBases();
        ValidateConfiguration();
    }

    private void Update()
    {
        if (gameOver) return;

        if (spawnPattern == SpawnPattern.PerUnitInterval)
        {
            UpdatePerUnitSpawns(
                Team.Left,
                leftFactionData,
                leftUnitSpawnTimers,
                ref leftReadySpawnIndex);
            UpdatePerUnitSpawns(
                Team.Right,
                rightFactionData,
                rightUnitSpawnTimers,
                ref rightReadySpawnIndex);
            return;
        }

        UpdateGlobalSpawns();
    }

    private void LateUpdate()
    {
        if (gameOver) return;

        timeRemaining = Mathf.Max(0f, timeRemaining - Time.deltaTime);
        if (timeRemaining <= 0f)
        {
            ResolveTimeout();
        }
    }

    public void EndGame(Team winningTeam)
    {
        CompleteMatch(winningTeam, MatchEndReason.StrongholdDestroyed);
    }

    public void RegisterUnitDeath(Team team, UnitRole role, int unitCost)
    {
        if (gameOver) return;

        int teamIndex = GetTeamIndex(team);
        int roleIndex = GetRoleIndex(role);
        if (roleIndex < 0) return;

        unitLossCounts[teamIndex, roleIndex]++;
        totalUnitLossValues[teamIndex] += Mathf.Max(0, unitCost);
    }

    public int GetUnitLossCount(Team team, UnitRole role)
    {
        int roleIndex = GetRoleIndex(role);
        return roleIndex < 0 ? 0 : unitLossCounts[GetTeamIndex(team), roleIndex];
    }

    public int GetTotalUnitLossValue(Team team)
    {
        return totalUnitLossValues[GetTeamIndex(team)];
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
            CompleteDraw(MatchEndReason.TimeoutDraw);
            return;
        }

        int healthComparison = leftBase.CurrentHealth.CompareTo(rightBase.CurrentHealth);
        if (healthComparison != 0)
        {
            CompleteMatch(
                healthComparison > 0 ? Team.Left : Team.Right,
                MatchEndReason.TimeoutHealth);
            return;
        }

        int leftLossValue = GetTotalUnitLossValue(Team.Left);
        int rightLossValue = GetTotalUnitLossValue(Team.Right);
        if (leftLossValue != rightLossValue)
        {
            CompleteMatch(
                leftLossValue < rightLossValue ? Team.Left : Team.Right,
                MatchEndReason.TimeoutUnitLossValue);
            return;
        }

        CompleteDraw(MatchEndReason.TimeoutDraw);
    }

    private void CompleteMatch(Team winner, MatchEndReason reason)
    {
        if (gameOver) return;

        gameOver = true;
        hasWinner = true;
        winningTeam = winner;
        endReason = reason;
    }

    private void CompleteDraw(MatchEndReason reason)
    {
        if (gameOver) return;

        gameOver = true;
        hasWinner = false;
        endReason = reason;
    }

    private void ResolveBases()
    {
        leftBase = null;
        rightBase = null;

        foreach (Base battleBase in FindObjectsByType<Base>())
        {
            if (battleBase.Team == Team.Left)
            {
                leftBase = battleBase;
            }
            else
            {
                rightBase = battleBase;
            }
        }
    }

    private static int GetTeamIndex(Team team)
    {
        return team == Team.Left ? 0 : 1;
    }

    private static int GetRoleIndex(UnitRole role)
    {
        for (int i = 0; i < SpawnRoles.Length; i++)
        {
            if (SpawnRoles[i] == role) return i;
        }

        return -1;
    }

    private void UpdateGlobalSpawns()
    {
        globalSpawnTimer += Time.deltaTime;

        if (globalSpawnTimer < Mathf.Max(0.1f, spawnInterval)) return;

        globalSpawnTimer -= Mathf.Max(0.1f, spawnInterval);
        TrySpawnGlobalUnit(Team.Left);
        TrySpawnGlobalUnit(Team.Right);
    }

    private void UpdatePerUnitSpawns(
        Team team,
        FactionData factionData,
        float[] timers,
        ref int readySpawnIndex)
    {
        if (factionData == null) return;

        for (int i = 0; i < SpawnRoles.Length; i++)
        {
            timers[i] += Time.deltaTime;
        }

        int scanStartIndex = readySpawnIndex;
        int availableSpawnSlots = GetAvailableSpawnSlots(team);

        for (int offset = 0; offset < SpawnRoles.Length; offset++)
        {
            int roleIndex = (scanStartIndex + offset) % SpawnRoles.Length;
            UnitRole role = SpawnRoles[roleIndex];

            if (!factionData.TryGetUnitData(role, out UnitData data))
            {
                continue;
            }

            float roleSpawnInterval = data.SpawnInterval;
            timers[roleIndex] = Mathf.Min(timers[roleIndex], roleSpawnInterval);

            if (timers[roleIndex] < roleSpawnInterval)
            {
                continue;
            }

            if (availableSpawnSlots <= 0 || !TrySpawnUnit(team, role))
            {
                continue;
            }

            timers[roleIndex] -= roleSpawnInterval;
            availableSpawnSlots--;
            readySpawnIndex = (roleIndex + 1) % SpawnRoles.Length;
        }
    }

    private void TrySpawnGlobalUnit(Team team)
    {
        if (GetAvailableSpawnSlots(team) <= 0) return;

        if (randomiseSpawns)
        {
            if (TrySelectWeightedRole(team, out UnitRole selectedRole))
            {
                TrySpawnUnit(team, selectedRole);
            }

            return;
        }

        int scanStartIndex = team == Team.Left ? leftSpawnIndex : rightSpawnIndex;
        for (int offset = 0; offset < SpawnRoles.Length; offset++)
        {
            int roleIndex = (scanStartIndex + offset) % SpawnRoles.Length;
            if (!TrySpawnUnit(team, SpawnRoles[roleIndex]))
            {
                continue;
            }

            int nextSpawnIndex = (roleIndex + 1) % SpawnRoles.Length;
            if (team == Team.Left)
            {
                leftSpawnIndex = nextSpawnIndex;
            }
            else
            {
                rightSpawnIndex = nextSpawnIndex;
            }

            return;
        }
    }

    private bool TrySelectWeightedRole(Team team, out UnitRole selectedRole)
    {
        FactionData factionData = GetFactionData(team);
        float totalWeight = 0f;

        foreach (UnitRole role in SpawnRoles)
        {
            if (factionData != null && factionData.TryGetUnitData(role, out UnitData data))
            {
                totalWeight += GetSpawnWeight(data);
            }
        }

        if (totalWeight <= 0f)
        {
            selectedRole = default;
            return false;
        }

        selectedRole = default;
        float roll = Random.Range(0f, totalWeight);
        foreach (UnitRole role in SpawnRoles)
        {
            if (factionData == null || !factionData.TryGetUnitData(role, out UnitData data))
            {
                continue;
            }

            selectedRole = role;
            roll -= GetSpawnWeight(data);
            if (roll <= 0f)
            {
                return true;
            }
        }

        return true;
    }

    private static float GetSpawnWeight(UnitData data)
    {
        return 1f / Mathf.Max(1, data.Cost);
    }

    private int GetAvailableSpawnSlots(Team team)
    {
        BaseUnit[] allUnits = FindObjectsByType<BaseUnit>();
        int teamUnitCount = 0;

        foreach (BaseUnit unit in allUnits)
        {
            if (unit.Team == team)
            {
                teamUnitCount++;
            }
        }

        return Mathf.Max(0, maxUnitsPerTeam - teamUnitCount);
    }

    private bool TrySpawnUnit(Team team, UnitRole role)
    {
        FactionData factionData = GetFactionData(team);
        Transform spawnPoint = team == Team.Left ? leftSpawnPoint : rightSpawnPoint;
        Transform targetPoint = team == Team.Left ? leftTargetPoint : rightTargetPoint;

        if (factionData == null || spawnPoint == null || targetPoint == null)
        {
            return false;
        }

        if (!factionData.TryGetUnitPrefab(role, out BaseUnit prefab))
        {
            return false;
        }

        BaseUnit unitInstance = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        unitInstance.Initialize(team, targetPoint, role);
        return true;
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
