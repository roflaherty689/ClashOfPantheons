using System;
using UnityEngine;

[Serializable]
public class FactionUnitEntry
{
    [SerializeField] private UnitRole role;
    [SerializeField] private BaseUnit prefab;

    public UnitRole Role => role;
    public BaseUnit Prefab => prefab;
}
