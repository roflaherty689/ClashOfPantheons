using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Faction", menuName = "Clash of Pantheons/Faction")]
public class FactionData : ScriptableObject
{
    public const int StandardProductionSlotCount = 5;

    [Header("Faction Info")]
    [SerializeField] private string factionName;

    [Header("Ordered Standard Production Slots")]
    [FormerlySerializedAs("units")]
    [SerializeField] private FactionUnitEntry[] productionUnits;

    [Header("Worker")]
    [SerializeField] private WorkerUnit workerPrefab;

    [Header("Building Presentation")]
    [SerializeField] private Sprite castleSprite;
    [SerializeField] private Sprite handInSprite;

    public string FactionName => factionName;
    public WorkerUnit WorkerPrefab => workerPrefab;
    public Sprite CastleSprite => castleSprite;
    public Sprite HandInSprite => handInSprite;
    public bool HasBuildingPresentation => castleSprite != null && handInSprite != null;
    public IReadOnlyList<FactionUnitEntry> ProductionUnits =>
        productionUnits ?? System.Array.Empty<FactionUnitEntry>();

    public bool TryGetProductionEntry(ProductionSlotId slotId, out FactionUnitEntry entry)
    {
        int index = GetStandardSlotIndex(slotId);
        if (index >= 0 && productionUnits != null && index < productionUnits.Length)
        {
            entry = productionUnits[index];
            return entry != null;
        }

        entry = null;
        return false;
    }

    public bool TryGetUnitPrefab(ProductionSlotId slotId, out BaseUnit prefab)
    {
        if (TryGetProductionEntry(slotId, out FactionUnitEntry entry) && entry.Prefab != null)
        {
            prefab = entry.Prefab;
            return true;
        }

        prefab = null;
        return false;
    }

    public bool TryGetUnitData(ProductionSlotId slotId, out UnitData unitData)
    {
        if (TryGetUnitPrefab(slotId, out BaseUnit prefab) && prefab.UnitData != null)
        {
            unitData = prefab.UnitData;
            return true;
        }

        unitData = null;
        return false;
    }

    public bool TryValidateProductionUnits(out string error)
    {
        StringBuilder issues = new StringBuilder();
        if (productionUnits == null || productionUnits.Length != StandardProductionSlotCount)
        {
            int configuredCount = productionUnits?.Length ?? 0;
            issues.Append(
                $"expected exactly {StandardProductionSlotCount} ordered standard production entries, found {configuredCount}");
        }

        int countToValidate = productionUnits == null
            ? 0
            : Mathf.Min(productionUnits.Length, StandardProductionSlotCount);
        for (int index = 0; index < countToValidate; index++)
        {
            FactionUnitEntry entry = productionUnits[index];
            if (entry == null)
            {
                AppendIssue(issues, $"standard slot {index} has no entry");
                continue;
            }

            if (!System.Enum.IsDefined(typeof(UnitRole), entry.Role))
            {
                AppendIssue(issues, $"standard slot {index} has invalid role {entry.Role}");
            }

            if (entry.Prefab == null)
            {
                AppendIssue(issues, $"standard slot {index} has no prefab");
            }
            else if (entry.Prefab.UnitData == null)
            {
                AppendIssue(issues, $"standard slot {index} prefab '{entry.Prefab.name}' has no UnitData");
            }
        }

        error = issues.ToString();
        return error.Length == 0;
    }

    private static int GetStandardSlotIndex(ProductionSlotId slotId)
    {
        return slotId switch
        {
            ProductionSlotId.Standard0 => 0,
            ProductionSlotId.Standard1 => 1,
            ProductionSlotId.Standard2 => 2,
            ProductionSlotId.Standard3 => 3,
            ProductionSlotId.Standard4 => 4,
            _ => -1
        };
    }

    private static void AppendIssue(StringBuilder issues, string issue)
    {
        if (issues.Length > 0)
        {
            issues.Append("; ");
        }

        issues.Append(issue);
    }
}
