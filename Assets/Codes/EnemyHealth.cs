using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    public Slider healthBar; // Przypisz slider (HealthBar) w Inspectorze

    private Image fillImage;
    private Image backgroundImage;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;

            // Pobierz referencje do obrazów fill i background
            fillImage = healthBar.fillRect.GetComponent<Image>();
            backgroundImage = healthBar.transform.Find("Background")?.GetComponent<Image>();

            if (fillImage != null)
                fillImage.color = Color.red; // Ustaw pasek na czerwony

            if (backgroundImage != null)
                backgroundImage.enabled = false; // Ukryj tło na starcie
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthBar != null)
        {
            healthBar.value = currentHealth;

            // Pasek zawsze czerwony
            if (fillImage != null)
                fillImage.color = Color.red;

            // Szare tło pojawia się tylko, jeśli HP < max
            if (backgroundImage != null)
                backgroundImage.enabled = (currentHealth < maxHealth);
        }

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }
}