using UnityEngine;

public class Base : MonoBehaviour, IDamageable
{
    [Header("Team")]
    [SerializeField] private Team team;

    [Header("Health")]
    [SerializeField, Min(0.01f)] private float maxHealth = 50f;
    [SerializeField] private HealthBar healthBarPrefab;
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0, 1.2f, 0);

    [Header("Visuals")]
    [SerializeField] private Transform visualTransform;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float shakeDuration = 0.1f;
    [SerializeField] private float shakeStrength = 0.05f;

    private float currentHealth;
    private GameManager gameManager;
    private HealthBar healthBar;
    private bool isShaking;
    private float shakeTimer;
    private Vector3 originalVisualPosition;
    private bool isDestroyed;

    public Team Team => team;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => Mathf.Max(0.01f, maxHealth);
    public Transform Transform => transform;
    public TargetType TargetType => TargetType.Building;

    private void Awake()
    {
        currentHealth = Mathf.Max(0.01f, maxHealth);
        gameManager = FindAnyObjectByType<GameManager>();

        if (visualTransform != null)
        {
            originalVisualPosition = visualTransform.localPosition;
        }

        if (gameManager != null && gameManager.SetTeamColour && spriteRenderer != null)
        {
            spriteRenderer.color = team == Team.Left ? Color.red : Color.blue;
        }

        if (healthBarPrefab == null) return;

        healthBar = Instantiate(
            healthBarPrefab,
            transform.position + healthBarOffset,
            Quaternion.identity,
            transform);
        healthBar.SetHealth(currentHealth, MaxHealth);
    }

    private void Update()
    {
        UpdateShake();
    }

    private void UpdateShake()
    {
        if (!isShaking || visualTransform == null) return;

        shakeTimer -= Time.deltaTime;

        if (shakeTimer <= 0)
        {
            isShaking = false;
            visualTransform.localPosition = originalVisualPosition;
            return;
        }

        float offsetX = Random.Range(-shakeStrength, shakeStrength);

        visualTransform.localPosition = originalVisualPosition + new Vector3(offsetX, 0f, 0f);
    }

    private void PlayDamageAnimation()
    {
        if (visualTransform == null) return;

        isShaking = true;
        shakeTimer = shakeDuration;
    }

    public void TakeDamage(float damage)
    {
        if (isDestroyed || damage <= 0f) return;

        currentHealth = Mathf.Max(0f, currentHealth - damage);

        PlayDamageAnimation();

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, MaxHealth);
        }

        if (currentHealth > 0f) return;

        isDestroyed = true;
        Team winningTeam = team == Team.Left ? Team.Right : Team.Left;
        gameManager?.EndGame(winningTeam);
    }
}
