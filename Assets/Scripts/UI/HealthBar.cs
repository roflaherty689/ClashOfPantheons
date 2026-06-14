using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Transform fill;

    public void SetHealth(float currentHealth, float maxHealth)
    {
        float healthPercent = Mathf.Clamp01(currentHealth / maxHealth);

        gameObject.SetActive(healthPercent < 1f);

        fill.localScale = new Vector3(
            healthPercent,
            fill.localScale.y,
            fill.localScale.z
        );

        SpriteRenderer fillRenderer = fill.GetComponent<SpriteRenderer>();
        fillRenderer.color = GetHealthColor(healthPercent);
    }

    private Color GetHealthColor(float healthPercent)
    {
        if (healthPercent > 0.5f)
        {
            return Color.Lerp(Color.yellow, Color.green, (healthPercent - 0.5f) * 2f);
        }

        return Color.Lerp(Color.red, Color.yellow, healthPercent * 2f);
    }
}