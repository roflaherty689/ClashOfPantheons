using UnityEngine;

public class Projectile : MonoBehaviour
{
    private BaseUnit target;
    private float damage;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    private float travelTime;
    private float timer;

    private float arcHeight;

    public void Initialize(BaseUnit target, float damage, float travelTime, float arcHeight)
    {
        this.target = target;
        this.damage = damage;
        this.travelTime = travelTime;
        this.arcHeight = arcHeight;

        startPosition = transform.position;
        targetPosition = target.transform.position;
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