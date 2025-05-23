using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public RawImage[] lifeImages; // Tablica RawImage reprezentujących życie gracza
    private int currentLife; // Aktualna liczba żyć gracza

    void Start()
    {
        currentLife = lifeImages.Length;

        foreach (RawImage lifeImage in lifeImages)
        {
            if (lifeImage != null)
            {
                lifeImage.enabled = true; // Pokazuje wszystkie życia na start
            }
            else
            {
                Debug.LogError("Błąd przypisania RawImage");
            }
        }
    }

    private void HandleLifeGrow()
    {
        if (currentLife < lifeImages.Length) {
            lifeImages[currentLife].enabled = true; // Pokazuje kolejne życie
            currentLife++;
        }
        else {
            Debug.Log("Masz juz maksymalna ilosc zyc");
        }
    }

    private void HandleLifeLoss()
    {
        if (currentLife > 0) {
            lifeImages[currentLife - 1].enabled = false; // Ukrywa ostatnie życie
            currentLife--;

            if (currentLife == 0)
            {
                Debug.Log("Game Over! Player has no lives left.");
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Działa");
        if (collision.gameObject.CompareTag("Enemy")) {
            Debug.Log("Wykryto kolizje");
            HandleLifeLoss();
        }

        if (collision.gameObject.CompareTag("HP")) {
            Debug.Log("Zebrano dodatkowe zycie");
            HandleLifeGrow();
            Destroy(collision.gameObject);
        }
    }
}