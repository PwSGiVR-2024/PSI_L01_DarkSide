using UnityEngine;
using UnityEngine.SceneManagement;

public class changeLevel : MonoBehaviour
{
    public ScoreManager sm;

    private void Start()
    {
        sm = FindObjectOfType<ScoreManager>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            string currentScene = SceneManager.GetActiveScene().name;

            if (currentScene == "Level1" && ScoreManager.score == 5)
            {
                SceneTransitionManager.Instance.FadeToScene("Level2");
            }
            else if (currentScene == "Level2" && ScoreManager.score == 12)
            {
                SceneTransitionManager.Instance.FadeToScene("Level3");
            }
        }
    }
}