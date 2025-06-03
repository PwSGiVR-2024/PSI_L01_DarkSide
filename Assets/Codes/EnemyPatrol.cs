using UnityEngine;

public class EnemyPatrolAI : MonoBehaviour
{
    [Header("Patrol Points")]
    public Transform pointA;
    public Transform pointB;
    
    [Header("Player Detection")]
    public Transform player;
    public float detectionRadius = 5f;
    public float losePlayerDelay = 2f;
    
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    
    [Header("2D Rotation Settings")]
    public bool enableFlipping = true;          // Czy włączyć obracanie lewo/prawo
    public bool useScaleFlip = true;            // Używaj scale.x zamiast rotation
    
    [Header("Auto Patrol Settings")]
    public bool useAutoPatrol = true;
    public float patrolDistance = 3f;
    public LayerMask groundLayerMask = 1;
    
    [Header("Auto Player Detection")]
    public bool autoFindPlayer = true;
    
    [Header("Audio")]
    public GameObject walkSoundObject;
    public GameObject attackSoundObject;

    // Zmienne prywatne - stan AI
    private Vector3 currentTarget;              // Aktualny cel patrolowania
    private bool chasingPlayer = false;         // Czy obecnie ściga gracza
    private float timeSinceLastSeen = 0f;       // Czas od ostatniego widzenia gracza
    private bool playerDeathHandled = false;    // Flaga - czy śmierć gracza została obsłużona
    
    // Komponenty
    private Animator animator;
    private AudioSource walkAudioSource;
    private AudioSource attackAudioSource;
    
    // Narzędzia
    private Vector2 lastPosition;               // Ostatnia pozycja do liczenia prędkości
    private bool facingRight = true;            // Czy enemy patrzy w prawo
    private Vector3 originalScale;              // Oryginalna skala obiektu

    // Właściwości publiczne dla innych skryptów
    public bool IsChasingPlayer => chasingPlayer;
    public Vector3 CurrentTarget => currentTarget;
    public bool HasValidPatrolPoints => pointA != null && pointB != null;
    public bool PlayerFound => player != null;
    public bool FacingRight => facingRight;    // NOWA WŁAŚCIWOŚĆ - kierunek patrzenia

    #region Unity Lifecycle
    
    void Start()
    {
        // Inicjalizacja wszystkich komponentów i ustawień
        InitializeComponents();
        InitializePlayer();
        InitializePatrolPoints();
        ValidateSetup();
    }

    void Update()
    {
        // Sprawdź czy punkty patrolowania są prawidłowe
        if (!HasValidPatrolPoints)
        {
            Debug.LogWarning("[EnemyPatrolAI] Brak prawidłowych punktów patrolowania!");
            return;
        }

        // Główna pętla AI - sprawdź gracza, porusz się, obracaj, animuj
        UpdatePlayerStatus();
        UpdateMovement();
        UpdateFacing();             // NOWA FUNKCJA - proste obroty 2D
        UpdateAnimation();
        UpdateAudio();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Obsługa kolizji z graczem (atak)
        HandleCollision(collision);
    }

    void OnDrawGizmosSelected()
    {
        // Rysowanie pomocy wizualnych w edytorze
        DrawDebugGizmos();
    }

    void OnDestroy()
    {
        // Sprzątanie przed usunięciem obiektu
        CleanupPatrolPoints();
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        // Pobierz komponenty z tego obiektu
        animator = GetComponent<Animator>();
        lastPosition = transform.position;
        originalScale = transform.localScale;   // NOWE - zapamiętaj oryginalną skalę

        SetupAudioSources();
    }

    private void SetupAudioSources()
    {
        // Skonfiguruj źródła dźwięku
        if (walkSoundObject != null)
            walkAudioSource = walkSoundObject.GetComponent<AudioSource>();
        if (attackSoundObject != null)
            attackAudioSource = attackSoundObject.GetComponent<AudioSource>();
    }

    private void InitializePlayer()
    {
        // Automatyczne wyszukiwanie gracza jeśli włączone
        if (autoFindPlayer && player == null)
        {
            FindPlayer();
        }

        // Sprawdź czy gracz został znaleziony
        if (player == null)
        {
            Debug.LogWarning("[EnemyPatrolAI] Gracz nie został znaleziony! Enemy nie będzie nikogo ścigał.");
        }
        else
        {
            Debug.Log($"[EnemyPatrolAI] Znaleziono gracza: {player.name}");
        }
    }

    private void InitializePatrolPoints()
    {
        // Automatyczne tworzenie punktów patrolowania jeśli włączone
        if (useAutoPatrol && (!HasValidPatrolPoints))
        {
            SetupAutoPatrolPoints();
        }
        
        // Ustaw początkowy cel patrolowania
        SetInitialTarget();
    }

    private void SetInitialTarget()
    {
        // Ustaw punkt A jako pierwszy cel
        if (pointA != null)
        {
            currentTarget = pointA.position;
            Debug.Log($"[EnemyPatrolAI] Początkowy cel ustawiony na: {currentTarget}");
        }
        else
        {
            Debug.LogWarning("[EnemyPatrolAI] Punkt A jest pusty! Enemy nie będzie patrolował.");
            currentTarget = transform.position;
        }
    }

    private void ValidateSetup()
    {
        // Sprawdź czy konfiguracja jest prawidłowa
        if (!HasValidPatrolPoints)
        {
            Debug.LogError("[EnemyPatrolAI] Nieprawidłowa konfiguracja! Wymagane są oba punkty A i B.");
        }
    }

    #endregion

    #region Player Detection & Management

    private void FindPlayer()
    {
        // Znajdź gracza po tagu "Player"
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        
        if (playerObject != null)
        {
            player = playerObject.transform;
            Debug.Log($"[EnemyPatrolAI] Automatycznie znaleziono gracza: {player.name}");
            return;
        }

        // Plan B: szukaj po nazwie "Player"
        playerObject = GameObject.Find("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            Debug.Log($"[EnemyPatrolAI] Znaleziono gracza po nazwie: {player.name}");
        }
        else
        {
            Debug.LogWarning("[EnemyPatrolAI] Nie znaleziono gracza na scenie!");
        }
    }

    private bool IsPlayerAlive()
    {
        // Sprawdź czy gracz istnieje i czy żyje
        if (player == null) return false;

        HealthManager playerHealth = player.GetComponent<HealthManager>();
        // Jeśli nie ma HealthManager, zakładamy że gracz żyje
        return (playerHealth == null) || (playerHealth.IsAlive && !playerHealth.IsDead);
    }

    private void UpdatePlayerStatus()
    {
        // Główna logika wykrywania i reagowania na gracza
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool playerIsAlive = IsPlayerAlive();
        
        // Obsłuż żywego gracza
        HandlePlayerAliveStatus(playerIsAlive, distanceToPlayer);
        // Obsłuż martwego gracza
        HandlePlayerDeathStatus(playerIsAlive);
    }

    private void HandlePlayerAliveStatus(bool playerIsAlive, float distanceToPlayer)
    {
        // Obsługa gdy gracz żyje
        if (!playerIsAlive) return;

        // Resetuj flagę śmierci gdy gracz żyje
        playerDeathHandled = false;

        // Sprawdź czy gracz jest w zasięgu wykrywania
        if (distanceToPlayer < detectionRadius)
        {
            StartChasingPlayer();
        }
        else if (chasingPlayer)
        {
            // Gracz uciekł - zacznij odliczanie do powrotu na patrol
            HandlePlayerOutOfRange();
        }
    }

    private void HandlePlayerDeathStatus(bool playerIsAlive)
    {
        // Obsługa śmierci gracza - TYLKO RAZ
        if (playerIsAlive || playerDeathHandled || !chasingPlayer) return;

        // Oznacz że śmierć została obsłużona
        playerDeathHandled = true;
        StopChasingPlayer();
        ReturnToNearestPatrolPoint();
        
        Debug.Log("[EnemyPatrolAI] Gracz nie żyje, przestaję ścigać i wracam na patrol");
    }

    private void StartChasingPlayer()
    {
        // Rozpocznij ściganie gracza
        chasingPlayer = true;
        timeSinceLastSeen = 0f;
    }

    private void StopChasingPlayer()
    {
        // Zatrzymaj ściganie gracza
        chasingPlayer = false;
        timeSinceLastSeen = losePlayerDelay;
    }

    private void HandlePlayerOutOfRange()
    {
        // Obsługa gdy gracz jest poza zasięgiem
        timeSinceLastSeen += Time.deltaTime;
        
        // Po określonym czasie wróć na patrol
        if (timeSinceLastSeen >= losePlayerDelay)
        {
            StopChasingPlayer();
            ReturnToNearestPatrolPoint();
            Debug.Log("[EnemyPatrolAI] Straciłem gracza, wracam na patrol");
        }
    }

    #endregion

    #region Movement & Patrol

    private void UpdateMovement()
    {
        // Główna logika ruchu - ścigaj gracza lub patroluj
        if (ShouldChasePlayer())
        {
            ChasePlayer();
        }
        else
        {
            PatrolBetweenPoints();
        }
    }

    private bool ShouldChasePlayer()
    {
        // Sprawdź czy powinien ścigać gracza
        return chasingPlayer && player != null && IsPlayerAlive();
    }

    private void ChasePlayer()
    {
        // Porusz się w kierunku gracza
        transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
    }

    private void PatrolBetweenPoints()
    {
        // Patrolowanie między punktami A i B
        transform.position = Vector2.MoveTowards(transform.position, currentTarget, moveSpeed * Time.deltaTime);
        
        // Sprawdź czy dotarł do celu
        CheckPatrolTargetReached();
    }

    private void CheckPatrolTargetReached()
    {
        // Sprawdź czy dotarł do punktu patrolowania
        float distanceToTarget = Vector2.Distance(transform.position, currentTarget);
        
        if (distanceToTarget < 0.2f)
        {
            // Przełącz na drugi punkt
            SwitchPatrolTarget();
        }
    }

    private void SwitchPatrolTarget()
    {
        // Przełączanie między punktami A i B
        if (Vector3.Distance(currentTarget, pointA.position) < 0.2f)
        {
            currentTarget = pointB.position;
            Debug.Log("[EnemyPatrolAI] Przełączam cel na Punkt B");
        }
        else
        {
            currentTarget = pointA.position;
            Debug.Log("[EnemyPatrolAI] Przełączam cel na Punkt A");
        }
    }

    private void ReturnToNearestPatrolPoint()
    {
        // Wróć do najbliższego punktu patrolowania
        if (!HasValidPatrolPoints) return;

        float distToA = Vector2.Distance(transform.position, pointA.position);
        float distToB = Vector2.Distance(transform.position, pointB.position);
        currentTarget = (distToA < distToB) ? pointA.position : pointB.position;
    }

    #endregion

    #region 2D Rotation & Facing

    private void UpdateFacing()
    {
        // Aktualizuj kierunek patrzenia enemy (lewo/prawo)
        if (!enableFlipping) return;

        Vector3 targetPosition = GetCurrentTargetPosition();
        UpdateFacingDirection(targetPosition);
    }

    private Vector3 GetCurrentTargetPosition()
    {
        // Pobierz aktualny cel (gracz lub punkt patrolowania)
        if (ShouldChasePlayer())
        {
            return player.position;
        }
        else
        {
            return currentTarget;
        }
    }

    private void UpdateFacingDirection(Vector3 targetPosition)
    {
        // Sprawdź kierunek do celu
        float horizontalDirection = targetPosition.x - transform.position.x;
        
        // Jeśli różnica jest znacząca, zaktualizuj kierunek
        if (Mathf.Abs(horizontalDirection) > 0.1f)
        {
            bool shouldFaceRight = horizontalDirection > 0;
            
            // Zmień kierunek tylko jeśli jest różny od obecnego
            if (shouldFaceRight != facingRight)
            {
                FlipCharacter();
            }
        }
    }

    private void FlipCharacter()
    {
        // Zmień kierunek patrzenia enemy
        facingRight = !facingRight;
        
        if (useScaleFlip)
        {
            // Metoda 1: Używaj scale.x (zalecana dla sprite'ów)
            FlipWithScale();
        }
        else
        {
            // Metoda 2: Używaj rotation Y
            FlipWithRotation();
        }
        
        Debug.Log($"[EnemyPatrolAI] Obrócony w kierunku: {(facingRight ? "PRAWO" : "LEWO")}");
    }

    private void FlipWithScale()
    {
        // Odwróć sprite za pomocą skali X
        Vector3 newScale = originalScale;
        newScale.x = facingRight ? Mathf.Abs(originalScale.x) : -Mathf.Abs(originalScale.x);
        transform.localScale = newScale;
    }

    private void FlipWithRotation()
    {
        // Odwróć sprite za pomocą rotacji Y
        Vector3 rotation = transform.eulerAngles;
        rotation.y = facingRight ? 0 : 180;
        transform.eulerAngles = rotation;
    }

    // Funkcje pomocnicze do ręcznego ustawiania kierunku
    public void FaceRight()
    {
        // Ustaw kierunek na prawo
        if (!facingRight)
        {
            FlipCharacter();
        }
    }

    public void FaceLeft()
    {
        // Ustaw kierunek na lewo
        if (facingRight)
        {
            FlipCharacter();
        }
    }

    public void FaceTowards(Vector3 targetPosition)
    {
        // Obróć się w kierunku określonej pozycji
        float direction = targetPosition.x - transform.position.x;
        if (direction > 0.1f && !facingRight)
        {
            FlipCharacter();
        }
        else if (direction < -0.1f && facingRight)
        {
            FlipCharacter();
        }
    }

    #endregion

    #region Auto Patrol Setup

    private void SetupAutoPatrolPoints()
    {
        // Automatyczne tworzenie punktów patrolowania
        Vector2 currentPos = transform.position;
        Vector2 leftPoint = currentPos + Vector2.left * patrolDistance;
        Vector2 rightPoint = currentPos + Vector2.right * patrolDistance;
        
        // Sprawdź które pozycje są dostępne
        bool leftValid = IsPositionValid(leftPoint);
        bool rightValid = IsPositionValid(rightPoint);
        
        // Stwórz punkty w zależności od dostępnych pozycji
        CreatePatrolPointsBasedOnValidPositions(currentPos, leftPoint, rightPoint, leftValid, rightValid);
        
        Debug.Log($"[EnemyPatrolAI] Automatyczne punkty patrolowania utworzone - A: {pointA?.position}, B: {pointB?.position}");
    }

    private void CreatePatrolPointsBasedOnValidPositions(Vector2 currentPos, Vector2 leftPoint, Vector2 rightPoint, bool leftValid, bool rightValid)
    {
        // Tworzenie punktów w zależności od dostępnych pozycji
        if (leftValid && rightValid)
        {
            // Oba kierunki dostępne - idealny patrol
            CreatePatrolPoint(ref pointA, leftPoint, "PatrolPointA");
            CreatePatrolPoint(ref pointB, rightPoint, "PatrolPointB");
        }
        else if (leftValid)
        {
            // Tylko lewy kierunek dostępny
            CreatePatrolPoint(ref pointA, leftPoint, "PatrolPointA");
            CreatePatrolPoint(ref pointB, currentPos + Vector2.right * (patrolDistance * 0.5f), "PatrolPointB");
        }
        else if (rightValid)
        {
            // Tylko prawy kierunek dostępny
            CreatePatrolPoint(ref pointA, currentPos + Vector2.left * (patrolDistance * 0.5f), "PatrolPointA");
            CreatePatrolPoint(ref pointB, rightPoint, "PatrolPointB");
        }
        else
        {
            // Plan awaryjny: mniejsze punkty patrolowania
            CreatePatrolPoint(ref pointA, currentPos + Vector2.left * (patrolDistance * 0.5f), "PatrolPointA");
            CreatePatrolPoint(ref pointB, currentPos + Vector2.right * (patrolDistance * 0.5f), "PatrolPointB");
        }
    }

    private bool IsPositionValid(Vector2 position)
    {
        // Sprawdź czy pozycja nie koliduje z przeszkodami
        Collider2D hit = Physics2D.OverlapCircle(position, 0.2f, groundLayerMask);
        return hit == null;
    }

    private void CreatePatrolPoint(ref Transform point, Vector2 position, string pointName)
    {
        // Stwórz lub zaktualizuj punkt patrolowania
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

    private void CleanupPatrolPoints()
    {
        // Usuń automatycznie utworzone punkty patrolowania
        CleanupPatrolPoint(pointA);
        CleanupPatrolPoint(pointB);
    }

    private void CleanupPatrolPoint(Transform point)
    {
        // Usuń punkt jeśli był automatycznie utworzony
        if (point != null && point.gameObject.name.Contains("PatrolPoint"))
        {
            DestroyImmediate(point.gameObject);
        }
    }

    #endregion

    #region Animation & Audio

    private void UpdateAnimation()
    {
        // Aktualizuj animacje na podstawie prędkości ruchu
        if (animator == null) return;

        float movementSpeed = GetMovementSpeed();
        animator.SetFloat("Speed", movementSpeed);
    }

    private void UpdateAudio()
    {
        // Aktualizuj dźwięki na podstawie ruchu
        if (walkAudioSource == null) return;

        float movementSpeed = GetMovementSpeed();
        HandleWalkingAudio(movementSpeed);
    }

    private float GetMovementSpeed()
    {
        // Oblicz prędkość ruchu na podstawie zmiany pozycji
        float movementSpeed = ((Vector2)transform.position - lastPosition).magnitude / Time.deltaTime;
        lastPosition = transform.position;
        return movementSpeed;
    }

    private void HandleWalkingAudio(float movementSpeed)
    {
        // Obsługa dźwięku chodzenia
        bool shouldPlayWalkSound = movementSpeed > 0.01f;
        
        if (shouldPlayWalkSound && !walkAudioSource.isPlaying)
        {
            walkAudioSource.Play();
        }
        else if (!shouldPlayWalkSound && walkAudioSource.isPlaying)
        {
            walkAudioSource.Stop();
        }
    }

    #endregion

    #region Collision Handling

    private void HandleCollision(Collision2D collision)
    {
        // Obsługa kolizji z graczem (atak)
        if (collision.collider.CompareTag("Player"))
        {
            PlayAttackAnimation();
            PlayAttackSound();
        }
    }

    private void PlayAttackAnimation()
    {
        // Odtwórz animację ataku
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    private void PlayAttackSound()
    {
        // Odtwórz dźwięk ataku
        if (attackAudioSource != null && !attackAudioSource.isPlaying)
        {
            attackAudioSource.Play();
        }
    }

    #endregion

    #region Debug & Visualization

    private void DrawDebugGizmos()
    {
        // Rysuj wszystkie pomoce wizualne w edytorze
        DrawDetectionRadius();
        DrawPatrolPoints();
        DrawPatrolArea();
        DrawCurrentTarget();
        DrawPlayerConnection();
        DrawFacingDirection();      // NOWA FUNKCJA - kierunek patrzenia
    }

    private void DrawDetectionRadius()
    {
        // Narysuj zasięg wykrywania gracza (czerwony okrąg)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    private void DrawPatrolPoints()
    {
        // Narysuj punkty patrolowania (zielone kółka i linie)
        Gizmos.color = Color.green;
        
        if (pointA != null)
        {
            Gizmos.DrawWireSphere(pointA.position, 0.2f);
            Gizmos.DrawLine(transform.position, pointA.position);
        }
        
        if (pointB != null)
        {
            Gizmos.DrawWireSphere(pointB.position, 0.2f);
            Gizmos.DrawLine(transform.position, pointB.position);
        }
    }

    private void DrawPatrolArea()
    {
        // Narysuj obszar auto-patrolowania (żółty okrąg)
        if (useAutoPatrol)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, patrolDistance);
        }
    }

    private void DrawCurrentTarget()
    {
        // Narysuj aktualny cel patrolowania (niebieskie kółko)
        if (!chasingPlayer && Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(currentTarget, 0.15f);
        }
    }

    private void DrawPlayerConnection()
    {
        // Narysuj połączenie z graczem (czerwone = ściga, szare = nie ściga)
        if (player != null && Application.isPlaying)
        {
            Gizmos.color = chasingPlayer ? Color.red : Color.gray;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }

    private void DrawFacingDirection()
    {
        // Narysuj kierunek patrzenia (magentowa strzałka)
        if (Application.isPlaying)
        {
            Gizmos.color = Color.magenta;
            Vector3 direction = facingRight ? Vector3.right : Vector3.left;
            Vector3 arrowEnd = transform.position + direction * 0.8f;
            
            // Główna linia strzałki
            Gizmos.DrawLine(transform.position, arrowEnd);
            
            // Grotka strzałki
            Vector3 arrowHead1 = arrowEnd - direction * 0.3f + Vector3.up * 0.2f;
            Vector3 arrowHead2 = arrowEnd - direction * 0.3f + Vector3.down * 0.2f;
            Gizmos.DrawLine(arrowEnd, arrowHead1);
            Gizmos.DrawLine(arrowEnd, arrowHead2);
        }
    }

    #endregion

    #region Public Methods & Context Menu

    [ContextMenu("Znajdź gracza")]
    public void ForceFindPlayer()
    {
        FindPlayer();
    }

    [ContextMenu("Ustaw punkty patrolowania")]
    public void ForceSetupPatrolPoints()
    {
        SetupAutoPatrolPoints();
    }

    [ContextMenu("Resetuj punkty patrolowania")]
    public void ResetPatrolPoints()
    {
        // Usuń stare punkty i stwórz nowe
        CleanupPatrolPoints();
        pointA = null;
        pointB = null;
        
        if (useAutoPatrol)
        {
            SetupAutoPatrolPoints();
            SetInitialTarget();
        }
    }

    [ContextMenu("Obróć w prawo")]
    public void ForceFlipRight()
    {
        FaceRight();
    }

    [ContextMenu("Obróć w lewo")]
    public void ForceFlipLeft()
    {
        FaceLeft();
    }

    [ContextMenu("Włącz/Wyłącz obracanie")]
    public void ToggleFlipping()
    {
        enableFlipping = !enableFlipping;
        Debug.Log($"[EnemyPatrolAI] Obracanie: {(enableFlipping ? "WŁĄCZONE" : "WYŁĄCZONE")}");
    }

    [ContextMenu("Informacje debug")]
    public void PrintDebugInfo()
    {
        // Wyświetl szczegółowe informacje o stanie AI
        Debug.Log("=== ENEMY PATROL AI DEBUG ===");
        Debug.Log($"Gracz: {(player != null ? player.name : "BRAK")}");
        Debug.Log($"Gracz żyje: {IsPlayerAlive()}");
        Debug.Log($"Punkt A: {(pointA != null ? pointA.position.ToString() : "BRAK")}");
        Debug.Log($"Punkt B: {(pointB != null ? pointB.position.ToString() : "BRAK")}");
        Debug.Log($"Aktualny cel: {currentTarget}");
        Debug.Log($"Ściga gracza: {chasingPlayer}");
        Debug.Log($"Śmierć gracza obsłużona: {playerDeathHandled}");
        Debug.Log($"Prędkość ruchu: {moveSpeed}");
        Debug.Log($"Zasięg wykrywania: {detectionRadius}");
        Debug.Log($"Patrzy w prawo: {facingRight}");
        Debug.Log($"Obracanie włączone: {enableFlipping}");
        Debug.Log($"Użyj scale flip: {useScaleFlip}");
    }

    public void SetPlayer(Transform newPlayer)
    {
        // Ręczne ustawienie gracza
        player = newPlayer;
        Debug.Log($"[EnemyPatrolAI] Gracz ustawiony ręcznie na: {(player != null ? player.name : "BRAK")}");
    }

    public void SetPatrolPoints(Transform newPointA, Transform newPointB)
    {
        // Ręczne ustawienie punktów patrolowania
        pointA = newPointA;
        pointB = newPointB;
        SetInitialTarget();
        Debug.Log($"[EnemyPatrolAI] Punkty patrolowania ustawione ręcznie - A: {pointA?.position}, B: {pointB?.position}");
    }

    #endregion
}