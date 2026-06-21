using UnityEngine;

public class Projectile : MonoBehaviour
{
    private IDamageable target;
    private float damage;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    private float travelTime;
    private float timer;

    private float arcHeight;

    public void Initialize(IDamageable target, float damage, float travelTime, float arcHeight)
    {
        this.target = target;
        this.damage = damage;
        this.travelTime = travelTime;
        this.arcHeight = arcHeight;

        startPosition = transform.position;
        targetPosition = target.Transform.position;


        Vector3 direction = target.Transform.position - transform.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, angle);

        Destroy(gameObject, 2f); // Auto-destroy after 2 seconds
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        timer += Time.deltaTime;

        float progress = timer / travelTime;

        if (progress >= 1f)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        Vector3 flatPosition = Vector3.Lerp(startPosition, targetPosition, progress);

        float arc = Mathf.Sin(progress * Mathf.PI) * arcHeight;

        transform.position = flatPosition + new Vector3(0, arc, 0);
    }
}