using UnityEngine;

public class NeedKeyBubbleUI : MonoBehaviour
{
    public float showTime = 2f;
    public float popScale = 1.2f;
    public float popSpeed = 12f;
    public float floatHeight = 20f;
    public float floatSpeed = 2f;
    public float shakeAmount = 2f;
    public float shakeSpeed = 20f;

    Vector3 startPos;

    void Awake()
    {
        startPos = transform.localPosition;
        transform.localScale = Vector3.zero;
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        transform.localScale = Vector3.zero;
        transform.localPosition = startPos;
        Invoke(nameof(Hide), showTime);
    }

    void Update()
    {
        // เด้ง
        float t = Mathf.PingPong(Time.time * popSpeed, 1f);
        float scale = Mathf.Lerp(1f, popScale, t);
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            Vector3.one * scale,
            Time.deltaTime * popSpeed
        );

        // ลอย + สั่น
        float y = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        float x = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
        transform.localPosition = startPos + new Vector3(x, y, 0);
    }

    void Hide()
    {
        gameObject.SetActive(false);
    }
}
