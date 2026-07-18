using System;

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

    public float GetTimer(Team team, UnitRole role)
    {
        int roleIndex = GetRoleIndex(role);
        return roleIndex < 0 ? 0f : timers[GetTeamIndex(team), roleIndex];
    }

    public void ResetTimer(Team team, UnitRole role)
    {
        int roleIndex = GetRoleIndex(role);
        if (roleIndex < 0) return;

        timers[GetTeamIndex(team), roleIndex] = 0f;
    }

    public void AdvanceTimer(Team team, UnitRole role, float deltaTime)
    {
        int roleIndex = GetRoleIndex(role);
        if (roleIndex < 0) return;

        timers[GetTeamIndex(team), roleIndex] += Math.Max(0f, deltaTime);
    }

    public void ClampTimer(Team team, UnitRole role, float maximum)
    {
        int roleIndex = GetRoleIndex(role);
        if (roleIndex < 0) return;

        int teamIndex = GetTeamIndex(team);
        timers[teamIndex, roleIndex] = Math.Min(timers[teamIndex, roleIndex], maximum);
    }

    public void ConsumeTimer(Team team, UnitRole role, float duration)
    {
        int roleIndex = GetRoleIndex(role);
        if (roleIndex < 0) return;

        int teamIndex = GetTeamIndex(team);
        timers[teamIndex, roleIndex] = Math.Max(0f, timers[teamIndex, roleIndex] - duration);
    }

    public int GetReadySpawnIndex(Team team)
    {
        return readySpawnIndices[GetTeamIndex(team)];
    }

    public void SetReadySpawnIndex(Team team, int index)
    {
        readySpawnIndices[GetTeamIndex(team)] = index;
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

    private static int GetTeamIndex(Team team)
    {
        return team == Team.Left ? 0 : 1;
    }
}
