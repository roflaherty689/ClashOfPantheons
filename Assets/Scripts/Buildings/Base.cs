using UnityEngine;

public class Base : MonoBehaviour, IDamageable
{

    [SerializeField]
    private FactionData faction;

    [SerializeField] private Team team;
    [SerializeField] private float maxHealth = 50f;

    private float currentHealth;
    private GameManager gameManager;

    public Team Team => team;

    [SerializeField] private HealthBar healthBarPrefab;
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0, 1.2f, 0);

    private HealthBar healthBar;

    [SerializeField] private Transform visualTransform;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private float shakeDuration = 0.1f;
    [SerializeField] private float shakeStrength = 0.05f;
    public TargetType TargetType => TargetType.Building;

    private bool isShaking;
    private float shakeTimer;
    private Vector3 originalVisualPosition;

    public Transform Transform => transform;

    private void Awake()
    {
        currentHealth = maxHealth;
        gameManager = FindFirstObjectByType<GameManager>();

        originalVisualPosition = visualTransform.localPosition;

        if (gameManager.setTeamColour)
        {
            if (team == Team.Left)
            {
                spriteRenderer.color = Color.red;
            }
            else
            {
                spriteRenderer.color = Color.blue;
            }            
        }

        healthBar = Instantiate(
            healthBarPrefab,
            transform.position + healthBarOffset,
            Quaternion.identity,
            transform
        );

        healthBar.SetHealth(currentHealth, maxHealth);
    }

    private void Update()
    {
        UpdateShake();
    }

    private void UpdateShake()
    {
        if (!isShaking) return;

        shakeTimer -= Time.deltaTime;

        if (shakeTimer <= 0)
        {
            isShaking = false;
            visualTransform.localPosition = originalVisualPosition;
            return;
        }

        float offsetX = Random.Range(-shakeStrength, shakeStrength);

        visualTransform.localPosition =
            originalVisualPosition + new Vector3(offsetX, 0f, 0f);
    }

    private void PlayDamageAnimation()
    {
        isShaking = true;
        shakeTimer = shakeDuration;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        PlayDamageAnimation();

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            string winningTeam = team == Team.Left
                ? "Blue"
                : "Red";

            gameManager.EndGame(winningTeam);
        }
    }
}