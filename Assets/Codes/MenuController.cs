using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameObject start;
    public GameObject settings;
    public GameObject quitSettings;
    public GameObject quitGame;
    public GameObject slider;
    public AudioSource audio;

    public void playClick()
    {
        audio.Play();
        // tutaj doda sie przejscie do 1 poziomu
    }
    public void settingsClick()
    {
        audio.Play();
        start.SetActive(false);
        settings.SetActive(false);
        quitGame.SetActive(false);
        slider.SetActive(true);
        quitSettings.SetActive(true);
    }

    public void quitSettingsClick()
    {
        audio.Play();
        start.SetActive(true);
        settings.SetActive(true);
        quitGame.SetActive(true);
        slider.SetActive(false);
        quitSettings.SetActive(false);
    }

    public void quitClick()
    {
        audio.Play();
        Application.Quit();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
