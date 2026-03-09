using UnityEngine;

public class SpriteFadeBlink : MonoBehaviour
{
    public float blinkSpeed = 2f;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        Color c = sr.color;
        c.a = Mathf.PingPong(Time.time * blinkSpeed, 1f);
        sr.color = c;
    }
}