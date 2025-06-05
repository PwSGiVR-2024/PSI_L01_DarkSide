using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public AudioSource collectSound;
    public Text scoreText; // UI do aktualizacji punktów
    public static int score = 0;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            score = 0;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Utwórz tymczasowy obiekt z AudioSource
            GameObject tempAudio = new GameObject("TempAudio");
            AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
            tempSource.clip = collectSound.clip;
            tempSource.Play();

            // Zniszcz tymczasowy obiekt po zakoñczeniu dŸwiêku
            Destroy(tempAudio, tempSource.clip.length);

            // Zwiêksz wynik i ukryj collectible
            score++;
            scoreText.text = "Score: " + score;
            gameObject.SetActive(false);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Utwórz tymczasowy obiekt z AudioSource
            GameObject tempAudio = new GameObject("TempAudio");
            AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
            tempSource.clip = collectSound.clip;
            tempSource.Play();

            // Zniszcz tymczasowy obiekt po zakoñczeniu dŸwiêku
            Destroy(tempAudio, tempSource.clip.length);

            // Zwiêksz wynik i ukryj collectible
            score++;
            scoreText.text = "Score: " + score;
            gameObject.SetActive(false);
        }
    }
}