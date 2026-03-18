using UnityEngine;

public class ShoutWave : MonoBehaviour
{
    public float expandSpeed = 3f;
    public float fadeSpeed = 2f;

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // ขยาย
        transform.localScale += Vector3.one * expandSpeed * Time.deltaTime;

        // จาง
        Color c = sr.color;
        c.a -= fadeSpeed * Time.deltaTime;
        sr.color = c;

        // ลบตัวเอง
        if (c.a <= 0)
            Destroy(gameObject);
    }
}