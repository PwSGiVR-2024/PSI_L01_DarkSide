using UnityEngine;

/// <summary>
/// Zaawansowany system AI dla przeciwników z funkcjami patrolowania i ścigania gracza.
/// Obsługuje automatyczne tworzenie punktów patrolowania, wykrywanie gracza oraz audio.
/// </summary>
public class EnemyPatrolAI : MonoBehaviour
{
    #region Inspector Configuration
    
    [Header("Punkty Patrolowania")]
    [Tooltip("Pierwszy punkt patrolowania (opcjonalny jeśli używasz auto-patrol)")]
    public Transform pointA;
    
    [Tooltip("Drugi punkt patrolowania (opcjonalny jeśli używasz auto-patrol)")]
    public Transform pointB;
    
    [Header("Wykrywanie Gracza")]
    [Tooltip("Transform gracza (zostanie automatycznie znaleziony jeśli pusty)")]
    public Transform player;
    
    [Tooltip("Zasięg wykrywania gracza")]
    [Range(1f, 10f)]
    public float detectionRadius = 5f;
    
    [Tooltip("Czas po którym przeciwnik przestaje ścigać gracza poza zasięgiem")]
    [Range(0.5f, 5f)]
    public float losePlayerDelay = 2f;
    
    [Header("Ruch i Prędkość")]
    [Tooltip("Prędkość poruszania się przeciwnika")]
    [Range(0.5f, 8f)]
    public float moveSpeed = 2f;
    
    [Header("Obracanie 2D")]
    [Tooltip("Czy przeciwnik ma się obracać w kierunku ruchu")]
    public bool enableFlipping = true;
    
    [Tooltip("Używaj skalowania X zamiast rotacji Y")]
    public bool useScaleFlip = true;
    
    [Header("Automatyczne Patrolowanie")]
    [Tooltip("Automatycznie twórz punkty patrolowania")]
    public bool useAutoPatrol = true;
    
    [Tooltip("Odległość punktów od pozycji startowej")]
    [Range(1f, 8f)]
    public float patrolDistance = 3f;
    
    [Tooltip("Maska warstw dla wykrywania przeszkód")]
    public LayerMask groundLayerMask = 1;
    
    [Header("Automatyczne Wykrywanie")]
    [Tooltip("Automatycznie znajdź gracza na scenie")]
    public bool autoFindPlayer = true;
    
    [Header("Dźwięki")] 
    [Tooltip("Dźwięk ataku (jednokrotny)")]
    public AudioClip attackSound;
    
    [Tooltip("Głośność dźwięku ataku")]
    [Range(0f, 1f)]
    public float attackVolume = 0.8f;
    
    #endregion
    
    #region Private State Variables
    
    // Stan AI
    private Vector3 currentTarget;           // Aktualny cel patrolowania
    private bool chasingPlayer = false;      // Czy obecnie ściga gracza
    private float timeSinceLastSeen = 0f;    // Czas od ostatniego widzenia gracza
    private bool playerDeathHandled = false; // Czy śmierć gracza została obsłużona
    
    // Komponenty
    private Animator animator;
    private AudioSource audioSource;
    
    // Pomocnicze
    private Vector2 lastPosition;       // Do obliczania prędkości
    private bool facingRight = true;    // Kierunek patrzenia
    private Vector3 originalScale;      // Oryginalna skala sprite'a
    
    #endregion
    
    #region Public Properties
    
    /// <summary>Czy przeciwnik obecnie ściga gracza</summary>
    public bool IsChasingPlayer => chasingPlayer;
    
    /// <summary>Aktualny cel patrolowania</summary>
    public Vector3 CurrentTarget => currentTarget;
    
    /// <summary>Czy punkty patrolowania są prawidłowo ustawione</summary>
    public bool HasValidPatrolPoints => pointA != null && pointB != null;
    
    /// <summary>Czy gracz został znaleziony</summary>
    public bool PlayerFound => player != null;
    
    /// <summary>Czy przeciwnik patrzy w prawo</summary>
    public bool FacingRight => facingRight;
    
    #endregion
    
    #region Unity Lifecycle
    
    /// <summary>Inicjalizacja komponentów i konfiguracji AI</summary>
    void Start()
    {
        InitializeComponents();
        InitializePlayer();
        InitializePatrolPoints();
        ValidateSetup();
    }
    
    /// <summary>Główna pętla AI - wykrywanie, ruch, animacje</summary>
    void Update()
    {
        if (!HasValidPatrolPoints)
        {
            Debug.LogWarning($"[{name}] Brak prawidłowych punktów patrolowania!");
            return;
        }
        
        UpdatePlayerStatus();
        UpdateMovement();
        UpdateFacing();
        UpdateAnimation();
    }
    
    /// <summary>Obsługa kolizji z graczem (atak)</summary>
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            HandlePlayerCollision();
        }
    }
    
    /// <summary>Rysowanie pomocy wizualnych w edytorze</summary>
    void OnDrawGizmosSelected()
    {
        DrawDebugGizmos();
    }
    
    /// <summary>Sprzątanie przed usunięciem obiektu</summary>
    void OnDestroy()
    {
        CleanupAutoPatrolPoints();
    }
    
    #endregion
    
    #region Initialization
    
    /// <summary>Inicjalizacja komponentów i podstawowych ustawień</summary>
    private void InitializeComponents()
    {
        animator = GetComponent<Animator>();
        lastPosition = transform.position;
        originalScale = transform.localScale;
        
        SetupAudioSource();
    }
    
    /// <summary>Konfiguracja źródła dźwięku</summary>
    private void SetupAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    /// <summary>Inicjalizacja referencji do gracza</summary>
    private void InitializePlayer()
    {
        if (autoFindPlayer && player == null)
        {
            FindPlayer();
        }
        
        if (player == null)
        {
            Debug.LogWarning($"[{name}] Gracz nie został znaleziony!");
        }
    }
    
    /// <summary>Konfiguracja punktów patrolowania</summary>
    private void InitializePatrolPoints()
    {
        if (useAutoPatrol && !HasValidPatrolPoints)
        {
            SetupAutoPatrolPoints();
        }
        
        SetInitialTarget();
    }
    
    /// <summary>Ustawienie początkowego celu patrolowania</summary>
    private void SetInitialTarget()
    {
        currentTarget = pointA != null ? pointA.position : transform.position;
    }
    
    /// <summary>Sprawdzenie poprawności konfiguracji</summary>
    private void ValidateSetup()
    {
        if (!HasValidPatrolPoints)
        {
            Debug.LogError($"[{name}] Nieprawidłowa konfiguracja! Wymagane są punkty A i B.");
        }
    }
    
    #endregion
    
    #region Player Detection & Management
    
    /// <summary>Automatyczne wyszukiwanie gracza na scenie</summary>
    private void FindPlayer()
    {
        // Próba wyszukania po tagu
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        
        if (playerObject != null)
        {
            player = playerObject.transform;
            Debug.Log($"[{name}] Znaleziono gracza: {player.name}");
            return;
        }
        
        // Plan awaryjny - wyszukanie po nazwie
        playerObject = GameObject.Find("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            Debug.Log($"[{name}] Znaleziono gracza po nazwie: {player.name}");
        }
    }
    
    /// <summary>Sprawdzenie czy gracz żyje</summary>
    private bool IsPlayerAlive()
    {
        if (player == null) return false;
        
        HealthManager playerHealth = player.GetComponent<HealthManager>();
        return playerHealth == null || (playerHealth.IsAlive && !playerHealth.IsDead);
    }
    
    /// <summary>Główna logika wykrywania i reagowania na gracza</summary>
    private void UpdatePlayerStatus()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool playerIsAlive = IsPlayerAlive();
        
        HandlePlayerAliveStatus(playerIsAlive, distanceToPlayer);
        HandlePlayerDeathStatus(playerIsAlive);
    }
    
    /// <summary>Obsługa żywego gracza</summary>
    private void HandlePlayerAliveStatus(bool playerIsAlive, float distanceToPlayer)
    {
        if (!playerIsAlive) return;
        
        playerDeathHandled = false;
        
        if (distanceToPlayer < detectionRadius)
        {
            StartChasingPlayer();
        }
        else if (chasingPlayer)
        {
            HandlePlayerOutOfRange();
        }
    }
    
    /// <summary>Obsługa śmierci gracza</summary>
    private void HandlePlayerDeathStatus(bool playerIsAlive)
    {
        if (playerIsAlive || playerDeathHandled || !chasingPlayer) return;
        
        playerDeathHandled = true;
        StopChasingPlayer();
        ReturnToNearestPatrolPoint();
        
        Debug.Log($"[{name}] Gracz zmarł, wracam na patrol");
    }
    
    /// <summary>Rozpoczęcie ścigania gracza</summary>
    private void StartChasingPlayer()
    {
        chasingPlayer = true;
        timeSinceLastSeen = 0f;
    }
    
    /// <summary>Zatrzymanie ścigania gracza</summary>
    private void StopChasingPlayer()
    {
        chasingPlayer = false;
        timeSinceLastSeen = losePlayerDelay;
    }
    
    /// <summary>Obsługa gdy gracz uciekł poza zasięg</summary>
    private void HandlePlayerOutOfRange()
    {
        timeSinceLastSeen += Time.deltaTime;
        
        if (timeSinceLastSeen >= losePlayerDelay)
        {
            StopChasingPlayer();
            ReturnToNearestPatrolPoint();
            Debug.Log($"[{name}] Straciłem gracza, wracam na patrol");
        }
    }
    
    #endregion
    
    #region Movement & Patrol
    
    /// <summary>Główna logika ruchu - ściganie lub patrolowanie</summary>
    private void UpdateMovement()
    {
        if (ShouldChasePlayer())
        {
            ChasePlayer();
        }
        else
        {
            PatrolBetweenPoints();
        }
    }
    
    /// <summary>Sprawdzenie czy powinien ścigać gracza</summary>
    private bool ShouldChasePlayer()
    {
        return chasingPlayer && player != null && IsPlayerAlive();
    }
    
    /// <summary>Ruch w kierunku gracza</summary>
    private void ChasePlayer()
    {
        transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
    }
    
    /// <summary>Patrolowanie między punktami A i B</summary>
    private void PatrolBetweenPoints()
    {
        transform.position = Vector2.MoveTowards(transform.position, currentTarget, moveSpeed * Time.deltaTime);
        
        if (Vector2.Distance(transform.position, currentTarget) < 0.2f)
        {
            SwitchPatrolTarget();
        }
    }
    
    /// <summary>Przełączanie między punktami patrolowania</summary>
    private void SwitchPatrolTarget()
    {
        bool isAtPointA = Vector3.Distance(currentTarget, pointA.position) < 0.2f;
        currentTarget = isAtPointA ? pointB.position : pointA.position;
    }
    
    /// <summary>Powrót do najbliższego punktu patrolowania</summary>
    private void ReturnToNearestPatrolPoint()
    {
        if (!HasValidPatrolPoints) return;
        
        float distToA = Vector2.Distance(transform.position, pointA.position);
        float distToB = Vector2.Distance(transform.position, pointB.position);
        currentTarget = distToA < distToB ? pointA.position : pointB.position;
    }
    
    #endregion
    
    #region Facing Direction
    
    /// <summary>Aktualizacja kierunku patrzenia</summary>
    private void UpdateFacing()
    {
        if (!enableFlipping) return;
        
        Vector3 targetPosition = ShouldChasePlayer() ? player.position : currentTarget;
        UpdateFacingDirection(targetPosition);
    }
    
    /// <summary>Zmiana kierunku patrzenia w zależności od celu</summary>
    private void UpdateFacingDirection(Vector3 targetPosition)
    {
        float horizontalDirection = targetPosition.x - transform.position.x;
        
        if (Mathf.Abs(horizontalDirection) > 0.1f)
        {
            bool shouldFaceRight = horizontalDirection > 0;
            
            if (shouldFaceRight != facingRight)
            {
                FlipCharacter();
            }
        }
    }
    
    /// <summary>Obrócenie sprite'a przeciwnika</summary>
    private void FlipCharacter()
    {
        facingRight = !facingRight;
        
        if (useScaleFlip)
        {
            FlipWithScale();
        }
        else
        {
            FlipWithRotation();
        }
    }
    
    /// <summary>Obrót przez skalowanie X</summary>
    private void FlipWithScale()
    {
        Vector3 newScale = originalScale;
        newScale.x = facingRight ? Mathf.Abs(originalScale.x) : -Mathf.Abs(originalScale.x);
        transform.localScale = newScale;
    }
    
    /// <summary>Obrót przez rotację Y</summary>
    private void FlipWithRotation()
    {
        Vector3 rotation = transform.eulerAngles;
        rotation.y = facingRight ? 0 : 180;
        transform.eulerAngles = rotation;
    }
    
    #endregion
    
    #region Auto Patrol Setup
    
    /// <summary>Automatyczne tworzenie punktów patrolowania</summary>
    private void SetupAutoPatrolPoints()
    {
        Vector2 currentPos = transform.position;
        Vector2 leftPoint = currentPos + Vector2.left * patrolDistance;
        Vector2 rightPoint = currentPos + Vector2.right * patrolDistance;
        
        bool leftValid = IsPositionValid(leftPoint);
        bool rightValid = IsPositionValid(rightPoint);
        
        CreateOptimalPatrolPoints(currentPos, leftPoint, rightPoint, leftValid, rightValid);
        
        Debug.Log($"[{name}] Auto-patrol punkty utworzone - A: {pointA?.position}, B: {pointB?.position}");
    }
    
    /// <summary>Tworzenie optymalnych punktów patrolowania</summary>
    private void CreateOptimalPatrolPoints(Vector2 currentPos, Vector2 leftPoint, Vector2 rightPoint, bool leftValid, bool rightValid)
    {
        if (leftValid && rightValid)
        {
            CreatePatrolPoint(ref pointA, leftPoint, "AutoPatrolPoint_A");
            CreatePatrolPoint(ref pointB, rightPoint, "AutoPatrolPoint_B");
        }
        else if (leftValid)
        {
            CreatePatrolPoint(ref pointA, leftPoint, "AutoPatrolPoint_A");
            CreatePatrolPoint(ref pointB, currentPos + Vector2.right * (patrolDistance * 0.5f), "AutoPatrolPoint_B");
        }
        else if (rightValid)
        {
            CreatePatrolPoint(ref pointA, currentPos + Vector2.left * (patrolDistance * 0.5f), "AutoPatrolPoint_A");
            CreatePatrolPoint(ref pointB, rightPoint, "AutoPatrolPoint_B");
        }
        else
        {
            // Awaryjne małe punkty
            float safeDistance = patrolDistance * 0.3f;
            CreatePatrolPoint(ref pointA, currentPos + Vector2.left * safeDistance, "AutoPatrolPoint_A");
            CreatePatrolPoint(ref pointB, currentPos + Vector2.right * safeDistance, "AutoPatrolPoint_B");
        }
    }
    
    /// <summary>Sprawdzenie czy pozycja jest wolna od przeszkód</summary>
    private bool IsPositionValid(Vector2 position)
    {
        return Physics2D.OverlapCircle(position, 0.2f, groundLayerMask) == null;
    }
    
    /// <summary>Utworzenie lub aktualizacja punktu patrolowania</summary>
    private void CreatePatrolPoint(ref Transform point, Vector2 position, string pointName)
    {
        if (point == null)
        {
            GameObject newPoint = new GameObject(pointName);
            newPoint.transform.position = position;
            point = newPoint.transform;
        }
        else
        {
            point.position = position;
        }
    }
    
    /// <summary>Usunięcie automatycznie utworzonych punktów</summary>
    private void CleanupAutoPatrolPoints()
    {
        CleanupPoint(pointA);
        CleanupPoint(pointB);
    }
    
    /// <summary>Usunięcie pojedynczego auto-punktu</summary>
    private void CleanupPoint(Transform point)
    {
        if (point != null && point.gameObject.name.Contains("AutoPatrolPoint"))
        {
            DestroyImmediate(point.gameObject);
        }
    }
    
    #endregion
    
    #region Animation & Audio
    
    /// <summary>Aktualizacja animacji na podstawie ruchu</summary>
    private void UpdateAnimation()
    {
        if (animator == null) return;
        
        float movementSpeed = CalculateMovementSpeed();
        animator.SetFloat("Speed", movementSpeed);
    }
    
    /// <summary>Obliczenie aktualnej prędkości ruchu</summary>
    private float CalculateMovementSpeed()
    {
        float speed = ((Vector2)transform.position - lastPosition).magnitude / Time.deltaTime;
        lastPosition = transform.position;
        return speed;
    }
    #endregion
    
    #region Collision Handling
    
    /// <summary>Obsługa kolizji z graczem</summary>
    private void HandlePlayerCollision()
    {
        PlayAttackAnimation();
        PlayAttackSound();
    }
    
    /// <summary>Odtworzenie animacji ataku</summary>
    private void PlayAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }
    
    /// <summary>Odtworzenie dźwięku ataku</summary>
    private void PlayAttackSound()
    {
        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound, attackVolume);
        }
    }
    
    #endregion
    
    #region Debug & Visualization
    
    /// <summary>Rysowanie wszystkich pomocy wizualnych</summary>
    private void DrawDebugGizmos()
    {
        DrawDetectionRadius();
        DrawPatrolPoints();
        DrawCurrentTarget();
        DrawPlayerConnection();
        
        if (useAutoPatrol)
        {
            DrawPatrolRange();
        }
    }
    
    /// <summary>Zasięg wykrywania gracza</summary>
    private void DrawDetectionRadius()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
    
    /// <summary>Punkty patrolowania</summary>
    private void DrawPatrolPoints()
    {
        Gizmos.color = Color.green;
        
        if (pointA != null)
        {
            Gizmos.DrawWireSphere(pointA.position, 0.2f);
        }
        
        if (pointB != null)
        {
            Gizmos.DrawWireSphere(pointB.position, 0.2f);
        }
    }
    
    /// <summary>Aktualny cel patrolowania</summary>
    private void DrawCurrentTarget()
    {
        if (!chasingPlayer && Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(currentTarget, 0.15f);
        }
    }
    
    /// <summary>Połączenie z graczem</summary>
    private void DrawPlayerConnection()
    {
        if (player != null && Application.isPlaying)
        {
            Gizmos.color = chasingPlayer ? Color.red : Color.gray;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
    
    /// <summary>Zasięg auto-patrolowania</summary>
    private void DrawPatrolRange()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, patrolDistance);
    }
    
    #endregion
    
    #region Public Methods & Context Menu
    
    [ContextMenu("Znajdź gracza")]
    public void ForceFindPlayer() => FindPlayer();
    
    [ContextMenu("Ustaw punkty patrolowania")]
    public void ForceSetupPatrolPoints() => SetupAutoPatrolPoints();
    
    [ContextMenu("Test dźwięków")]
    public void TestAudio()
    {
        if (audioSource != null)
        {
            if (attackSound != null) audioSource.PlayOneShot(attackSound);
        }
    }
    
    /// <summary>Ręczne ustawienie gracza</summary>
    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
        Debug.Log($"[{name}] Gracz ustawiony: {player?.name ?? "BRAK"}");
    }
    
    /// <summary>Ręczne ustawienie punktów patrolowania</summary>
    public void SetPatrolPoints(Transform newPointA, Transform newPointB)
    {
        pointA = newPointA;
        pointB = newPointB;
        SetInitialTarget();
        Debug.Log($"[{name}] Punkty patrolowania ustawione - A: {pointA?.position}, B: {pointB?.position}");
    }
    
    #endregion
}