using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class HealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    public RawImage[] lifeImages;
    public Animator animator;
    
    [Header("Death Settings")]
    public bool destroyPlayerOnDeath = false; // Opcja usuwania gracza
    public float destroyDelay = 2f; // Opóźnienie przed usunięciem
    public GameObject deathEffect; // Efekt śmierci
    
    [Header("Health Events")]
    public UnityEvent OnHealthChanged;
    public UnityEvent OnPlayerDeath;
    public UnityEvent OnHealthHealed;
    
    [Header("Audio")]
    public AudioClip damageSound;
    public AudioClip healSound;
    public AudioClip deathSound;
    
    private int currentLife; // Liczba żyć
    private int maxLife; // Maksymalna liczba żyć
    private AudioSource audioSource;
    private bool isDead = false; // Flaga czy gracz jest martwy
    
    // Właściwości publiczne dla innych skryptów
    public int CurrentLife => currentLife;
    public int MaxLife => maxLife;
    public bool IsAlive => currentLife > 0 && !isDead;
    public float HealthPercentage => maxLife > 0 ? (float)currentLife / maxLife : 0f;
    public bool IsDead => isDead;

    void Start()
    {
        InitializeHealth();
        SetupAudio();
        ValidateComponents();
    }

    private void InitializeHealth()
    {
        // Walidacja lifeImages
        if (lifeImages == null || lifeImages.Length == 0)
        {
            Debug.LogError($"[{name}] lifeImages array is null or empty! Please assign life UI images in inspector.");
            return;
        }

        maxLife = lifeImages.Length;
        currentLife = maxLife;
        isDead = false;

        // Sprawdź wszystkie obrazy życia
        for (int i = 0; i < lifeImages.Length; i++)
        {
            if (lifeImages[i] != null)
            {
                lifeImages[i].enabled = true;
            }
            else
            {
                Debug.LogError($"[{name}] lifeImages[{i}] is null! Please assign all life images in inspector.");
            }
        }

        Debug.Log($"[{name}] Health initialized with {currentLife}/{maxLife} lives");
    }

    private void SetupAudio()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void ValidateComponents()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning($"[{name}] No Animator found. Death animation will not play.");
            }
        }
    }

    // Publiczne metody dla innych skryptów
    public void TakeDamage(int damage = 1)
    {
        if (!IsAlive || damage <= 0 || isDead) return;

        int previousLife = currentLife;
        currentLife = Mathf.Max(0, currentLife - damage);
        
        UpdateLifeDisplay();
        PlayDamageEffects();
        
        Debug.Log($"[{name}] Took {damage} damage. Health: {currentLife}/{maxLife}");
        
        OnHealthChanged?.Invoke();
        
        if (currentLife == 0 && previousLife > 0)
        {
            HandleDeath();
        }
    }

    public void Heal(int amount = 1)
    {
        if (amount <= 0 || isDead) return;

        if (currentLife >= maxLife)
        {
            Debug.Log($"[{name}] Already at maximum health ({currentLife}/{maxLife})");
            return;
        }

        int previousLife = currentLife;
        currentLife = Mathf.Min(maxLife, currentLife + amount);
        
        UpdateLifeDisplay();
        PlayHealEffects();
        
        Debug.Log($"[{name}] Healed {amount} health. Health: {currentLife}/{maxLife}");
        
        OnHealthChanged?.Invoke();
        OnHealthHealed?.Invoke();
    }

    public void SetHealth(int newHealth)
    {
        if (isDead) return;
        
        newHealth = Mathf.Clamp(newHealth, 0, maxLife);
        currentLife = newHealth;
        UpdateLifeDisplay();
        OnHealthChanged?.Invoke();
    }

    public void ResetHealth()
    {
        currentLife = maxLife;
        isDead = false;
        UpdateLifeDisplay();
        OnHealthChanged?.Invoke();
        
        // Włącz z powrotem komponenty gracza
        EnablePlayerComponents(true);
        
        Debug.Log($"[{name}] Health reset to full: {currentLife}/{maxLife}");
    }

    private void HandleLifeGrow()
    {
        Heal(1);
    }

    private void HandleLifeLoss()
    {
        TakeDamage(1);
    }

    private void UpdateLifeDisplay()
    {
        if (lifeImages == null) return;

        for (int i = 0; i < lifeImages.Length; i++)
        {
            if (lifeImages[i] != null)
            {
                lifeImages[i].enabled = i < currentLife;
            }
        }
    }

    private void HandleDeath()
    {
        if (isDead) return; // Zapobiegnij wielokrotnemu wywoływaniu
        
        isDead = true;
        Debug.Log($"[{name}] Game Over! Player has no lives left.");
        
        PlayDeathEffects();
        
        // Animacja śmierci
        if (animator != null)
        {
            animator.SetBool("IsDead", true);
        }
        
        // Efekt wizualny śmierci
        if (deathEffect != null)
        {
            GameObject effect = Instantiate(deathEffect, transform.position, transform.rotation);
            Destroy(effect, 3f);
        }
        
        // POPRAWKA: Dodaj wyłączenie komponentów
        EnablePlayerComponents(false);
        
        OnPlayerDeath?.Invoke();
        
        // Usuń gracza jeśli opcja jest włączona
        if (destroyPlayerOnDeath)
        {
            StartCoroutine(DestroyPlayerCoroutine());
        }
    }

    private void EnablePlayerComponents(bool enable)
    {
        // Wyłącz/włącz komponenty kontrolera ruchu
        MovementController movement = GetComponent<MovementController>();
        if (movement != null)
        {
            movement.enabled = enable;
        }
        
        // Wyłącz/włącz system ataku
        DamageManager damageManager = GetComponent<DamageManager>();
        if (damageManager != null)
        {
            damageManager.enabled = enable;
        }
        
        // Wyłącz/włącz kolizje (opcjonalnie)
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            playerCollider.enabled = enable;
        }
        
        // Wyłącz/włącz Rigidbody (zatrzymaj fizyki)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null && !enable)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = enable;
        }
        else if (rb != null && enable)
        {
            rb.simulated = enable;
        }
        
        Debug.Log($"[{name}] Player components {(enable ? "enabled" : "disabled")}");
    }

    private System.Collections.IEnumerator DestroyPlayerCoroutine()
    {
        Debug.Log($"[{name}] Player will be destroyed in {destroyDelay} seconds...");
        
        yield return new WaitForSeconds(destroyDelay);
        
        Debug.Log($"[{name}] Destroying player object...");
        Destroy(gameObject);
    }

    // Publiczna metoda do natychmiastowego usunięcia gracza
    public void DestroyPlayerImmediately()
    {
        Debug.Log($"[{name}] Destroying player immediately...");
        Destroy(gameObject);
    }

    // Publiczna metoda do wskrzeszenia gracza
    public void RevivePlayer(int livesToRestore = -1)
    {
        if (livesToRestore == -1)
        {
            livesToRestore = maxLife; // Przywróć wszystkie życia
        }
        
        currentLife = Mathf.Clamp(livesToRestore, 1, maxLife);
        isDead = false;
        
        UpdateLifeDisplay();
        EnablePlayerComponents(true);
        
        if (animator != null)
        {
            animator.SetBool("IsDead", false);
        }
        
        OnHealthChanged?.Invoke();
        Debug.Log($"[{name}] Player revived with {currentLife}/{maxLife} lives");
    }

    private void PlayDamageEffects()
    {
        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }
    }

    private void PlayHealEffects()
    {
        if (audioSource != null && healSound != null)
        {
            audioSource.PlayOneShot(healSound);
        }
    }

    private void PlayDeathEffects()
    {
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
    }

    // Kolizje - można przenieść do PlayerController jeśli potrzeba
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return; // Nie reaguj na kolizje jeśli martwy
        
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Sprawdź czy wróg ma określoną ilość obrażeń
            EnemyDamage enemyDamage = collision.gameObject.GetComponent<EnemyDamage>();
            if (enemyDamage != null)
            {
                TakeDamage(enemyDamage.damageAmount);
            }
            else
            {
                TakeDamage(1); // Domyślne obrażenia
            }
        }

        if (collision.gameObject.CompareTag("HP"))
        {
            // Sprawdź ile HP daje pickup
            HealthPickup pickup = collision.gameObject.GetComponent<HealthPickup>();
            if (pickup != null)
            {
                Heal(pickup.healAmount);
            }
            else
            {
                Heal(1); // Domyślne leczenie
            }
            
            Destroy(collision.gameObject);
        }
    }

    // Debug w edytorze
    void OnValidate()
    {
        if (lifeImages != null && Application.isPlaying)
        {
            UpdateLifeDisplay();
        }
    }

    // Publiczne informacje dla debugowania
    public void PrintHealthInfo()
    {
        Debug.Log($"[{name}] Health Info: {currentLife}/{maxLife} ({HealthPercentage:P1}) - Alive: {IsAlive} - Dead: {isDead}");
    }
}

public class EnemyDamage : MonoBehaviour
{
    public int damageAmount = 1;
}

public class HealthPickup : MonoBehaviour  
{
    public int healAmount = 1;
}