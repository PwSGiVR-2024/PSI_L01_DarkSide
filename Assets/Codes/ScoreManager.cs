using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public AudioSource collectSound;
    public Text scoreText;
    public static int score = 0;

    private bool collected = false;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            score = 0;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!collected && other.CompareTag("Player"))
        {
            Collect();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collected && collision.gameObject.CompareTag("Player"))
        {
            Collect();
        }
    }

    private void Collect()
    {
        collected = true;

        GameObject tempAudio = new GameObject("TempAudio");
        AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
        tempSource.clip = collectSound.clip;
        tempSource.Play();
        Destroy(tempAudio, tempSource.clip.length);

        score++;
        scoreText.text = "Score: " + score;

        gameObject.SetActive(false);
    }
}