using UnityEngine;

public class UIBounce : MonoBehaviour
{
    public float bounceScale = 1.15f;
    public float bounceSpeed = 12f;

    private Vector3 originalScale;
    private bool bouncing;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    void OnEnable()
    {
        PlayBounce();
    }

    void Update()
    {
        if (!bouncing) return;

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            originalScale,
            Time.unscaledDeltaTime * bounceSpeed
        );

        if (Vector3.Distance(transform.localScale, originalScale) < 0.01f)
        {
            transform.localScale = originalScale;
            bouncing = false;
        }
    }

    public void PlayBounce()
    {
        transform.localScale = originalScale * bounceScale;
        bouncing = true;
    }
}
