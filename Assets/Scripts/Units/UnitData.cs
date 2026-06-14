using UnityEngine;

[CreateAssetMenu(fileName = "UnitData", menuName = "ClashOfPantheons/Unit Data")]
public class UnitData : ScriptableObject
{
    public float maxHealth = 10;
    public float damage = 1;
    public float attackRange = 0.5f;
    public float attackSpeed = 1;
    public float moveSpeed = 2;
}