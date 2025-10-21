using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("Death Settings")]
    public bool destroyOnDeath = true;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
    }

    // Public method to apply damage
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth < 0)
            currentHealth = 0;

        if (currentHealth == 0)
            Die();
    }

    // Public method to heal
    public void Heal(int amount)
    {
        currentHealth += amount;

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }

    // Called when health reaches zero
    void Die()
    {
        // If this object is the Player, trigger Game Over instead of destroying
        if (CompareTag("Player"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.GameOver();
            else
            {
                var gm = Object.FindAnyObjectByType<GameManager>();
                if (gm != null) gm.GameOver();
            }
            return;
        }

        // Default: destroy if configured
        if (destroyOnDeath)
            Destroy(gameObject);
    }
}
