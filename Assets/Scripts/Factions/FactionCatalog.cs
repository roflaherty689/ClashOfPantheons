using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FactionCatalog", menuName = "Clash of Pantheons/Faction Catalog")]
public sealed class FactionCatalog : ScriptableObject
{
    [SerializeField] private FactionData[] factions;

    public IReadOnlyList<FactionData> Factions => factions;
}
