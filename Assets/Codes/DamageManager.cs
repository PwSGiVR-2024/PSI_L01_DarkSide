using UnityEngine;
using System.Collections;

public class DamageManager : MonoBehaviour
{
    #region Public Fields
    
    [Header("Attack Settings")]
    public int attackDamage = 1;
    public float attackRange = 2f;
    public float attackCooldown = 0.5f;
    public LayerMask enemyLayerMask = -1; // Które warstwy są wrogami

    [Header("Knockback Settings")]
    public bool enableKnockback = true;
    public float knockbackForce = 10f;
    public float knockbackDuration = 0.3f;
    public AnimationCurve knockbackCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));

    [Header("Attack Effects")]
    public GameObject attackEffect; // Efekt wizualny ataku
    public AudioClip attackSound; // Dźwięk ataku
    public Transform attackPoint; // Punkt z którego wychodzi atak

    [Header("Animation")]
    public Animator playerAnimator; // Animator gracza
    public string attackTriggerName = "Attack"; // Nazwa triggera w animatorze
    public float animationDelay = 0.1f; // Opóźnienie między animacją a atakiem
    
    #endregion

    #region Private Fields
    
    private float lastAttackTime = -Mathf.Infinity;
    private AudioSource audioSource;
    
    #endregion

    #region Unity Lifecycle
    
    void Start()
    {
        InitializeComponents();
        ValidateSettings();
    }

    void Update()
    {
        HandleInput();
    }
    
    #endregion

    #region Initialization
    
    private void InitializeComponents()
    {
        // Pobierz AudioSource lub dodaj jeśli nie ma
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && attackSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Jeśli brak attackPoint, użyj pozycji gracza
        if (attackPoint == null)
        {
            attackPoint = transform;
        }

        // Pobierz animator jeśli nie jest przypisany
        if (playerAnimator == null)
        {
            playerAnimator = GetComponent<Animator>();
        }
    }

    private void ValidateSettings()
    {
        if (attackDamage <= 0)
        {
            Debug.LogWarning($"[{name}] attackDamage should be greater than 0!");
            attackDamage = 1;
        }

        if (attackRange <= 0)
        {
            Debug.LogWarning($"[{name}] attackRange should be greater than 0!");
            attackRange = 1f;
        }

        if (attackCooldown < 0)
        {
            Debug.LogWarning($"[{name}] attackCooldown should not be negative!");
            attackCooldown = 0f;
        }
    }
    
    #endregion

    #region Input Handling
    
    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryAttack();
        }
    }
    
    #endregion

    #region Attack System
    
    private void TryAttack()
    {
        // Sprawdź cooldown
        if (Time.time < lastAttackTime + attackCooldown)
        {
            Debug.Log("Attack on cooldown!");
            return;
        }

        // Wykonaj atak z animacją
        PerformAttackWithAnimation();
        lastAttackTime = Time.time;
    }

    private void PerformAttackWithAnimation()
    {
        Debug.Log("Player attacks!");

        // Uruchom animację ataku
        TriggerPlayerAttackAnimation();

        // Jeśli jest opóźnienie animacji, poczekaj
        if (animationDelay > 0)
        {
            StartCoroutine(DelayedAttackCoroutine());
        }
        else
        {
            ExecuteAttack();
        }
    }

    private void ExecuteAttack()
    {
        PlayAttackEffects();
        AttackEnemiesInRange();
        
        // Zakończ animację ataku (jeśli nie ma opóźnienia)
        if (animationDelay <= 0)
        {
            StartCoroutine(EndAttackAnimationCoroutine());
        }
    }

    private void PlayAttackEffects()
    {
        // Dźwięk ataku
        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        // Efekt wizualny
        if (attackEffect != null)
        {
            GameObject effect = Instantiate(attackEffect, attackPoint.position, attackPoint.rotation);
            Destroy(effect, 2f);
        }
    }

    private void AttackEnemiesInRange()
    {
        // Atak okrężny (360°)
        Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayerMask);

        foreach (Collider2D enemy in enemies)
        {
            DealDamageToEnemy(enemy);
        }

        Debug.Log($"Hit {enemies.Length} enemies");
    }
    
    #endregion

    #region Damage System
    
    private void DealDamageToEnemy(Collider2D enemy)
    {
        bool damageDealt = AttemptDamage(enemy);

        if (damageDealt)
        {
            TriggerEnemyHitAnimation(enemy);
            
            if (enableKnockback)
            {
                ApplyKnockback(enemy);
            }
        }
    }

    private bool AttemptDamage(Collider2D enemy)
    {
        // Sprawdź czy wróg ma EnemyHealth
        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(attackDamage);
            Debug.Log($"Dealt {attackDamage} damage to {enemy.name}");
            return true;
        }

        // Sprawdź czy wróg ma EnemyPatrolAI (dla prostych wrogów)
        EnemyPatrolAI enemyAI = enemy.GetComponent<EnemyPatrolAI>();
        if (enemyAI != null && enemyAI.GetComponent<EnemyHealth>() == null)
        {
            Destroy(enemy.gameObject);
            Debug.Log($"Destroyed {enemy.name}");
            return true;
        }

        // Fallback dla wrogów z tagiem Enemy
        if (enemy.CompareTag("Enemy"))
        {
            Destroy(enemy.gameObject);
            Debug.Log($"Destroyed {enemy.name}");
            return true;
        }

        return false;
    }
    
    #endregion

    #region Animation System
    
    private void TriggerPlayerAttackAnimation()
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger(attackTriggerName);
            playerAnimator.SetBool("IsAttacking", true);
        }
    }

    private void TriggerEnemyHitAnimation(Collider2D enemy)
    {
        Animator enemyAnimator = enemy.GetComponent<Animator>();
        if (enemyAnimator != null)
        {
            enemyAnimator.SetTrigger("Hit");
            enemyAnimator.SetBool("IsHurt", true);
            
            StartCoroutine(EndEnemyHurtAnimationCoroutine(enemyAnimator));
        }
    }
    
    #endregion

    #region Knockback System
    
    private void ApplyKnockback(Collider2D enemy)
    {
        Vector2 knockbackDirection = CalculateKnockbackDirection(enemy);
        
        Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
        if (enemyRb != null)
        {
            ApplyPhysicsKnockback(enemyRb, knockbackDirection);
        }
        else
        {
            StartCoroutine(ApplyTransformKnockbackCoroutine(enemy.transform, knockbackDirection));
        }

        ApplyKnockbackToAI(enemy, knockbackDirection);
    }

    private Vector2 CalculateKnockbackDirection(Collider2D enemy)
    {
        return (enemy.transform.position - attackPoint.position).normalized;
    }

    private void ApplyPhysicsKnockback(Rigidbody2D enemyRb, Vector2 direction)
    {
        enemyRb.velocity = Vector2.zero;
        enemyRb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
        Debug.Log($"Applied impulse knockback: {direction * knockbackForce}");
    }

    private void ApplyKnockbackToAI(Collider2D enemy, Vector2 direction)
    {
        // EnemyPatrolAI knockback
        EnemyPatrolAI enemyAI = enemy.GetComponent<EnemyPatrolAI>();
        if (enemyAI != null && enemyAI.GetType().GetMethod("ApplyKnockback") != null)
        {
            enemyAI.SendMessage("ApplyKnockback", direction, SendMessageOptions.DontRequireReceiver);
        }

        // EnemyPlayerDetection AI disable
        EnemyPlayerDetection enemyDetection = enemy.GetComponent<EnemyPlayerDetection>();
        if (enemyDetection != null)
        {
            StartCoroutine(TemporarilyDisableAICoroutine(enemyDetection));
        }
    }
    
    #endregion

    #region Coroutines
    
    private IEnumerator DelayedAttackCoroutine()
    {
        yield return new WaitForSeconds(animationDelay);
        ExecuteAttack();
        
        // Zakończ animację ataku
        yield return new WaitForSeconds(0.1f);
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsAttacking", false);
        }
    }

    private IEnumerator EndAttackAnimationCoroutine()
    {
        yield return new WaitForSeconds(0.3f);
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsAttacking", false);
        }
    }

    private IEnumerator EndEnemyHurtAnimationCoroutine(Animator enemyAnimator)
    {
        yield return new WaitForSeconds(0.2f);
        if (enemyAnimator != null)
        {
            enemyAnimator.SetBool("IsHurt", false);
        }
    }

    private IEnumerator ApplyTransformKnockbackCoroutine(Transform enemyTransform, Vector2 direction)
    {
        Vector3 startPosition = enemyTransform.position;
        Vector3 targetPosition = startPosition + (Vector3)(direction * knockbackForce * 0.1f);

        float elapsedTime = 0f;

        while (elapsedTime < knockbackDuration)
        {
            if (enemyTransform == null) yield break;

            float progress = elapsedTime / knockbackDuration;
            float curveValue = knockbackCurve.Evaluate(progress);

            enemyTransform.position = Vector3.Lerp(startPosition, targetPosition, curveValue);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Debug.Log("Transform knockback completed");
    }

    private IEnumerator TemporarilyDisableAICoroutine(EnemyPlayerDetection enemyAI)
    {
        enemyAI.enabled = false;
        yield return new WaitForSeconds(knockbackDuration);
        
        if (enemyAI != null)
        {
            enemyAI.enabled = true;
        }
    }
    
    #endregion

    #region Debug & Visualization
    
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        DrawAttackRange();
        DrawAttackPoint();
        DrawKnockbackDirections();
    }

    private void DrawAttackRange()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    private void DrawAttackPoint()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPoint.position, 0.1f);
    }

    private void DrawKnockbackDirections()
    {
        if (!enableKnockback) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
            Gizmos.DrawRay(attackPoint.position, direction * (knockbackForce * 0.2f));
        }
    }
    
    #endregion
}