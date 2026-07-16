using UnityEngine;

[CreateAssetMenu(fileName = "UnitData", menuName = "Clash of Pantheons/Unit Data")]
public class UnitData : ScriptableObject
{
    public float maxHealth = 10;
    public float damage = 1;
    public float attackRange = 0.5f;
    public float attackSpeed = 1;
    public float moveSpeed = 2;
    public int cost = 50;

    [Header("Production")]
    [Min(0.1f)] public float spawnInterval = 3f;

    [Header("Damage Modifiers")]
    public float unitDamageMultiplier = 1f;
    public float buildingDamageMultiplier = 1f;

    public float GetDamageAgainst(TargetType targetType)
    {
        return targetType switch
        {
            TargetType.Unit => damage * unitDamageMultiplier,
            TargetType.Building => damage * buildingDamageMultiplier,
            _ => damage
        };
    }
}
