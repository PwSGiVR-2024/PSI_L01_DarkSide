using UnityEngine;

public class HealthCollectible : MonoBehaviour
{
    [Header("Pickup Settings")]
    public int healAmount = 1;
    public bool destroyOnPickup = true;
    
    [Header("Effects")]
    public AudioClip pickupSound;
    public GameObject pickupEffect;
    public float effectDuration = 2f;
    
    [Header("UI")]
    public bool updateScoreUI = false;
    public int scoreValue = 10;
    
    private AudioSource audioSource;
    private bool isPickedUp = false;
    
    void Start()
    {
        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && pickupSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isPickedUp) return; // Zapobiegnij wielokrotnemu zebraniu
        
        if (other.CompareTag("Player"))
        {
            HealthManager playerHealth = other.GetComponent<HealthManager>();
            if (playerHealth != null)
            {
                // Sprawdź czy gracz potrzebuje leczenia
                if (playerHealth.CurrentLife < playerHealth.MaxLife)
                {
                    // Ulecz gracza
                    playerHealth.Heal(healAmount);
                    
                    // Wywołaj efekty
                    PlayPickupEffects();
                    
                    // Aktualizuj wynik jeśli potrzeba
                    if (updateScoreUI)
                    {
                        ScoreManager.score += scoreValue;
                        // Aktualizuj UI score jeśli istnieje
                        UpdateScoreUI();
                    }
                    
                    Debug.Log($"[{name}] Player healed by {healAmount}. Current health: {playerHealth.CurrentLife}/{playerHealth.MaxLife}");
                    
                    // Oznacz jako zebrane
                    isPickedUp = true;
                    
                    // Usuń obiekt
                    if (destroyOnPickup)
                    {
                        DestroyPickup();
                    }
                }
                else
                {
                    Debug.Log($"[{name}] Player already has full health ({playerHealth.CurrentLife}/{playerHealth.MaxLife})");
                }
            }
            else
            {
                Debug.LogWarning($"[{name}] Player doesn't have HealthManager component!");
            }
        }
    }

    private void PlayPickupEffects()
    {
        // Stwórz tymczasowy obiekt do dźwięku
        if (pickupSound != null)
        {
            GameObject tempAudio = new GameObject("TempPickupAudio");
            AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
            tempSource.clip = pickupSound;
            tempSource.volume = 0.6f;
            tempSource.Play();
            Destroy(tempAudio, pickupSound.length);
        }

        // Efekt wizualny
        if (pickupEffect != null)
        {
            GameObject effect = Instantiate(pickupEffect, transform.position, transform.rotation);
            Destroy(effect, effectDuration);
        }
    }

    private void UpdateScoreUI()
    {
        // Znajdź ScoreManager i aktualizuj UI
        ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
        if (scoreManager != null && scoreManager.scoreText != null)
        {
            scoreManager.scoreText.text = "Score: " + ScoreManager.score;
        }
    }
    
    private void DestroyPickup()
    {
        // Jeśli ma dźwięk, poczekaj aż się skończy
        if (audioSource != null && pickupSound != null)
        {
            // Ukryj wizualnie ale nie usuń od razu
            GetComponent<SpriteRenderer>()?.gameObject.SetActive(false);
            GetComponent<Collider2D>().enabled = false;
            
            // Usuń po czasie trwania dźwięku
            Destroy(gameObject, pickupSound.length);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
}