using UnityEngine;

public class Unit : MonoBehaviour
{
    public Team team;
    public UnitData data;

    private int currentHealth;
    private float attackTimer;

    private void Awake()
    {
        currentHealth = data.maxHealth;
    }

    private void Update()
    {
        attackTimer -= Time.deltaTime;

        Unit enemyUnit = FindClosestEnemyUnit();

        if (enemyUnit != null)
        {
            float distance = Vector2.Distance(transform.position, enemyUnit.transform.position);

            if (distance <= data.attackRange)
            {
                TryAttack(enemyUnit);
                return;
            }
        }

        BaseBuilding enemyBase = FindEnemyBase();

        if (enemyBase != null)
        {
            float distanceToBase = Vector2.Distance(transform.position, enemyBase.transform.position);

            if (distanceToBase <= data.attackRange)
            {
                TryAttackBase(enemyBase);
                return;
            }
        }

        MoveForward();
    }

    private void MoveForward()
    {
        float direction = team == Team.Left ? 1f : -1f;
        transform.position += Vector3.right * direction * data.moveSpeed * Time.deltaTime;
    }

    private void TryAttack(Unit target)
    {
        if (attackTimer > 0) return;

        target.TakeDamage(data.damage);
        attackTimer = data.attackCooldown;
    }

    private void TryAttackBase(BaseBuilding target)
    {
        if (attackTimer > 0) return;

        target.TakeDamage(data.damage);
        attackTimer = data.attackCooldown;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    private Unit FindClosestEnemyUnit()
    {
        Unit[] units = FindObjectsByType<Unit>(FindObjectsSortMode.None);

        Unit closest = null;
        float closestDistance = float.MaxValue;

        foreach (Unit unit in units)
        {
            if (unit.team == team) continue;

            float distance = Vector2.Distance(transform.position, unit.transform.position);

            if (distance < closestDistance)
            {
                closest = unit;
                closestDistance = distance;
            }
        }

        return closest;
    }

    private BaseBuilding FindEnemyBase()
    {
        BaseBuilding[] bases = FindObjectsByType<BaseBuilding>(FindObjectsSortMode.None);

        foreach (BaseBuilding baseBuilding in bases)
        {
            if (baseBuilding.team != team)
            {
                return baseBuilding;
            }
        }

        return null;
    }
}