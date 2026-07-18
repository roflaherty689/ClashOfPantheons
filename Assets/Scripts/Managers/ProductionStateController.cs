using System;

public interface IProductionSpawnContext
{
    int GetAvailableSpawnSlots(Team team);
    bool TryGetSpawnInterval(Team team, UnitRole role, out float interval);
    bool TrySpawnUnit(Team team, UnitRole role, int productionTier);
}

public sealed class ProductionStateController
{
    public const int RoleCount = 5;

    private readonly int[,] tiers = new int[2, RoleCount];
    private readonly float[,] timers = new float[2, RoleCount];
    private readonly int[] readySpawnIndices = new int[2];
    private readonly BaseUnit[] selectedMythics = new BaseUnit[2];

    public int GetTier(Team team, UnitRole role)
    {
        int roleIndex = GetRoleIndex(role);
        return roleIndex < 0 ? 0 : tiers[GetTeamIndex(team), roleIndex];
    }

    public void SetTier(Team team, UnitRole role, int tier)
    {
        int roleIndex = GetRoleIndex(role);
        if (roleIndex < 0) return;

        tiers[GetTeamIndex(team), roleIndex] = tier;
    }

    public BaseUnit GetSelectedMythic(Team team)
    {
        return selectedMythics[GetTeamIndex(team)];
    }

    public void SetSelectedMythic(Team team, BaseUnit prefab)
    {
        selectedMythics[GetTeamIndex(team)] = prefab;
    }

    public void UpdateSpawns(
        Team team,
        float deltaTime,
        IProductionSpawnContext spawnContext)
    {
        if (spawnContext == null) return;

        AdvanceUnlockedTimers(team, deltaTime);

        int teamIndex = GetTeamIndex(team);
        int scanStartIndex = readySpawnIndices[teamIndex];
        int availableSpawnSlots = -1;

        for (int offset = 0; offset < RoleCount; offset++)
        {
            int roleIndex = (scanStartIndex + offset) % RoleCount;
            int productionTier = tiers[teamIndex, roleIndex];
            if (productionTier <= 0)
            {
                continue;
            }

            UnitRole role = GetRole(roleIndex);
            if (!spawnContext.TryGetSpawnInterval(team, role, out float interval))
            {
                continue;
            }

            timers[teamIndex, roleIndex] = Math.Min(timers[teamIndex, roleIndex], interval);
            if (timers[teamIndex, roleIndex] < interval)
            {
                continue;
            }

            if (availableSpawnSlots < 0)
            {
                availableSpawnSlots = spawnContext.GetAvailableSpawnSlots(team);
            }

            if (availableSpawnSlots <= 0 ||
                !spawnContext.TrySpawnUnit(team, role, productionTier))
            {
                continue;
            }

            timers[teamIndex, roleIndex] =
                Math.Max(0f, timers[teamIndex, roleIndex] - interval);
            availableSpawnSlots--;
            readySpawnIndices[teamIndex] = (roleIndex + 1) % RoleCount;
        }
    }

    public void ResetTimer(Team team, UnitRole role)
    {
        int roleIndex = GetRoleIndex(role);
        if (roleIndex < 0) return;

        timers[GetTeamIndex(team), roleIndex] = 0f;
    }

    public static int GetRoleIndex(UnitRole role)
    {
        return role switch
        {
            UnitRole.Melee => 0,
            UnitRole.Archer => 1,
            UnitRole.Cavalry => 2,
            UnitRole.Siege => 3,
            UnitRole.Mythic => 4,
            _ => -1
        };
    }

    private void AdvanceUnlockedTimers(Team team, float deltaTime)
    {
        int teamIndex = GetTeamIndex(team);
        float safeDeltaTime = Math.Max(0f, deltaTime);

        for (int roleIndex = 0; roleIndex < RoleCount; roleIndex++)
        {
            if (tiers[teamIndex, roleIndex] <= 0)
            {
                timers[teamIndex, roleIndex] = 0f;
                continue;
            }

            timers[teamIndex, roleIndex] += safeDeltaTime;
        }
    }

    private static UnitRole GetRole(int roleIndex)
    {
        return roleIndex switch
        {
            0 => UnitRole.Melee,
            1 => UnitRole.Archer,
            2 => UnitRole.Cavalry,
            3 => UnitRole.Siege,
            4 => UnitRole.Mythic,
            _ => default
        };
    }

    private static int GetTeamIndex(Team team)
    {
        return team == Team.Left ? 0 : 1;
    }
}
