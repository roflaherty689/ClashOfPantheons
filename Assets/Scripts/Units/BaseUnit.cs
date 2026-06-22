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
    [SerializeField] private float attackPositionTolerance = 0.05f;
    [SerializeField] private float minLaneY = -0.7f;
    [SerializeField] private float maxLaneY = 0.7f;

    [Header("Friendly Blocking")]
    [SerializeField] private float friendlyStopRange = 0.6f;
    [SerializeField] private float blockedPauseDuration = 0.2f;

    private static readonly int IsMovingHash = Animator.StringToHash("isMoving");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private static readonly float[] AttackSlotOffsets =
    {
        0f,
        0.35f,
        -0.35f,
        0.7f,
        -0.7f
    };
    private bool wasInCombat;
    private float assignedAttackY;

    [SerializeField] private float combatSpreadStartX = -1f;

    public TargetType TargetType => TargetType.Unit;
    public Team Team => team;
    public Transform Transform => transform;

    private Team team;
    private Transform targetPoint;
    private IDamageable currentTarget;
    private float currentHealth;
    private float attackTimer;
    private float blockedPauseTimer;

    private HealthBar healthBar;
    private GameManager gameManager;

    private float attackAnimationTimer;
    private bool isAttackAnimating;
    private Quaternion originalVisualRotation;

    private float assignedAttackYOffset;
    private bool hasAssignedAttackSlot;

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

        bool isInCombat = currentTarget != null;

        if (isInCombat && !wasInCombat)
        {
            AssignRandomAttackSlot();
        }

        if (isInCombat)
        {
            HandleCombatMovement(currentTarget);
            wasInCombat = true;
            return;
        }

        ResetAttackSlot();
        wasInCombat = false;

        // if (IsFriendlyUnitBlockingAhead())
        // {
        //     blockedPauseTimer = blockedPauseDuration;
        //     SetMovingAnimation(false);
        //     return;
        // }

        if (blockedPauseTimer > 0f)
        {
            blockedPauseTimer -= Time.deltaTime;
            SetMovingAnimation(false);
            return;
        }

        MoveTowardsTargetPoint();
    }

    private void AssignRandomAttackSlot()
    {
        if (hasAssignedAttackSlot)
            return;

        bool canSpread =
            team == Team.Left
                ? transform.position.x > combatSpreadStartX
                : transform.position.x < -combatSpreadStartX;

        if (!canSpread)
        {
            assignedAttackY = 0f;
        }
        else
        {
            int index = Random.Range(0, AttackSlotOffsets.Length);

            assignedAttackY = Mathf.Clamp(
                transform.position.y + AttackSlotOffsets[index],
                minLaneY,
                maxLaneY
            );
        }

        hasAssignedAttackSlot = true;
    }

    private void HandleCombatMovement(IDamageable target)
    {
        Vector3 attackPosition = GetAttackPosition(target);

        float distanceToAttackPosition = Vector2.Distance(
            transform.position,
            attackPosition
        );

        if (distanceToAttackPosition > attackPositionTolerance)
        {
            SetMovingAnimation(true);

            transform.position = Vector3.MoveTowards(
                transform.position,
                attackPosition,
                unitData.moveSpeed * Time.deltaTime
            );

            return;
        }

        SetMovingAnimation(false);
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

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPoint.position,
            unitData.moveSpeed * Time.deltaTime
        );
    }

    private Vector3 GetAttackPosition(IDamageable target)
    {
        float directionToEnemy = team == Team.Left ? 1f : -1f;

        float idealStopX = target.Transform.position.x - directionToEnemy * unitData.attackRange;

        float stopX = team == Team.Left
            ? Mathf.Max(transform.position.x, idealStopX)
            : Mathf.Min(transform.position.x, idealStopX);

        if (!hasAssignedAttackSlot)
        {
            AssignRandomAttackSlot();
        }

        return new Vector3(stopX, assignedAttackY, transform.position.z);
    }

    private float GetSlotOffsetForThisUnit()
    {
        if (!hasAssignedAttackSlot)
        {
            int index = Random.Range(0, AttackSlotOffsets.Length);
            assignedAttackYOffset = AttackSlotOffsets[index];
            hasAssignedAttackSlot = true;
        }

        return assignedAttackYOffset;
    }

    private void ResetAttackSlot()
    {
        hasAssignedAttackSlot = false;
        assignedAttackYOffset = 0f;
        assignedAttackY = 0f;
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

        if (attackAnimationTimer > 0f) return;

        isAttackAnimating = false;
        visualTransform.rotation = originalVisualRotation;
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
}