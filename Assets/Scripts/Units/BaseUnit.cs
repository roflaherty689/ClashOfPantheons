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

    [Header("Animation")]
    [SerializeField] protected Animator animator;
    
    private static readonly int IsMovingHash = Animator.StringToHash("isMoving");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    [Header("Friendly Blocking")]
    [SerializeField] private float friendlyStopRange = 0.6f;

    [Header("Attack Animation")]
    [SerializeField] private float attackTiltAngle = 15f;
    [SerializeField] private float attackTiltDuration = 0.1f;

    [Header("Projectile")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float projectileTravelTime = 0.4f;
    [SerializeField] private float projectileArcHeight = 0.5f;
    [SerializeField] private bool usesProjectile;

    public TargetType TargetType => TargetType.Unit;
    public Team Team => team;
    public Transform Transform => transform;

    private Team team;
    private Transform targetPoint;
    private IDamageable currentTarget;

    private float currentHealth;
    private float attackTimer;

    private HealthBar healthBar;
    private GameManager gameManager;

    private float attackAnimationTimer;
    private bool isAttackAnimating;
    private Quaternion originalVisualRotation;

    public virtual void Initialize(Team team, Transform targetPoint)
    {
        gameManager = FindFirstObjectByType<GameManager>();

        this.team = team;
        this.targetPoint = targetPoint;

        currentHealth = unitData.maxHealth;

        if (visualTransform != null)
        {
            originalVisualRotation = visualTransform.rotation;
        }

        SpawnHealthBar();
        SetFacingDirection();
        SetTeamColour();
    }

    private void Update()
    {
        if (gameManager != null && gameManager.IsGameOver)
        {
            return;
        }

        UpdateAttackAnimation();

        currentTarget = FindTargetInRange();

        if (currentTarget != null)
        {
            SetMovingAnimation(false);
            AttackTarget(currentTarget);
            return;
        }

        if (IsFriendlyUnitBlockingAhead())
        {
            SetMovingAnimation(false);
            return;
        }

        MoveTowardsTargetPoint();
    }

    private void SpawnHealthBar()
    {
        if (healthBarPrefab == null) return;

        healthBar = Instantiate(
            healthBarPrefab,
            transform.position + healthBarOffset,
            Quaternion.identity,
            transform
        );

        healthBar.SetHealth(currentHealth, unitData.maxHealth);
    }

    private void SetFacingDirection()
    {
        if (visualTransform == null) return;

        float xScale = team == Team.Left ? 1f : -1f;
        visualTransform.localScale = new Vector3(xScale, 1f, 1f);
    }

    private void SetTeamColour()
    {
        if (spriteRenderer == null) return;

        spriteRenderer.color = team == Team.Left
            ? Color.red
            : Color.blue;
    }

    private void MoveTowardsTargetPoint()
    {
        if (targetPoint == null) return;

        SetMovingAnimation(true);

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPoint.position,
            unitData.moveSpeed * Time.deltaTime
        );
    }

    private void SetMovingAnimation(bool isMoving)
    {
        if (animator == null) return;

        animator.SetBool(IsMovingHash, isMoving);
    }

    private IDamageable FindTargetInRange()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            unitData.attackRange
        );

        BaseUnit closestEnemyUnit = null;
        float closestUnitDistance = Mathf.Infinity;

        Base enemyBase = null;

        foreach (Collider2D hit in hits)
        {
            BaseUnit unit = hit.GetComponent<BaseUnit>();

            if (unit != null && unit != this && unit.Team != team)
            {
                float distance = Vector2.Distance(transform.position, unit.transform.position);

                if (distance < closestUnitDistance)
                {
                    closestUnitDistance = distance;
                    closestEnemyUnit = unit;
                }

                continue;
            }

            Base baseTarget = hit.GetComponent<Base>();

            if (baseTarget != null && baseTarget.Team != team)
            {
                enemyBase = baseTarget;
            }
        }

        // Prioritise units over base so units don't ignore defenders.
        if (closestEnemyUnit != null)
        {
            return closestEnemyUnit;
        }

        return enemyBase;
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

    private void AttackTarget(IDamageable target)
    {
        attackTimer += Time.deltaTime;

        float attackCooldown = 1f / Mathf.Max(unitData.attackSpeed, 0.01f);

        if (attackTimer < attackCooldown)
        {
            return;
        }

        attackTimer = 0f;

        float finalDamage = unitData.GetDamageAgainst(target.TargetType);

        PlayAttackAnimation();

        if (usesProjectile)
        {
            FireProjectileAt(target, finalDamage);
        }
        else
        {
            target.TakeDamage(finalDamage);
        }
    }

    private void FireProjectileAt(IDamageable target, float damage)
    {
        if (projectilePrefab == null || projectileSpawnPoint == null)
        {
            target.TakeDamage(damage);
            return;
        }

        Projectile projectile = Instantiate(
            projectilePrefab,
            projectileSpawnPoint.position,
            Quaternion.identity
        );

        projectile.Initialize(
            target,
            damage,
            projectileTravelTime,
            projectileArcHeight
        );
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
        Debug.Log($"{name} attack animation triggered");

        if (animator != null)
        {
            animator.ResetTrigger(AttackHash);
            animator.SetTrigger(AttackHash);
            return;
        }

        if (visualTransform == null) return;

        isAttackAnimating = true;
        attackAnimationTimer = attackTiltDuration;

        float angle = team == Team.Left
            ? -attackTiltAngle
            : attackTiltAngle;

        visualTransform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void UpdateAttackAnimation()
    {
        if (!isAttackAnimating || visualTransform == null) return;

        attackAnimationTimer -= Time.deltaTime;

        if (attackAnimationTimer > 0) return;

        isAttackAnimating = false;
        visualTransform.rotation = originalVisualRotation;
    }
}