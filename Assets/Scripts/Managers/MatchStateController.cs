using System;

public sealed class MatchStateController
{
    private const int TrackedUnitRoleCount = 5;

    private readonly int[,] unitLossCounts = new int[2, TrackedUnitRoleCount];
    private readonly int[] totalUnitLossValues = new int[2];

    private bool isGameOver;
    private bool hasWinner;
    private Team winningTeam;
    private MatchEndReason endReason;
    private float timeRemaining;

    public bool IsGameOver => isGameOver;
    public bool HasWinner => hasWinner;
    public Team WinningTeam => winningTeam;
    public MatchEndReason EndReason => endReason;
    public float TimeRemaining => Math.Max(0f, timeRemaining);

    public void Start(float durationSeconds)
    {
        timeRemaining = Math.Max(1f, durationSeconds);
    }

    public bool AdvanceClock(float deltaTime)
    {
        if (isGameOver) return false;

        timeRemaining = Math.Max(0f, timeRemaining - Math.Max(0f, deltaTime));
        return timeRemaining <= 0f;
    }

    public void RegisterUnitDeath(Team team, UnitRole role, int unitCost)
    {
        if (isGameOver) return;

        int roleIndex = GetUnitRoleIndex(role);
        if (roleIndex < 0) return;

        int teamIndex = GetTeamIndex(team);
        unitLossCounts[teamIndex, roleIndex]++;
        totalUnitLossValues[teamIndex] += Math.Max(0, unitCost);
    }

    public int GetUnitLossCount(Team team, UnitRole role)
    {
        int roleIndex = GetUnitRoleIndex(role);
        return roleIndex < 0 ? 0 : unitLossCounts[GetTeamIndex(team), roleIndex];
    }

    public int GetTotalUnitLossValue(Team team)
    {
        return totalUnitLossValues[GetTeamIndex(team)];
    }

    public void CompleteWithWinner(Team winner, MatchEndReason reason)
    {
        if (isGameOver) return;

        isGameOver = true;
        hasWinner = true;
        winningTeam = winner;
        endReason = reason;
    }

    public void CompleteDraw(MatchEndReason reason)
    {
        if (isGameOver) return;

        isGameOver = true;
        hasWinner = false;
        endReason = reason;
    }

    private static int GetTeamIndex(Team team)
    {
        return team == Team.Left ? 0 : 1;
    }

    private static int GetUnitRoleIndex(UnitRole role)
    {
        int index = (int)role;
        return index >= 0 && index < TrackedUnitRoleCount ? index : -1;
    }
}
