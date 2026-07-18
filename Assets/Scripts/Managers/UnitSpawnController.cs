using UnityEngine;

public sealed class UnitSpawnController : IProductionSpawnContext
{
    private static readonly UnitRole[] Roles =
    {
        UnitRole.Melee,
        UnitRole.Archer,
        UnitRole.Cavalry,
        UnitRole.Siege,
        UnitRole.Mythic
    };

    private readonly FactionData leftFaction;
    private readonly FactionData rightFaction;
    private readonly Transform leftSpawnPoint;
    private readonly Transform rightSpawnPoint;
    private readonly Transform leftTargetPoint;
    private readonly Transform rightTargetPoint;
    private readonly ProductionStateController productionState;
    private readonly int maxUnitsPerTeam;
    private readonly bool randomiseSpawns;

    private float globalSpawnTimer;
    private int leftSpawnIndex;
    private int rightSpawnIndex;

    public UnitSpawnController(
        FactionData leftFaction,
        FactionData rightFaction,
        Transform leftSpawnPoint,
        Transform rightSpawnPoint,
        Transform leftTargetPoint,
        Transform rightTargetPoint,
        ProductionStateController productionState,
        int maxUnitsPerTeam,
        bool randomiseSpawns)
    {
        this.leftFaction = leftFaction;
        this.rightFaction = rightFaction;
        this.leftSpawnPoint = leftSpawnPoint;
        this.rightSpawnPoint = rightSpawnPoint;
        this.leftTargetPoint = leftTargetPoint;
        this.rightTargetPoint = rightTargetPoint;
        this.productionState = productionState;
        this.maxUnitsPerTeam = maxUnitsPerTeam;
        this.randomiseSpawns = randomiseSpawns;
    }

    public void UpdateGlobalSpawns(float deltaTime, float interval)
    {
        float safeInterval = Mathf.Max(0.1f, interval);
        globalSpawnTimer += Mathf.Max(0f, deltaTime);
        if (globalSpawnTimer < safeInterval) return;

        globalSpawnTimer -= safeInterval;
        TrySpawnGlobalUnit(Team.Left);
        TrySpawnGlobalUnit(Team.Right);
    }

    public int GetAvailableSpawnSlots(Team team)
    {
        int teamUnitCount = 0;
        foreach (BaseUnit unit in Object.FindObjectsByType<BaseUnit>())
        {
            if (unit.Team == team)
            {
                teamUnitCount++;
            }
        }

        return Mathf.Max(0, maxUnitsPerTeam - teamUnitCount);
    }

    public bool TryGetSpawnInterval(Team team, UnitRole role, out float interval)
    {
        FactionData faction = GetFaction(team);
        if (faction != null && faction.TryGetUnitData(role, out UnitData data))
        {
            interval = data.SpawnInterval;
            return true;
        }

        interval = 0f;
        return false;
    }

    public bool TrySpawnUnit(Team team, UnitRole role, int productionTier)
    {
        FactionData faction = GetFaction(team);
        Transform spawnPoint = team == Team.Left ? leftSpawnPoint : rightSpawnPoint;
        Transform targetPoint = team == Team.Left ? leftTargetPoint : rightTargetPoint;
        if (faction == null || spawnPoint == null || targetPoint == null)
        {
            return false;
        }

        BaseUnit prefab = role == UnitRole.Mythic
            ? productionState.GetSelectedMythic(team)
            : null;
        if (prefab == null && !faction.TryGetUnitPrefab(role, out prefab))
        {
            return false;
        }

        BaseUnit instance = Object.Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        instance.Initialize(team, targetPoint, role, productionTier);
        return true;
    }

    private void TrySpawnGlobalUnit(Team team)
    {
        if (GetAvailableSpawnSlots(team) <= 0) return;

        if (randomiseSpawns)
        {
            if (TrySelectWeightedRole(team, out UnitRole selectedRole))
            {
                TrySpawnUnit(team, selectedRole, 1);
            }

            return;
        }

        int scanStartIndex = team == Team.Left ? leftSpawnIndex : rightSpawnIndex;
        for (int offset = 0; offset < Roles.Length; offset++)
        {
            int roleIndex = (scanStartIndex + offset) % Roles.Length;
            if (!TrySpawnUnit(team, Roles[roleIndex], 1))
            {
                continue;
            }

            int nextIndex = (roleIndex + 1) % Roles.Length;
            if (team == Team.Left)
            {
                leftSpawnIndex = nextIndex;
            }
            else
            {
                rightSpawnIndex = nextIndex;
            }

            return;
        }
    }

    private bool TrySelectWeightedRole(Team team, out UnitRole selectedRole)
    {
        FactionData faction = GetFaction(team);
        float totalWeight = 0f;
        foreach (UnitRole role in Roles)
        {
            if (faction != null && faction.TryGetUnitData(role, out UnitData data))
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
        foreach (UnitRole role in Roles)
        {
            if (faction == null || !faction.TryGetUnitData(role, out UnitData data)) continue;

            selectedRole = role;
            roll -= GetSpawnWeight(data);
            if (roll <= 0f) return true;
        }

        return true;
    }

    private FactionData GetFaction(Team team)
    {
        return team == Team.Left ? leftFaction : rightFaction;
    }

    private static float GetSpawnWeight(UnitData data)
    {
        return 1f / Mathf.Max(1, data.Cost);
    }
}
