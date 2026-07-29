using System;

public interface IProductionSpawnContext
{
    int GetAvailableSpawnSlots(Team team);
    bool TryGetSpawnInterval(Team team, ProductionSlotId slotId, out float interval);
    bool TrySpawnUnit(Team team, ProductionSlotId slotId, int productionTier);
}

public sealed class ProductionStateController
{
    public const int SlotCount = 5;

    private readonly int[,] tiers = new int[2, SlotCount];
    private readonly float[,] timers = new float[2, SlotCount];
    private readonly int[] readySpawnIndices = new int[2];
    private readonly BaseUnit[] selectedMythics = new BaseUnit[2];

    public int GetTier(Team team, ProductionSlotId slotId)
    {
        int slotIndex = GetSlotIndex(slotId);
        return slotIndex < 0 ? 0 : tiers[GetTeamIndex(team), slotIndex];
    }

    public void SetTier(Team team, ProductionSlotId slotId, int tier)
    {
        int slotIndex = GetSlotIndex(slotId);
        if (slotIndex < 0) return;

        tiers[GetTeamIndex(team), slotIndex] = tier;
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

        for (int offset = 0; offset < SlotCount; offset++)
        {
            int slotIndex = (scanStartIndex + offset) % SlotCount;
            int productionTier = tiers[teamIndex, slotIndex];
            if (productionTier <= 0)
            {
                continue;
            }

            ProductionSlotId slotId = (ProductionSlotId)slotIndex;
            if (!spawnContext.TryGetSpawnInterval(team, slotId, out float interval))
            {
                continue;
            }

            timers[teamIndex, slotIndex] = Math.Min(timers[teamIndex, slotIndex], interval);
            if (timers[teamIndex, slotIndex] < interval)
            {
                continue;
            }

            if (availableSpawnSlots < 0)
            {
                availableSpawnSlots = spawnContext.GetAvailableSpawnSlots(team);
            }

            if (availableSpawnSlots <= 0 ||
                !spawnContext.TrySpawnUnit(team, slotId, productionTier))
            {
                continue;
            }

            timers[teamIndex, slotIndex] =
                Math.Max(0f, timers[teamIndex, slotIndex] - interval);
            availableSpawnSlots--;
            readySpawnIndices[teamIndex] = (slotIndex + 1) % SlotCount;
        }
    }

    public void ResetTimer(Team team, ProductionSlotId slotId)
    {
        int slotIndex = GetSlotIndex(slotId);
        if (slotIndex < 0) return;

        timers[GetTeamIndex(team), slotIndex] = 0f;
    }

    public static int GetSlotIndex(ProductionSlotId slotId)
    {
        int index = (int)slotId;
        return index >= 0 && index < SlotCount ? index : -1;
    }

    private void AdvanceUnlockedTimers(Team team, float deltaTime)
    {
        int teamIndex = GetTeamIndex(team);
        float safeDeltaTime = Math.Max(0f, deltaTime);

        for (int slotIndex = 0; slotIndex < SlotCount; slotIndex++)
        {
            if (tiers[teamIndex, slotIndex] <= 0)
            {
                timers[teamIndex, slotIndex] = 0f;
                continue;
            }

            timers[teamIndex, slotIndex] += safeDeltaTime;
        }
    }

    private static int GetTeamIndex(Team team)
    {
        return team == Team.Left ? 0 : 1;
    }
}
