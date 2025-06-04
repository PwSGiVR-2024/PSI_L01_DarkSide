using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class BossHealth : MonoBehaviour
{

    public int health = 20;
    public bool isInvulnerable = false;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Boss boss;
    private bool phaseTwoTriggered = false;



    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boss = GetComponent<Boss>();

    }

    public void TakeDamage(int damage)
    {
        if (isInvulnerable) return;
        health -= damage;

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

    void EnterPhaseTwo()
    {
        spriteRenderer.color = new Color(1f, 0.5f, 0.5f);

        boss.speed *= 1.8f;
        animator.speed *= 1.6f;
    }


    void Die()
    {
        animator.SetTrigger("Death");
        StartCoroutine(FadeOutAndDestroy());

    }

    IEnumerator FadeOutAndDestroy()
    {
        float fadeDuration = 7f;
        float elapsed = 0f;
        Color originalColor = spriteRenderer.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }

}