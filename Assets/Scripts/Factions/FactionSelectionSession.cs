using UnityEngine;

public static class FactionSelectionSession
{
    public static FactionData PlayerFaction { get; private set; }
    public static FactionData EnemyFaction { get; private set; }
    public static GameDifficulty Difficulty { get; private set; } = GameDifficulty.Easy;

    public static void SelectPlayerFaction(FactionData faction)
    {
        PlayerFaction = faction;
    }

    public static void ConfigureMatch(FactionData playerFaction, FactionData enemyFaction, GameDifficulty difficulty)
    {
        PlayerFaction = playerFaction;
        EnemyFaction = enemyFaction;
        Difficulty = difficulty;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        PlayerFaction = null;
        EnemyFaction = null;
        Difficulty = GameDifficulty.Easy;
    }
}
