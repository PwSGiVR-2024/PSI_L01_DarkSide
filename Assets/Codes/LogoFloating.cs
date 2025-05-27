using UnityEngine;

public class LogoFloating : MonoBehaviour
{
    public float amplitude = 10f; // wysoko�� ruchu g�ra-d� (w pikselach)
    public float frequency = 1f;  // szybko�� ruchu

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float newY = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.localPosition = startPos + new Vector3(0f, newY, 0f);
    }
}
