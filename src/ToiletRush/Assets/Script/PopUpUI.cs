using UnityEngine;
using System.Collections;

public class PopupUI : MonoBehaviour
{
    public float popDuration = 0.35f;
    public float startScale = 0.3f;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(Popup());
    }

    IEnumerator Popup()
    {
        float t = 0f;

        transform.localScale = Vector3.one * startScale;

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        while (t < popDuration)
        {
            t += Time.unscaledDeltaTime; // สำคัญ! เพราะ TimeScale = 0

            float p = t / popDuration;
            float ease = 1f - Mathf.Pow(1f - p, 3f); // Ease Out

            transform.localScale = Vector3.Lerp(
                Vector3.one * startScale,
                Vector3.one,
                ease
            );

            if (canvasGroup != null)
                canvasGroup.alpha = ease;

            yield return null;
        }

        transform.localScale = Vector3.one;

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }
}
