using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StartLevelUI : MonoBehaviour
{
    [Header("Start Panel")]
    public GameObject startPanel;
    public Image startImage;
    public Sprite startSprite;

    [Header("Countdown")]
    public GameObject countdownObject;   // GameObject ของรูป 3 2 1
    public Image countdownImage;
    public Sprite[] countdownSprites;    // ใส่ 3 รูป (3,2,1 ตามลำดับ)
    public float countdownInterval = 1f;

    [Header("Input")]
    public KeyCode closeKey = KeyCode.Space;

    private bool isShowingStart = false;
    private bool isCountingDown = false;

    void Start()
    {
        ShowStartUI();
    }

    void Update()
    {
        if (isShowingStart && Input.GetKeyDown(closeKey))
        {
            CloseStartUI();
        }
    }

    void ShowStartUI()
    {
        startPanel.SetActive(true);

        if (startImage != null && startSprite != null)
            startImage.sprite = startSprite;

        countdownObject.SetActive(false);

        Time.timeScale = 0f;   // หยุดเกม
        isShowingStart = true;
    }

    void CloseStartUI()
    {
        startPanel.SetActive(false);
        isShowingStart = false;

        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        isCountingDown = true;

        countdownObject.SetActive(true);

        for (int i = 0; i < countdownSprites.Length; i++)
        {
            countdownImage.sprite = countdownSprites[i];

            yield return StartCoroutine(PopAnimation());

            yield return new WaitForSecondsRealtime(countdownInterval);
        }

        countdownObject.SetActive(false);

        Time.timeScale = 1f;   //  เกมเริ่มจริง
        isCountingDown = false;
    }

    IEnumerator PopAnimation()
    {
        RectTransform rect = countdownObject.GetComponent<RectTransform>();

        float duration = 0.25f;
        float timer = 0f;

        Vector3 startScale = Vector3.zero;
        Vector3 overshoot = Vector3.one * 1.3f;
        Vector3 normal = Vector3.one;

        // ขยายเด้ง
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            rect.localScale = Vector3.Lerp(startScale, overshoot, timer / duration);
            yield return null;
        }

        // กลับมาขนาดปกติ
        timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            rect.localScale = Vector3.Lerp(overshoot, normal, timer / duration);
            yield return null;
        }

        rect.localScale = normal;
    }
}