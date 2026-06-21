using UnityEngine;

public class Projectile : MonoBehaviour
{
    private IDamageable target;
    private Transform targetTransform;

    private float damage;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    private float travelTime;
    private float timer;

    private float arcHeight;
    private bool initialized;

    [SerializeField] private float hitRadius = 0.2f;

    public void Initialize(IDamageable target, float damage, float travelTime, float arcHeight)
    {
        Destroy(gameObject, 2f);

        if (target == null || target.Transform == null)
        {
            Destroy(gameObject);
            return;
        }

        this.target = target;
        this.targetTransform = target.Transform;
        this.damage = damage;
        this.travelTime = Mathf.Max(0.01f, travelTime);
        this.arcHeight = arcHeight;

        startPosition = transform.position;
        targetPosition = targetTransform.position;

        Vector3 direction = targetPosition - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        initialized = true;
    }

    private void Update()
    {
        if (!initialized)
            return;

        timer += Time.deltaTime;

        float progress = timer / travelTime;

        if (progress >= 1f)
        {
            TryApplyDamage();
            Destroy(gameObject);
            return;
        }

        Vector3 flatPosition = Vector3.Lerp(startPosition, targetPosition, progress);
        float arc = Mathf.Sin(progress * Mathf.PI) * arcHeight;

        transform.position = flatPosition + new Vector3(0, arc, 0);
    }

    private void TryApplyDamage()
    {
        if (targetTransform == null)
            return;

        float distanceToImpactPoint = Vector3.Distance(targetTransform.position, targetPosition);

        if (distanceToImpactPoint <= hitRadius && target != null)
        {
            target.TakeDamage(damage);
        }
    }
}