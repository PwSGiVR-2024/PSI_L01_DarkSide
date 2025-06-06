using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Persistent Player Data")]
    public int savedHealth = -1; // -1 oznacza brak zapisanych danych
    public int savedMaxHealth = 3;
    public bool hasHealthData = false;
    
    [Header("Debug")]
    public bool resetHealthOnNewGame = true;
    
    private void Awake()
    {
        // Singleton pattern - tylko jedna instancja
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[PlayerStateManager] Created and marked as DontDestroyOnLoad");
        }
        else
        {
            Debug.Log("[PlayerStateManager] Duplicate instance destroyed");
            Destroy(gameObject);
        }
    }
    
    // Zapisz stan zdrowia gracza
    public void SavePlayerHealth(int currentHealth, int maxHealth)
    {
        savedHealth = currentHealth;
        savedMaxHealth = maxHealth;
        hasHealthData = true;
        
        Debug.Log($"[PlayerStateManager] Health saved: {currentHealth}/{maxHealth}");
    }
    
    // Pobierz zapisany stan zdrowia
    public bool GetSavedHealth(out int health, out int maxHealth)
    {
        health = savedHealth;
        maxHealth = savedMaxHealth;
        
        bool hasData = hasHealthData && savedHealth >= 0;
        Debug.Log($"[PlayerStateManager] Health requested - Has data: {hasData}, Health: {health}/{maxHealth}");
        
        return hasData;
    }
    
    // Wyczyść zapisane dane (nowa gra)
    public void ClearSavedData()
    {
        savedHealth = -1;
        savedMaxHealth = 3;
        hasHealthData = false;
        
        Debug.Log("[PlayerStateManager] Saved data cleared");
    }
    
    // Sprawdź czy to nowa gra (np. z menu głównego)
    public void StartNewGame()
    {
        if (resetHealthOnNewGame)
        {
            ClearSavedData();
            Debug.Log("[PlayerStateManager] New game started - health data reset");
        }
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus) // Gdy gra wraca z pauzy
        {
            Debug.Log("[PlayerStateManager] Application resumed");
        }
    }
}