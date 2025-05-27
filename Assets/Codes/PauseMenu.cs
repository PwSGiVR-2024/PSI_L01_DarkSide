using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public bool isPaused = false;
    public AudioSource audio;
    public Slider volumeSlider;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    private void Start()
    {
        Time.timeScale = 1f; // Wznawia czas
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void Resume()
    {
        audio.Play();
        pauseMenu.SetActive(false);
        Time.timeScale = 1f; // Wznawia czas
        isPaused = false;
    }

    void Pause()
    {
        audio.Play();
        pauseMenu.SetActive(true);
        Time.timeScale = 0f; // Zatrzymuje grê
        isPaused = true;
    }

    public void QuitGame()
    {
        // Powrót do menu g³ównego
        audio.Play();
        SceneManager.LoadScene("MainMenu");
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }
}