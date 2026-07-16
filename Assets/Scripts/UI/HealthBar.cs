using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Transform fill;

    private SpriteRenderer fillRenderer;

    private void Awake()
    {
        if (fill != null)
        {
            fillRenderer = fill.GetComponent<SpriteRenderer>();
        }
    }

    public void SetHealth(float currentHealth, float maxHealth)
    {
        if (fill == null)
        {
            return;
        }

        float healthPercent = maxHealth > 0f
            ? Mathf.Clamp01(currentHealth / maxHealth)
            : 0f;

        gameObject.SetActive(healthPercent < 1f);

        Vector3 fillScale = fill.localScale;
        fillScale.x = healthPercent;
        fill.localScale = fillScale;

        if (fillRenderer != null)
        {
            fillRenderer.color = GetHealthColor(healthPercent);
        }
    }

    private static Color GetHealthColor(float healthPercent)
    {
        if (healthPercent > 0.5f)
        {
            return Color.Lerp(Color.yellow, Color.green, (healthPercent - 0.5f) * 2f);
        }

        return Color.Lerp(Color.red, Color.yellow, healthPercent * 2f);
    }
}
