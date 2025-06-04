using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBar : MonoBehaviour
{
    public BossHealth bossHealth;
    public Slider slider;
    public GameObject fillObject;

    private CanvasGroup canvasGroup;

    void Start()
    {
        slider.minValue = 0;
        slider.maxValue = bossHealth.health;
        slider.value = bossHealth.health;
        canvasGroup = slider.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = slider.gameObject.AddComponent<CanvasGroup>();
        }
    }

    void Update()
    {
        slider.value = bossHealth.health;

        if (slider.value <= 0)
        {
            fillObject.SetActive(false);
            if (!fading)
            {
                fading = true;
                StartCoroutine(FadeOutSlider());
            }
        }
        else
        {
            fillObject.SetActive(true);
        }
    }

    private bool fading = false;

    IEnumerator FadeOutSlider()
    {
        float fadeDuration = 7f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        slider.gameObject.SetActive(false);
    }
}
