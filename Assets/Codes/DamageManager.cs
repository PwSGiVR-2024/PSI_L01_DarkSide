using UnityEngine;
using System.Collections;

public class DamageManager : MonoBehaviour
{
    #region Public Fields

    [Header("Attack Settings")]
    public int attackDamage = 1;
    public float attackRange = 2f;
    public float attackCooldown = 0.7f;
    public LayerMask enemyLayerMask = -1;

    [Header("Knockback Settings")]
    public bool enableKnockback = true;
    public float knockbackForce = 10f;
    public float knockbackDuration = 0.3f;
    public AnimationCurve knockbackCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));

    [Header("Attack Effects")]
    public GameObject attackEffect;
    public AudioSource attackSound; // ← AudioSource zamiast AudioClip
    public Transform attackPoint;

    [Header("Animation")]
    public Animator playerAnimator;
    public string attackTriggerName = "Attack";
    public float animationDelay = 0.05f;

    #endregion

    #region Private Fields

    private float lastAttackTime = -Mathf.Infinity;

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
        if (attackPoint == null)
        {
            attackPoint = transform;
        }

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
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryAttack();
        }
    }

    #endregion

    #region Attack System

    private void TryAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown)
        {
            Debug.Log("Attack on cooldown!");
            return;
        }

        PerformAttackWithAnimation();
        lastAttackTime = Time.time;
    }

    private void PerformAttackWithAnimation()
    {
        Debug.Log("Player attacks!");
        TriggerPlayerAttackAnimation();

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

        if (animationDelay <= 0)
        {
            StartCoroutine(EndAttackAnimationCoroutine());
        }
    }

    private void PlayAttackEffects()
    {
        if (attackEffect != null)
        {
            GameObject effect = Instantiate(attackEffect, attackPoint.position, attackPoint.rotation);
            Destroy(effect, 2f);
        }

        if (attackSound != null && attackSound.clip != null)
        {
            attackSound.PlayOneShot(attackSound.clip);
        }
    }

    private void AttackEnemiesInRange()
    {
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
        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(attackDamage);
            Debug.Log($"Dealt {attackDamage} damage to {enemy.name}");
            return true;
        }

        BossHealth bossHealth = enemy.GetComponent<BossHealth>();
        if (bossHealth != null)
        {
            bossHealth.TakeDamage(attackDamage);
            Debug.Log($"Dealt {attackDamage} damage to BOSS: {enemy.name}");
            return true;
        }

        EnemyPatrolAI enemyAI = enemy.GetComponent<EnemyPatrolAI>();
        if (enemyAI != null && enemyAI.GetComponent<EnemyHealth>() == null)
        {
            Destroy(enemy.gameObject);
            Debug.Log($"Destroyed {enemy.name}");
            return true;
        }

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
        EnemyPatrolAI enemyAI = enemy.GetComponent<EnemyPatrolAI>();
        if (enemyAI != null && enemyAI.GetType().GetMethod("ApplyKnockback") != null)
        {
            enemyAI.SendMessage("ApplyKnockback", direction, SendMessageOptions.DontRequireReceiver);
        }

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

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPoint.position, 0.1f);

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