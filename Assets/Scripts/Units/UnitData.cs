using UnityEngine;

[CreateAssetMenu(menuName = "Clash of Pantheons/Unit Data")]
public class UnitData : ScriptableObject
{
    public string unitName;
    public int maxHealth = 100;
    public int damage = 10;
    public float attackRange = 1.2f;
    public float attackCooldown = 1f;
    public float moveSpeed = 2f;
    public int goldCost = 25;
}