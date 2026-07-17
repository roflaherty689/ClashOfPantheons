using UnityEngine;

[CreateAssetMenu(fileName = "New Faction", menuName = "Clash of Pantheons/Faction")]
public class FactionData : ScriptableObject
{
    [Header("Faction Info")]
    [SerializeField] private string factionName;

    [Header("Units")]
    [SerializeField] private FactionUnitEntry[] units;

    [Header("Building Presentation")]
    [SerializeField] private Sprite castleSprite;
    [SerializeField] private Sprite handInSprite;

    public string FactionName => factionName;
    public Sprite CastleSprite => castleSprite;
    public Sprite HandInSprite => handInSprite;
    public bool HasBuildingPresentation => castleSprite != null && handInSprite != null;

    public BaseUnit GetUnitPrefab(UnitRole role)
    {
        if (TryGetUnitPrefab(role, out BaseUnit prefab))
        {
            return prefab;
        }

        Debug.LogError($"No unit prefab found for role {role} in faction {factionName}");
        return null;
    }

    public bool TryGetUnitPrefab(UnitRole role, out BaseUnit prefab)
    {
        if (units != null)
        {
            foreach (FactionUnitEntry entry in units)
            {
                if (entry != null && entry.Role == role && entry.Prefab != null)
                {
                    prefab = entry.Prefab;
                    return true;
                }
            }
        }

        prefab = null;
        return false;
    }

    public bool TryGetUnitData(UnitRole role, out UnitData unitData)
    {
        if (TryGetUnitPrefab(role, out BaseUnit prefab) && prefab.UnitData != null)
        {
            unitData = prefab.UnitData;
            return true;
        }

        unitData = null;
        return false;
    }
}
