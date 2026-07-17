using UnityEngine;

public class MonkUnit : BaseUnit
{
    [Header("Healing")]
    [SerializeField, Min(0f)] private float healRange = 2f;
    [SerializeField, Min(0f)] private float baseHealAmount = 5f;
    [SerializeField, Min(0.01f)] private float healInterval = 3f;
    [SerializeField] private GameObject healEffectPrefab;
    [SerializeField, Min(0.01f)] private float healEffectLifetime = 1.1f;

    private float healTimer;

    protected override bool CanAttack => false;
    protected override float TargetPointStoppingDistance => healRange;
    protected override bool SeparateFriendliesWhileMoving => true;

    protected override bool TryHandleSpecialAction()
    {
        BaseUnit target = FindMostInjuredAllyInCombat();
        if (target == null)
        {
            if (HasEnemyUnitInRange())
            {
                SetMovingAnimation(false);
                ApplyFriendlyCombatSeparation();
                return true;
            }

            return false;
        }

        SetMovingAnimation(false);
        ApplyFriendlyCombatSeparation();
        healTimer += Time.deltaTime;

        float interval = Mathf.Max(0.01f, healInterval);
        if (healTimer < interval)
        {
            return true;
        }

        healTimer -= interval;
        float healedAmount = target.ReceiveHealing(baseHealAmount * StatMultiplier);
        if (healedAmount <= 0f)
        {
            return true;
        }

        PlayAttackAnimation();
        SpawnHealEffect(target);
        return true;
    }

    private BaseUnit FindMostInjuredAllyInCombat()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            Mathf.Max(0f, healRange));

        BaseUnit bestTarget = null;
        float lowestHealthRatio = 1f;
        float closestDistanceSquared = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            BaseUnit candidate = hit.GetComponent<BaseUnit>();
            if (!IsValidTarget(candidate))
            {
                continue;
            }

            float healthRatio = candidate.CurrentHealth / candidate.MaximumHealth;
            float distanceSquared = (candidate.transform.position - transform.position).sqrMagnitude;

            bool isMoreInjured = healthRatio < lowestHealthRatio;
            bool isEquivalentHealth = Mathf.Approximately(healthRatio, lowestHealthRatio);
            bool isCloser = distanceSquared < closestDistanceSquared;
            if (!isMoreInjured && !(isEquivalentHealth && isCloser))
            {
                continue;
            }

            bestTarget = candidate;
            lowestHealthRatio = healthRatio;
            closestDistanceSquared = distanceSquared;
        }

        return bestTarget;
    }

    private bool HasEnemyUnitInRange()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            Mathf.Max(0f, healRange));

        foreach (Collider2D hit in hits)
        {
            BaseUnit candidate = hit.GetComponent<BaseUnit>();
            if (candidate != null &&
                candidate.Team != Team &&
                !candidate.IsDead)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsValidTarget(BaseUnit candidate)
    {
        return candidate != null &&
               candidate != this &&
               candidate.Team == Team &&
               candidate.IsInCombat &&
               !candidate.IsDead &&
               candidate.CurrentHealth > 0f &&
               candidate.CurrentHealth < candidate.MaximumHealth;
    }

    private void SpawnHealEffect(BaseUnit target)
    {
        if (healEffectPrefab == null || target == null)
        {
            return;
        }

        GameObject effect = Instantiate(
            healEffectPrefab,
            target.transform.position,
            Quaternion.identity,
            target.transform);

        Destroy(effect, Mathf.Max(0.01f, healEffectLifetime));
    }
}
