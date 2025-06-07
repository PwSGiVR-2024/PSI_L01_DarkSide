using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameObject start;
    public GameObject settings;
    public GameObject quitSettings;
    public GameObject quitGame;
    public GameObject slider;
    public GameObject volumeText;
    public Slider volumeSlider;
    public AudioSource audio;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void playClick()
    {
        audio.Play();
        SceneTransitionManager.Instance.FadeToScene("Level1");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNewGame();
        }
        // tutaj doda sie przejscie do 1 poziomu
    }
    public void settingsClick()
    {
        audio.Play();
        start.SetActive(false);
        settings.SetActive(false);
        quitGame.SetActive(false);
        slider.SetActive(true);
        volumeText.SetActive(true);
        quitSettings.SetActive(true);
    }

    public void quitSettingsClick()
    {
        audio.Play();
        start.SetActive(true);
        settings.SetActive(true);
        quitGame.SetActive(true);
        slider.SetActive(false);
        volumeText.SetActive(false);
        quitSettings.SetActive(false);
    }

    public void quitClick()
    {
        audio.Play();
        Application.Quit();
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }
}
