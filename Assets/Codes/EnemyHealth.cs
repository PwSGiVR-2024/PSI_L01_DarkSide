using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public Animator animator;
    public Slider healthBar; // Przypisz slider (HealthBar) w Inspectorze
    public GameObject canvas;
    public int currentHealth;
    public Rigidbody2D rb;
    public GameObject collectible;

    private Image fillImage;
    private Image backgroundImage;
    private bool isDead = false;
    private float launchForce = 3f;

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

    private void FreezeMovement()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeAll; // ⬅️ Lepsze niż bodyType = Static
        }
    }

    private void throwCoin()
    {
        collectible.SetActive(true);

        collectible.transform.parent = null;

        Rigidbody2D rbCoin = collectible.GetComponent<Rigidbody2D>();
        if (rbCoin != null)
        {
            Vector2 randomDirection = new Vector2(Random.Range(-1f, 1f), 1f).normalized;
            rbCoin.AddForce(randomDirection * launchForce, ForceMode2D.Impulse);
        }
    }

    void Die()
    {
        GetComponent<EnemyPatrolAI>().enabled = false;
        GetComponent<CapsuleCollider2D>().enabled = false;
        throwCoin();
        FreezeMovement();
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
