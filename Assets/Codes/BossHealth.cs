using System.Collections;
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    public int health = 20;
    public bool isInvulnerable = false;

    public Material flashMaterial;  

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Boss boss;
    private bool phaseTwoTriggered = false;
    private Material originalMaterial;
    private Coroutine flashCoroutine;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boss = GetComponent<Boss>();

        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.sharedMaterial;
            spriteRenderer.material = Instantiate(originalMaterial);
        }
    }

    public void TakeDamage(int damage)
    {

        if (isInvulnerable) return;

        health -= damage;
        if (flashMaterial != null && spriteRenderer != null)
        {
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);

            flashCoroutine = StartCoroutine(DamageFlash());
        }

        if (!phaseTwoTriggered && health <= (boss.maxHealth / 2))
        {
            phaseTwoTriggered = true;
            EnterPhaseTwo();
        }

        if (health <= 0)
        {
            Die();
        }
    }

    IEnumerator DamageFlash()
    {
        spriteRenderer.material = flashMaterial;
        yield return new WaitForSeconds(0.12f);
        spriteRenderer.material = originalMaterial;
        flashCoroutine = null;
    }

    void EnterPhaseTwo()
    {
        spriteRenderer.color = new Color(1f, 0.5f, 0.5f);
        boss.speed *= 1.8f;
        animator.speed *= 1.6f;
    }

    void Die()
    {
        animator.SetTrigger("Death");
        StartCoroutine(HandleDeathAndTransition());
    }

    IEnumerator HandleDeathAndTransition()
    {
        float fadeOutTime = 7f;
        float elapsed = 0f;
        Color originalColor = spriteRenderer.color;

        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutTime);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        yield return new WaitForSeconds(1f);
        SceneTransitionManager.Instance.FadeToScene("MainMenu");
    }
}
