using UnityEngine;

public class Base : MonoBehaviour
{
    public Team team;
    public int maxHealth = 1000;

    private int currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Debug.Log($"{team} base destroyed!");
            Destroy(gameObject);
        }
    }
}