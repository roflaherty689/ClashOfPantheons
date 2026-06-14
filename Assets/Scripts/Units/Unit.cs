using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private UnitData unitData;

    private Team team;
    private Transform targetPoint;

    private Unit currentEnemyTarget;
    private float currentHealth;
    private float attackTimer;

    public Team Team => team;

    [SerializeField] private float friendlyStopRange = 0.6f;

    private Base currentBaseTarget;

    [SerializeField] private HealthBar healthBarPrefab;
    private HealthBar healthBar;

    private GameManager gameManager;

    [SerializeField] private float attackTiltAngle = 15f;
    [SerializeField] private float attackTiltDuration = 0.1f;

    private float attackAnimationTimer;
    private bool isAttackAnimating;
    private Quaternion originalRotation;

    [SerializeField] private Transform visualTransform;
    [SerializeField] private SpriteRenderer spriteRenderer;

    public void Initialize(Team team, Transform targetPoint)
    {
        gameManager = FindFirstObjectByType<GameManager>();

        this.team = team;
        this.targetPoint = targetPoint;
        currentHealth = unitData.maxHealth;
        originalRotation = visualTransform.rotation;

        healthBar = Instantiate(
            healthBarPrefab,
            transform.position + new Vector3(0, 0.8f, 0),
            Quaternion.identity,
            transform
        );

        healthBar.SetHealth(currentHealth, unitData.maxHealth);

        // attackTimer = Random.Range(0f, unitData.attackSpeed);

        if (team == Team.Left)
        {
            spriteRenderer.color = Color.red;
        }
        else
        {
            spriteRenderer.color = Color.blue;
        }
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

    private void PlayAttackAnimation()
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
            visualTransform.rotation = originalRotation;
        }
    }

    private void AttackBase()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer < unitData.attackSpeed) return;

        attackTimer = 0f;

        currentBaseTarget.TakeDamage(unitData.damage);
        PlayAttackAnimation();
    }

    private bool IsFriendlyUnitBlockingAhead()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            friendlyStopRange
        );

        foreach (Collider2D hit in hits)
        {
            Unit unit = hit.GetComponent<Unit>();

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

        foreach (Collider2D hit in hits)
        {
            Unit unit = hit.GetComponent<Unit>();

            if (unit == null) continue;
            if (unit == this) continue;
            if (unit.Team == team) continue;

            currentEnemyTarget = unit;
            return;
        }
    }

    private void AttackEnemy()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer < unitData.attackSpeed) return;

        attackTimer = 0f;

        currentEnemyTarget.TakeDamage(unitData.damage);
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
}