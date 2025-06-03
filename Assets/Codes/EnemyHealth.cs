using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public Animator animator;
    public Slider healthBar; // Przypisz slider (HealthBar) w Inspectorze
    public GameObject canvas;

    private Image fillImage;
    private Image backgroundImage;
    private int currentHealth;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;

        animator = GetComponent<Animator>(); // Pobierz animatora

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;

            fillImage = healthBar.fillRect.GetComponent<Image>();
            backgroundImage = healthBar.transform.Find("Background")?.GetComponent<Image>();

            if (fillImage != null)
                fillImage.color = Color.red;

            if (backgroundImage != null)
                backgroundImage.enabled = false;
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return; // Jeśli już martwy, ignoruj dalsze obrażenia

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthBar != null)
        {
            healthBar.value = currentHealth;

            if (fillImage != null)
                fillImage.color = Color.red;

            if (backgroundImage != null)
                backgroundImage.enabled = (currentHealth < maxHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        canvas.SetActive(false);
        if (isDead) return;
        isDead = true;

        if (animator != null)
        {
            animator.SetTrigger("IsDead");
            Destroy(gameObject, 1.3f);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
