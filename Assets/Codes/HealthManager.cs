using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public RawImage[] lifeImages;
    public Animator animator;
    private int currentLife; // Liczba żyć

    void Start()
    {
        currentLife = lifeImages.Length;

        foreach (RawImage lifeImage in lifeImages)
        {
            if (lifeImage != null)
            {
                lifeImage.enabled = true;
            }
            else
            {
                Debug.LogError("Błąd przypisania RawImage");
            }
        }
    }

    private void HandleLifeGrow()
    {
        if (currentLife < lifeImages.Length)
        {
            lifeImages[currentLife].enabled = true;
            currentLife++;
        }
        else
        {
            Debug.Log("Masz już maksymalną ilość żyć");
        }
    }

    private void HandleLifeLoss()
    {
        if (currentLife > 0)
        {
            lifeImages[currentLife - 1].enabled = false;
            currentLife--;

            if (currentLife == 0)
            {
                Debug.Log("Game Over! Player has no lives left.");

                if (animator != null)
                {
                    animator.SetBool("IsDead", true); // Uruchamia animację śmierci
                }
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            HandleLifeLoss();
        }

        if (collision.gameObject.CompareTag("HP"))
        {
            HandleLifeGrow();
            Destroy(collision.gameObject);
        }
    }
}
