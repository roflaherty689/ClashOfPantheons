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
    [TestCase(-1f, "0:00")]
    [TestCase(0f, "0:00")]
    [TestCase(0.01f, "0:01")]
    [TestCase(59f, "0:59")]
    [TestCase(59.01f, "1:00")]
    [TestCase(300f, "5:00")]
    public void GetCountdown_ClampsAndRoundsUpLikeBattleHud(float seconds, string expected)
    {
        Assert.That(MatchResultText.GetCountdown(seconds), Is.EqualTo(expected));
    }

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

public class ProductionTierRulesTests
{
    [TestCase(-1, 1f)]
    [TestCase(0, 1f)]
    [TestCase(1, 1f)]
    [TestCase(2, 1.25f)]
    [TestCase(3, 1.5f)]
    [TestCase(4, 1.5f)]
    public void GetStatMultiplier_UsesAcceptedStarCurve(int tier, float expected)
    {
        Assert.That(ProductionTierRules.GetStatMultiplier(tier), Is.EqualTo(expected));
    }

    [TestCase(0, 1)]
    [TestCase(1, 2)]
    [TestCase(2, 3)]
    public void TryAdvance_NormalProductionAdvancesOneTier(int currentTier, int expectedTier)
    {
        bool succeeded = ProductionTierRules.TryAdvance(currentTier, false, out int nextTier);

        Assert.That(succeeded, Is.True);
        Assert.That(nextTier, Is.EqualTo(expectedTier));
    }

    [Test]
    public void TryAdvance_MaximumTierIsRejectedWithoutChangingTier()
    {
        bool succeeded = ProductionTierRules.TryAdvance(3, false, out int nextTier);

        Assert.That(succeeded, Is.False);
        Assert.That(nextTier, Is.EqualTo(3));
    }

    [Test]
    public void TryAdvance_SelectionRoleCannotUseNormalInitialUnlock()
    {
        bool succeeded = ProductionTierRules.TryAdvance(0, true, out int nextTier);

        Assert.That(succeeded, Is.False);
        Assert.That(nextTier, Is.Zero);
    }

    [TestCase(1, 2)]
    [TestCase(2, 3)]
    public void TryAdvance_SelectedRoleCanUpgradeNormally(int currentTier, int expectedTier)
    {
        bool succeeded = ProductionTierRules.TryAdvance(currentTier, true, out int nextTier);

        Assert.That(succeeded, Is.True);
        Assert.That(nextTier, Is.EqualTo(expectedTier));
    }

    [Test]
    public void TryUnlockSelectedOption_LockedRoleBecomesTierOne()
    {
        bool succeeded = ProductionTierRules.TryUnlockSelectedOption(0, out int nextTier);

        Assert.That(succeeded, Is.True);
        Assert.That(nextTier, Is.EqualTo(1));
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void TryUnlockSelectedOption_AlreadyUnlockedRoleIsRejected(int currentTier)
    {
        bool succeeded = ProductionTierRules.TryUnlockSelectedOption(currentTier, out int nextTier);

        Assert.That(succeeded, Is.False);
        Assert.That(nextTier, Is.EqualTo(currentTier));
    }
}
