using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StarUI : MonoBehaviour
{
    public Image[] stars;
    public Sprite starOn;
    public Sprite starOff;

    [Header("Animation")]
    public float popDuration = 0.3f;
    public float delayBetweenStars = 0.2f;
    public float overshootMultiplier = 1.15f;

    private Vector3[] originalScales;

    void Awake()
    {
        // จำขนาดตั้งต้นของแต่ละดวง
        originalScales = new Vector3[stars.Length];
        for (int i = 0; i < stars.Length; i++)
        {
            originalScales[i] = stars[i].transform.localScale;
        }
    }

    public void ShowStars(int count)
    {
        StopAllCoroutines();
        StartCoroutine(AnimateStars(count));
    }

    IEnumerator AnimateStars(int count)
    {
        // รีเซ็ต
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].sprite = starOff;
            stars[i].transform.localScale = Vector3.zero;

            CanvasGroup cg = GetCanvasGroup(stars[i]);
            cg.alpha = 0f;
        }

        yield return null;

        for (int i = 0; i < stars.Length; i++)
        {
            if (i < count)
            {
                stars[i].sprite = starOn;
                yield return StartCoroutine(PopStar(stars[i], originalScales[i]));
                yield return new WaitForSecondsRealtime(delayBetweenStars);
            }
            else
            {
                stars[i].sprite = starOff;
                stars[i].transform.localScale = originalScales[i];
                GetCanvasGroup(stars[i]).alpha = 1f;
            }
        }
    }

    IEnumerator PopStar(Image star, Vector3 targetScale)
    {
        CanvasGroup cg = GetCanvasGroup(star);
        float t = 0f;

        while (t < popDuration)
        {
            t += Time.unscaledDeltaTime;

            float p = t / popDuration;
            float ease = 1f - Mathf.Pow(1f - p, 3f);

            float scaleMultiplier = Mathf.Lerp(0f, overshootMultiplier, ease);
            star.transform.localScale = targetScale * scaleMultiplier;
            cg.alpha = ease;

            yield return null;
        }

        // จบที่ขนาดจริง
        star.transform.localScale = targetScale;
        cg.alpha = 1f;
    }

    CanvasGroup GetCanvasGroup(Image img)
    {
        CanvasGroup cg = img.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = img.gameObject.AddComponent<CanvasGroup>();
        return cg;
    }
}
