using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundScaler : MonoBehaviour
{
    void Start()
    {
        FitBackgroundToScreen();
    }

    void FitBackgroundToScreen()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

        float screenHeight = Camera.main.orthographicSize * 2f;
        float screenWidth = screenHeight * Screen.width / Screen.height;

        Vector2 spriteSize = sr.sprite.bounds.size;

        Vector3 scale = transform.localScale;
        scale.x = screenWidth / spriteSize.x;
        scale.y = screenHeight / spriteSize.y;
        transform.localScale = scale;

        // Ustawienie pozycji t³a tak, ¿eby by³o zawsze za kamer¹
        transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, 10f);
    }
}
