public enum MatchEndReason
{
    None,
    StrongholdDestroyed,
    TimeoutHealth,
    TimeoutUnitLossValue,
    TimeoutDraw
}

public readonly struct MatchResult
{
    private MatchResult(bool hasWinner, Team winner, MatchEndReason reason)
    {
        HasWinner = hasWinner;
        Winner = winner;
        Reason = reason;
    }

    public bool HasWinner { get; }
    public Team Winner { get; }
    public MatchEndReason Reason { get; }

    public static MatchResult Win(Team winner, MatchEndReason reason)
    {
        return new MatchResult(true, winner, reason);
    }

    public static MatchResult Draw(MatchEndReason reason)
    {
        return new MatchResult(false, default, reason);
    }
}

public static class MatchResultResolver
{
    public static MatchResult ResolveTimeout(
        float leftStrongholdHealth,
        float rightStrongholdHealth,
        int leftLostUnitValue,
        int rightLostUnitValue)
    {
        int healthComparison = leftStrongholdHealth.CompareTo(rightStrongholdHealth);
        if (healthComparison != 0)
        {
            return MatchResult.Win(
                healthComparison > 0 ? Team.Left : Team.Right,
                MatchEndReason.TimeoutHealth);
        }

        if (leftLostUnitValue != rightLostUnitValue)
        {
            return MatchResult.Win(
                leftLostUnitValue < rightLostUnitValue ? Team.Left : Team.Right,
                MatchEndReason.TimeoutUnitLossValue);
        }

        return MatchResult.Draw(MatchEndReason.TimeoutDraw);
    }
}

public static class MatchResultText
{
    public static string GetTitle(bool hasWinner, Team winningTeam, Team playerTeam)
    {
        if (!hasWinner)
        {
            return "DRAW";
        }

        return winningTeam == playerTeam ? "VICTORY" : "DEFEAT";
    }

    public static string GetReason(
        MatchEndReason reason,
        bool playerWon,
        int playerLostUnitValue,
        int enemyLostUnitValue)
    {
        return reason switch
        {
            MatchEndReason.StrongholdDestroyed => playerWon
                ? "ENEMY STRONGHOLD DESTROYED"
                : "YOUR STRONGHOLD WAS DESTROYED",
            MatchEndReason.TimeoutHealth => playerWon
                ? "TIME EXPIRED · YOUR STRONGHOLD HAD MORE HEALTH"
                : "TIME EXPIRED · ENEMY STRONGHOLD HAD MORE HEALTH",
            MatchEndReason.TimeoutUnitLossValue =>
                $"TIME EXPIRED · LOSSES {playerLostUnitValue} vs {enemyLostUnitValue} GOLD",
            MatchEndReason.TimeoutDraw =>
                $"TIME EXPIRED · HEALTH AND LOSSES TIED AT {playerLostUnitValue} GOLD",
            _ => "MATCH COMPLETE"
        };
    }
}
