using UnityEngine;

public static class FactionSelectionSession
{
    public static FactionData PlayerFaction { get; private set; }

    public static void SelectPlayerFaction(FactionData faction)
    {
        PlayerFaction = faction;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        PlayerFaction = null;
    }
}
