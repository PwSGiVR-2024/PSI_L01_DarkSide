using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public AudioSource collectSound; // Mo¿na przypisaæ dŸwiêk do ka¿dego collectible
    public Text scoreText;           // UI do aktualizacji punktów
    public static int score = 0;     // static = wspólne dla wszystkich

    private void Start()
    {
        score = 0;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Zbiera tylko gracz
        {
            collectSound.Play();
            score++;
            scoreText.text = "Score: " + score;
            gameObject.SetActive(false); // Ukryj collectible
        }
    }
}