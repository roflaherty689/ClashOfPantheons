using UnityEngine;

[CreateAssetMenu(fileName = "UnitData", menuName = "Clash of Pantheons/Unit Data")]
public class UnitData : ScriptableObject
{
    [Header("Core Stats")]
    [SerializeField, Min(0.01f)] private float maxHealth = 10f;
    [SerializeField, Min(0f)] private float damage = 1f;
    [SerializeField, Min(0f)] private float attackRange = 0.5f;
    [SerializeField, Min(0.01f)] private float attackSpeed = 1f;
    [SerializeField, Min(0f)] private float moveSpeed = 2f;
    [SerializeField, Min(0)] private int cost = 50;

    [Header("Production")]
    [SerializeField, Min(0.1f)] private float spawnInterval = 3f;

    [Header("Damage Modifiers")]
    [SerializeField, Min(0f)] private float unitDamageMultiplier = 1f;
    [SerializeField, Min(0f)] private float buildingDamageMultiplier = 1f;

    public float MaxHealth => Mathf.Max(0.01f, maxHealth);
    public float AttackRange => Mathf.Max(0f, attackRange);
    public float AttackSpeed => Mathf.Max(0.01f, attackSpeed);
    public float MoveSpeed => Mathf.Max(0f, moveSpeed);
    public int Cost => Mathf.Max(0, cost);
    public float SpawnInterval => Mathf.Max(0.1f, spawnInterval);

    public float GetDamageAgainst(TargetType targetType)
    {
        return targetType switch
        {
            TargetType.Unit => damage * Mathf.Max(0f, unitDamageMultiplier),
            TargetType.Building => damage * Mathf.Max(0f, buildingDamageMultiplier),
            _ => damage
        };
    }
}
