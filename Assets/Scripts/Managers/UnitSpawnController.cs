using UnityEngine;
using Unity.Profiling;

public sealed class UnitSpawnController : IProductionSpawnContext
{
    private static readonly ProfilerMarker CapacityScanMarker = new("Clash.Production.CapacityScan");
    private static readonly ProductionSlotId[] Slots =
    {
        ProductionSlotId.Standard0,
        ProductionSlotId.Standard1,
        ProductionSlotId.Standard2,
        ProductionSlotId.Standard3,
        ProductionSlotId.Standard4,
        ProductionSlotId.Mythic
    };

    private readonly FactionData leftFaction;
    private readonly FactionData rightFaction;
    private readonly Transform leftSpawnPoint;
    private readonly Transform rightSpawnPoint;
    private readonly Transform leftTargetPoint;
    private readonly Transform rightTargetPoint;
    private readonly ProductionStateController productionState;
    private readonly int maxUnitsPerTeam;
    private readonly bool randomiseSpawns;

    private float globalSpawnTimer;
    private int leftSpawnIndex;
    private int rightSpawnIndex;

    public UnitSpawnController(
        FactionData leftFaction,
        FactionData rightFaction,
        Transform leftSpawnPoint,
        Transform rightSpawnPoint,
        Transform leftTargetPoint,
        Transform rightTargetPoint,
        ProductionStateController productionState,
        int maxUnitsPerTeam,
        bool randomiseSpawns)
    {
        this.leftFaction = leftFaction;
        this.rightFaction = rightFaction;
        this.leftSpawnPoint = leftSpawnPoint;
        this.rightSpawnPoint = rightSpawnPoint;
        this.leftTargetPoint = leftTargetPoint;
        this.rightTargetPoint = rightTargetPoint;
        this.productionState = productionState;
        this.maxUnitsPerTeam = maxUnitsPerTeam;
        this.randomiseSpawns = randomiseSpawns;
    }

    public void UpdateGlobalSpawns(float deltaTime, float interval)
    {
        float safeInterval = Mathf.Max(0.1f, interval);
        globalSpawnTimer += Mathf.Max(0f, deltaTime);
        if (globalSpawnTimer < safeInterval) return;

        globalSpawnTimer -= safeInterval;
        TrySpawnGlobalUnit(Team.Left);
        TrySpawnGlobalUnit(Team.Right);
    }

    public int GetAvailableSpawnSlots(Team team)
    {
        using ProfilerMarker.AutoScope _ = CapacityScanMarker.Auto();
        int teamUnitCount = 0;
        foreach (BaseUnit unit in Object.FindObjectsByType<BaseUnit>())
        {
            if (unit.Team == team)
            {
                teamUnitCount++;
            }
        }

        return Mathf.Max(0, maxUnitsPerTeam - teamUnitCount);
    }

    public bool TryGetSpawnInterval(Team team, ProductionSlotId slotId, out float interval)
    {
        if (TryGetProductionData(team, slotId, out UnitData data))
        {
            interval = data.SpawnInterval;
            return true;
        }

        interval = 0f;
        return false;
    }

    public bool TrySpawnUnit(Team team, ProductionSlotId slotId, int productionTier)
    {
        FactionData faction = GetFaction(team);
        Transform spawnPoint = team == Team.Left ? leftSpawnPoint : rightSpawnPoint;
        Transform targetPoint = team == Team.Left ? leftTargetPoint : rightTargetPoint;
        if (faction == null || spawnPoint == null || targetPoint == null)
        {
            return false;
        }

        BaseUnit prefab;
        UnitRole role;
        if (slotId == ProductionSlotId.Mythic)
        {
            prefab = productionState.GetSelectedMythic(team);
            role = UnitRole.Mythic;
        }
        else if (faction.TryGetProductionEntry(slotId, out FactionUnitEntry entry))
        {
            prefab = entry.Prefab;
            role = entry.Role;
        }
        else
        {
            return false;
        }

        if (prefab == null || prefab.UnitData == null)
        {
            return false;
        }

        BaseUnit instance = Object.Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        instance.Initialize(team, targetPoint, role, productionTier);
        SoundManager.PlayAt(SoundCue.UnitSpawn, spawnPoint.position);
        return true;
    }

    private void TrySpawnGlobalUnit(Team team)
    {
        if (GetAvailableSpawnSlots(team) <= 0) return;

        if (randomiseSpawns)
        {
            if (TrySelectWeightedSlot(team, out ProductionSlotId selectedSlot))
            {
                TrySpawnUnit(team, selectedSlot, 1);
            }

            return;
        }

        int scanStartIndex = team == Team.Left ? leftSpawnIndex : rightSpawnIndex;
        for (int offset = 0; offset < Slots.Length; offset++)
        {
            int slotIndex = (scanStartIndex + offset) % Slots.Length;
            if (!TrySpawnUnit(team, Slots[slotIndex], 1))
            {
                continue;
            }

            int nextIndex = (slotIndex + 1) % Slots.Length;
            if (team == Team.Left)
            {
                leftSpawnIndex = nextIndex;
            }
            else
            {
                rightSpawnIndex = nextIndex;
            }

            return;
        }
    }

    private bool TrySelectWeightedSlot(Team team, out ProductionSlotId selectedSlot)
    {
        float totalWeight = 0f;
        foreach (ProductionSlotId slotId in Slots)
        {
            if (TryGetProductionData(team, slotId, out UnitData data))
            {
                totalWeight += GetSpawnWeight(data);
            }
        }

        if (totalWeight <= 0f)
        {
            selectedSlot = default;
            return false;
        }

        selectedSlot = default;
        float roll = Random.Range(0f, totalWeight);
        foreach (ProductionSlotId slotId in Slots)
        {
            if (!TryGetProductionData(team, slotId, out UnitData data)) continue;

            selectedSlot = slotId;
            roll -= GetSpawnWeight(data);
            if (roll <= 0f) return true;
        }

        return true;
    }

    private FactionData GetFaction(Team team)
    {
        return team == Team.Left ? leftFaction : rightFaction;
    }

    private bool TryGetProductionData(
        Team team,
        ProductionSlotId slotId,
        out UnitData data)
    {
        if (slotId == ProductionSlotId.Mythic)
        {
            BaseUnit selectedMythic = productionState.GetSelectedMythic(team);
            if (selectedMythic != null && selectedMythic.UnitData != null)
            {
                data = selectedMythic.UnitData;
                return true;
            }

            data = null;
            return false;
        }

        FactionData faction = GetFaction(team);
        if (faction == null)
        {
            data = null;
            return false;
        }

        return faction.TryGetUnitData(slotId, out data);
    }

    private static float GetSpawnWeight(UnitData data)
    {
        return 1f / Mathf.Max(1, data.Cost);
    }
}
