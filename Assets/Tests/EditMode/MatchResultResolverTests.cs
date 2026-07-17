using NUnit.Framework;

public class MatchResultResolverTests
{
    [TestCase(100f, 50f, Team.Left)]
    [TestCase(50f, 100f, Team.Right)]
    public void ResolveTimeout_HigherStrongholdHealthWins(
        float leftHealth,
        float rightHealth,
        Team expectedWinner)
    {
        MatchResult result = MatchResultResolver.ResolveTimeout(leftHealth, rightHealth, 999, 0);

        Assert.That(result.HasWinner, Is.True);
        Assert.That(result.Winner, Is.EqualTo(expectedWinner));
        Assert.That(result.Reason, Is.EqualTo(MatchEndReason.TimeoutHealth));
    }

    [TestCase(10, 20, Team.Left)]
    [TestCase(20, 10, Team.Right)]
    public void ResolveTimeout_EqualHealthLowerLostValueWins(
        int leftLostValue,
        int rightLostValue,
        Team expectedWinner)
    {
        MatchResult result = MatchResultResolver.ResolveTimeout(100f, 100f, leftLostValue, rightLostValue);

        Assert.That(result.HasWinner, Is.True);
        Assert.That(result.Winner, Is.EqualTo(expectedWinner));
        Assert.That(result.Reason, Is.EqualTo(MatchEndReason.TimeoutUnitLossValue));
    }

    [Test]
    public void ResolveTimeout_ExactEqualityDraws()
    {
        MatchResult result = MatchResultResolver.ResolveTimeout(100f, 100f, 20, 20);

        Assert.That(result.HasWinner, Is.False);
        Assert.That(result.Reason, Is.EqualTo(MatchEndReason.TimeoutDraw));
    }
}

public class MatchResultTextTests
{
    [TestCase(false, Team.Left, Team.Left, "DRAW")]
    [TestCase(true, Team.Left, Team.Left, "VICTORY")]
    [TestCase(true, Team.Right, Team.Left, "DEFEAT")]
    public void GetTitle_UsesPlayerRelativeResult(
        bool hasWinner,
        Team winner,
        Team playerTeam,
        string expected)
    {
        Assert.That(MatchResultText.GetTitle(hasWinner, winner, playerTeam), Is.EqualTo(expected));
    }

    [TestCase(MatchEndReason.StrongholdDestroyed, true, "ENEMY STRONGHOLD DESTROYED")]
    [TestCase(MatchEndReason.StrongholdDestroyed, false, "YOUR STRONGHOLD WAS DESTROYED")]
    [TestCase(MatchEndReason.TimeoutHealth, true, "TIME EXPIRED · YOUR STRONGHOLD HAD MORE HEALTH")]
    [TestCase(MatchEndReason.TimeoutHealth, false, "TIME EXPIRED · ENEMY STRONGHOLD HAD MORE HEALTH")]
    public void GetReason_UsesPlayerRelativeWording(
        MatchEndReason reason,
        bool playerWon,
        string expected)
    {
        Assert.That(MatchResultText.GetReason(reason, playerWon, 10, 20), Is.EqualTo(expected));
    }

    [Test]
    public void GetReason_LostValueIncludesBothTeamsValues()
    {
        Assert.That(
            MatchResultText.GetReason(MatchEndReason.TimeoutUnitLossValue, true, 10, 20),
            Is.EqualTo("TIME EXPIRED · LOSSES 10 vs 20 GOLD"));
    }

    [Test]
    public void GetReason_DrawIncludesTiedValue()
    {
        Assert.That(
            MatchResultText.GetReason(MatchEndReason.TimeoutDraw, false, 20, 20),
            Is.EqualTo("TIME EXPIRED · HEALTH AND LOSSES TIED AT 20 GOLD"));
    }

    [Test]
    public void GetReason_UnknownReasonUsesFallback()
    {
        Assert.That(
            MatchResultText.GetReason(MatchEndReason.None, false, 0, 0),
            Is.EqualTo("MATCH COMPLETE"));
    }
}
