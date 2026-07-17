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
    public int ProductionTier => productionTier;
    public float CurrentHealth => currentHealth;
    public float MaximumHealth => maximumHealth;
    public bool IsDead => isDead;
    public bool IsInCombat { get; private set; }

    protected float StatMultiplier => statMultiplier;
    protected virtual bool CanAttack => true;
    protected virtual float TargetPointStoppingDistance => 0f;
    protected virtual bool SeparateFriendliesWhileMoving => false;

    private Team team;
    private UnitRole role;
    private Transform targetPoint;
    private float currentHealth;
    private float attackTimer;
    private bool isDead;
    private int productionTier = 1;
    private float statMultiplier = 1f;
    private float maximumHealth;

    private HealthBar healthBar;
    private GameManager gameManager;

    private float attackAnimationTimer;
    private bool isAttackAnimating;
    private Quaternion originalVisualRotation;
    private Vector3 originalVisualScale;
    private Vector3 originalVisualPosition;
    private Coroutine recoilCoroutine;

    public virtual void Initialize(Team team, Transform targetPoint, UnitRole role, int tier = 1)
    {
        gameManager = FindAnyObjectByType<GameManager>();

        this.team = team;
        this.targetPoint = targetPoint;
        this.role = role;
        productionTier = Mathf.Clamp(tier, 1, GameManager.MaximumProductionTier);
        statMultiplier = productionTier switch
        {
            2 => 1.5f,
            3 => 2f,
            _ => 1f
        };

        if (unitData == null)
        {
            Debug.LogError($"{name}: UnitData is not assigned.", this);
            enabled = false;
            return;
        }

        maximumHealth = unitData.MaxHealth * statMultiplier;
        currentHealth = maximumHealth;

        if (visualTransform != null)
        {
            originalVisualRotation = visualTransform.rotation;
            originalVisualScale = visualTransform.localScale;
            originalVisualPosition = visualTransform.localPosition;
        }

        SpawnHealthBar();
        SetFacingDirection();

        if (gameManager != null && gameManager.SetTeamColour)
        {
            SetTeamColour();
        }
    }

    private void Update()
    {
        UpdateAttackAnimation();

        if (gameManager != null && gameManager.IsGameOver)
        {
            IsInCombat = false;
            SetMovingAnimation(false);
            return;
        }

        if (TryHandleSpecialAction())
        {
            IsInCombat = false;
            return;
        }

        IDamageable currentTarget = CanAttack ? FindTargetInRange() : null;
        IsInCombat = currentTarget != null;

        if (currentTarget != null)
        {
            HandleCombat(currentTarget);
            return;
        }

        MoveTowardsTargetPoint();
    }

    protected virtual bool TryHandleSpecialAction()
    {
        return false;
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

        float distanceToTarget = Mathf.Abs(targetPoint.position.x - transform.position.x);
        if (distanceToTarget <= Mathf.Max(0f, TargetPointStoppingDistance))
        {
            SetMovingAnimation(false);
            if (SeparateFriendliesWhileMoving)
            {
                ApplyFriendlyCombatSeparation();
            }

            return;
        }

        SetMovingAnimation(true);

        if (SeparateFriendliesWhileMoving)
        {
            ApplyFriendlyCombatSeparation();
        }

        float direction = team == Team.Left ? 1f : -1f;
        float stoppingDistance = Mathf.Max(0f, TargetPointStoppingDistance);
        float movementDistance = unitData.MoveSpeed * statMultiplier * Time.deltaTime;
        movementDistance = Mathf.Min(movementDistance, distanceToTarget - stoppingDistance);

        Vector3 nextPosition = transform.position;
        nextPosition.x += direction * movementDistance;

        transform.position = nextPosition;
    }

    protected void ApplyFriendlyCombatSeparation()
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
            unitData.AttackRange * statMultiplier
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
        if (!DamageableUtility.IsValid(target)) return;

        attackTimer += Time.deltaTime;

        float attackCooldown = 1f / (unitData.AttackSpeed * statMultiplier);

        if (attackTimer < attackCooldown)
        {
            return;
        }

        attackTimer -= attackCooldown;

        float finalDamage = unitData.GetDamageAgainst(target.TargetType) * statMultiplier;

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

        if (!DamageableUtility.IsValid(target) ||
            (gameManager != null && gameManager.IsGameOver))
        {
            yield break;
        }

        FireProjectileAt(target, damage);
    }

    private void FireProjectileAt(IDamageable target, float damage)
    {
        if (!DamageableUtility.IsValid(target)) return;

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
        if (isDead || damage <= 0f) return;

        currentHealth = Mathf.Max(0f, currentHealth - damage);

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maximumHealth);
        }

        if (currentHealth > 0f) return;

        isDead = true;
        gameManager?.RegisterUnitDeath(team, role, unitData.Cost);
        Destroy(gameObject);
    }

    public float ReceiveHealing(float amount)
    {
        if (isDead || amount <= 0f || currentHealth >= maximumHealth)
        {
            return 0f;
        }

        float previousHealth = currentHealth;
        currentHealth = Mathf.Min(maximumHealth, currentHealth + amount);

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maximumHealth);
        }

        return currentHealth - previousHealth;
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

        healthBar.SetHealth(currentHealth, maximumHealth);
    }

    private void SetFacingDirection()
    {
        if (visualTransform == null) return;

        float xDirection = team == Team.Left ? 1f : -1f;
        Vector3 facingScale = originalVisualScale;
        facingScale.x = Mathf.Abs(originalVisualScale.x) * xDirection;
        visualTransform.localScale = facingScale;
    }

    private void SetTeamColour()
    {
        if (spriteRenderer == null) return;

        spriteRenderer.color = team == Team.Left
            ? Color.red
            : Color.blue;
    }

    protected void SetMovingAnimation(bool isMoving)
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

    protected void PlayRecoilAnimation(float distance = 0.15f, float duration = 0.05f)
    {
        if (visualTransform == null) return;

        if (recoilCoroutine != null)
        {
            StopCoroutine(recoilCoroutine);
            visualTransform.localPosition = originalVisualPosition;
        }

        recoilCoroutine = StartCoroutine(Recoil(distance, duration));
    }

    private IEnumerator Recoil(float distance, float duration)
    {
        float recoilDirection = team == Team.Left ? -1f : 1f;
        visualTransform.localPosition =
            originalVisualPosition + new Vector3(distance * recoilDirection, 0f, 0f);

        yield return new WaitForSeconds(duration);

        visualTransform.localPosition = originalVisualPosition;
        recoilCoroutine = null;
    }

    private void UpdateAttackAnimation()
    {
        if (!isAttackAnimating || visualTransform == null) return;

        attackAnimationTimer -= Time.deltaTime;

        if (attackAnimationTimer > 0f) return;

        isAttackAnimating = false;
        visualTransform.rotation = originalVisualRotation;
    }

    protected virtual void OnDisable()
    {
        recoilCoroutine = null;

        if (visualTransform == null) return;

        visualTransform.localPosition = originalVisualPosition;
        visualTransform.rotation = originalVisualRotation;
    }
}
