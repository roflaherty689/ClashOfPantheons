using UnityEngine;

[CreateAssetMenu(fileName = "New Faction", menuName = "Clash of Pantheons/Faction")]
public class FactionData : ScriptableObject
{
    [Header("Faction Info")]
    public string factionName;

    [Header("Units")]
    public FactionUnitEntry[] units;

    public BaseUnit GetUnitPrefab(UnitRole role)
    {
        foreach (var entry in units)
        {
            if (entry.role == role)
                return entry.prefab;
        }

        Debug.LogError($"No unit prefab found for role {role} in faction {factionName}");
        return null;
    }
}