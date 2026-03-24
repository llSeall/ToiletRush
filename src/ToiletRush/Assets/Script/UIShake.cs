using UnityEngine;

public class UIShake : MonoBehaviour
{
    public RectTransform target;
    public float duration = 0.2f;
    public float strength = 10f;

    private Vector3 originalPos;
    private float timer;
    private bool isShaking = false;

    void Awake()
    {
        if (target == null)
            target = GetComponent<RectTransform>();

        originalPos = target.anchoredPosition;
    }

    void Update()
    {
        if (!isShaking) return;

        timer -= Time.deltaTime;

        if (timer > 0)
        {
            Vector2 offset = Random.insideUnitCircle * strength;
            target.anchoredPosition = originalPos + (Vector3)offset;
        }
        else
        {
            target.anchoredPosition = originalPos;
            isShaking = false;
        }
    }

    public void Shake()
    {
        timer = duration;
        isShaking = true;
    }
}