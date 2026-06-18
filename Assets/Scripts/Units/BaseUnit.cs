using UnityEngine;

public abstract class BaseUnit : MonoBehaviour, IDamageable
{
    [Header("Data")]
    [SerializeField] protected UnitData unitData;

    [Header("Health Bar")]
    [SerializeField] private HealthBar healthBarPrefab;
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0, 0.8f, 0);

    [Header("Visuals")]
    [SerializeField] protected Transform visualTransform;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Friendly Blocking")]
    [SerializeField] private float friendlyStopRange = 0.6f;

    [Header("Attack Animation")]
    [SerializeField] private float attackTiltAngle = 15f;
    [SerializeField] private float attackTiltDuration = 0.1f;

    private Team team;
    private Transform targetPoint;

    private BaseUnit currentEnemyTarget;
    private Base currentBaseTarget;

    private float currentHealth;
    private float attackTimer;

    private HealthBar healthBar;
    private GameManager gameManager;

    private float attackAnimationTimer;
    private bool isAttackAnimating;
    private Quaternion originalVisualRotation;

    public Team Team => team;

    [Header("Projectile")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float projectileTravelTime = 0.4f;
    [SerializeField] private float projectileArcHeight = 0.5f;
    [SerializeField] private bool usesProjectile;

    public Transform Transform => transform;

    public virtual void Initialize(Team team, Transform targetPoint)
    {
        gameManager = FindFirstObjectByType<GameManager>();

        this.team = team;
        this.targetPoint = targetPoint;

        currentHealth = unitData.maxHealth;
        originalVisualRotation = visualTransform.rotation;

        SpawnHealthBar();
        SetFacingDirection();
        // SetTeamColour();
    }

    private void SetFacingDirection()
    {
        float xScale = team == Team.Left ? 1f : -1f;

        visualTransform.localScale = new Vector3(
            xScale,
            1f,
            1f
        );
    }

    private void Update()
    {
        if (gameManager != null && gameManager.IsGameOver)
        {
            return;
        }

        UpdateAttackAnimation();

        FindClosestEnemy();

        if (currentEnemyTarget != null)
        {
            AttackEnemy();
        }
        else if (FindEnemyBaseInRange())
        {
            AttackBase();
        }
        else if (IsFriendlyUnitBlockingAhead())
        {
            return;
        }
        else
        {
            MoveTowardsTargetPoint();
        }
    }

    private void SpawnHealthBar()
    {
        healthBar = Instantiate(
            healthBarPrefab,
            transform.position + healthBarOffset,
            Quaternion.identity,
            transform
        );

        healthBar.SetHealth(currentHealth, unitData.maxHealth);
    }

    private void SetTeamColour()
    {
        spriteRenderer.color = team == Team.Left
            ? Color.red
            : Color.blue;
    }

    private void MoveTowardsTargetPoint()
    {
        if (targetPoint == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPoint.position,
            unitData.moveSpeed * Time.deltaTime
        );
    }

    private void FindClosestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            unitData.attackRange
        );

        currentEnemyTarget = null;

        float closestDistance = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            BaseUnit unit = hit.GetComponent<BaseUnit>();

            if (unit == null) continue;
            if (unit == this) continue;
            if (unit.Team == team) continue;

            float distance = Vector2.Distance(
                transform.position,
                unit.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                currentEnemyTarget = unit;
            }
        }
    }

    private bool FindEnemyBaseInRange()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            unitData.attackRange
        );

        currentBaseTarget = null;

        foreach (Collider2D hit in hits)
        {
            Base baseTarget = hit.GetComponent<Base>();

            if (baseTarget == null) continue;
            if (baseTarget.Team == team) continue;

            currentBaseTarget = baseTarget;
            return true;
        }

        return false;
    }

    private bool IsFriendlyUnitBlockingAhead()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            friendlyStopRange
        );

        foreach (Collider2D hit in hits)
        {
            BaseUnit unit = hit.GetComponent<BaseUnit>();

            if (unit == null) continue;
            if (unit == this) continue;
            if (unit.Team != team) continue;

            bool isAhead = team == Team.Left
                ? unit.transform.position.x > transform.position.x
                : unit.transform.position.x < transform.position.x;

            if (isAhead)
            {
                return true;
            }
        }

        return false;
    }

    private void AttackEnemy()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer < unitData.attackSpeed) return;

        attackTimer = 0f;

        if (usesProjectile)
        {
            FireProjectileAt(currentEnemyTarget);
        }
        else
        {
            currentEnemyTarget.TakeDamage(unitData.damage);
        }

        PlayAttackAnimation();
    }

    private void FireProjectileAt(IDamageable target)
    {
        if (projectilePrefab == null || projectileSpawnPoint == null)
        {
            target.TakeDamage(unitData.damage);
            return;
        }

        Projectile projectile = Instantiate(
            projectilePrefab,
            projectileSpawnPoint.position,
            Quaternion.identity
        );

        projectile.Initialize(
            target,
            unitData.damage,
            projectileTravelTime,
            projectileArcHeight
        );
    }

    private void AttackBase()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer < unitData.attackSpeed) return;

        attackTimer = 0f;

        if (usesProjectile)
        {
            FireProjectileAt(currentBaseTarget);
        }
        else
        {
            currentBaseTarget.TakeDamage(unitData.damage);
        }

        PlayAttackAnimation();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, unitData.maxHealth);
        }

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void PlayAttackAnimation()
    {
        isAttackAnimating = true;
        attackAnimationTimer = attackTiltDuration;

        float angle = team == Team.Left
            ? -attackTiltAngle
            : attackTiltAngle;

        visualTransform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void UpdateAttackAnimation()
    {
        if (!isAttackAnimating) return;

        attackAnimationTimer -= Time.deltaTime;

        if (attackAnimationTimer <= 0)
        {
            isAttackAnimating = false;
            visualTransform.rotation = originalVisualRotation;
        }
    }
}