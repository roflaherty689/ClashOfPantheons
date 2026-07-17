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
