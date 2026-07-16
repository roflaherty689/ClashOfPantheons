using System.Collections;
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
    [SerializeField] private float attackTiltAngle = 15f;
    [SerializeField] private float attackTiltDuration = 0.1f;

    [Header("Projectile")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float projectileTravelTime = 0.4f;
    [SerializeField] private float projectileArcHeight = 0.5f;
    [SerializeField] private float projectileSpawnDelay = 0.15f;
    [SerializeField] private bool usesProjectile;

    [Header("Combat Positioning")]
    [SerializeField] private float minLaneY = -0.7f;
    [SerializeField] private float maxLaneY = 0.7f;

    [Header("Friendly Combat Separation")]
    [SerializeField] private float friendlySeparationRadius = 0.45f;
    [SerializeField] private float friendlySeparationStrength = 0.6f;
    [SerializeField] private float friendlySeparationDeadZone = 0.01f;

    private static readonly int IsMovingHash = Animator.StringToHash("isMoving");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    public TargetType TargetType => TargetType.Unit;
    public Team Team => team;
    public Transform Transform => transform;
    public UnitData UnitData => unitData;

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

        if (gameManager != null && gameManager.setTeamColour)
        {
            SetTeamColour();
        }
    }

    private void Update()
    {
        if (gameManager != null && gameManager.IsGameOver)
        {
            SetMovingAnimation(false);
            return;
        }

        UpdateAttackAnimation();

        currentTarget = FindTargetInRange();

        if (currentTarget != null)
        {
            HandleCombat(currentTarget);
            return;
        }

        MoveTowardsTargetPoint();
    }

    private void HandleCombat(IDamageable target)
    {
        SetMovingAnimation(false);

        ApplyFriendlyCombatSeparation();

        AttackTarget(target);
    }

    private void MoveTowardsTargetPoint()
    {
        if (targetPoint == null)
        {
            SetMovingAnimation(false);
            return;
        }

        SetMovingAnimation(true);

        float direction = team == Team.Left ? 1f : -1f;

        Vector3 nextPosition = transform.position;
        nextPosition.x += direction * unitData.moveSpeed * Time.deltaTime;

        transform.position = nextPosition;
    }

    private void ApplyFriendlyCombatSeparation()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            friendlySeparationRadius
        );

        float totalYPush = 0f;
        int pushCount = 0;

        foreach (Collider2D hit in hits)
        {
            BaseUnit otherUnit = hit.GetComponent<BaseUnit>();

            if (otherUnit == null) continue;
            if (otherUnit == this) continue;
            if (otherUnit.Team != team) continue;

            float yDifference = transform.position.y - otherUnit.transform.position.y;
            float distance = Mathf.Abs(yDifference);

            if (distance <= friendlySeparationDeadZone)
            {
                yDifference = Random.value > 0.5f ? 1f : -1f;
                distance = friendlySeparationDeadZone;
            }

            if (distance > friendlySeparationRadius)
                continue;

            float closeness = 1f - Mathf.Clamp01(distance / friendlySeparationRadius);
            totalYPush += Mathf.Sign(yDifference) * closeness;
            pushCount++;
        }

        if (pushCount == 0)
            return;

        float yPush = totalYPush / pushCount;

        Vector3 nextPosition = transform.position;
        nextPosition.y += yPush * friendlySeparationStrength * Time.deltaTime;
        nextPosition.y = Mathf.Clamp(nextPosition.y, minLaneY, maxLaneY);

        transform.position = nextPosition;
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

        if (closestEnemyUnit != null)
        {
            return closestEnemyUnit;
        }

        return enemyBase;
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
            StartCoroutine(DelayedProjectile(target, finalDamage));
        }
        else
        {
            target.TakeDamage(finalDamage);
        }
    }

    private IEnumerator DelayedProjectile(IDamageable target, float damage)
    {
        yield return new WaitForSeconds(projectileSpawnDelay);

        if (target == null)
        {
            yield break;
        }

        FireProjectileAt(target, damage);
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

    private void SetMovingAnimation(bool isMoving)
    {
        if (animator == null) return;

        animator.SetBool(IsMovingHash, isMoving);
    }

    protected virtual void PlayAttackAnimation()
    {
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

        if (attackAnimationTimer > 0f) return;

        isAttackAnimating = false;
        visualTransform.rotation = originalVisualRotation;
    }
}
