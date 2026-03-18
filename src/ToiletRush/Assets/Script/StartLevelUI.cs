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
    public GameObject countdownObject;
    public Image countdownImage;
    public Sprite[] countdownSprites;
    public float countdownInterval = 1f;

    [Header("Input")]
    public KeyCode closeKey = KeyCode.Space;

    [Header("Press Animation")]
    public RectTransform pressButton;
    public float pressAnimSpeed = 2f;
    public float pressScale = 1.15f;

    private bool isShowingStart = false;
    private bool isCountingDown = false;

    void Start()
    {
        ShowStartUI();
    }

    void Update()
    {
        if (!isShowingStart) return;

        // กด Spacebar
        if (Input.GetKeyDown(closeKey))
        {
            StartGame();
        }

        // Animation ปุ่มเด้ง
        if (pressButton != null)
        {
            float scale = 1 + Mathf.Sin(Time.unscaledTime * pressAnimSpeed) * (pressScale - 1);
            pressButton.localScale = Vector3.one * scale;
        }
    }

    void ShowStartUI()
    {
        startPanel.SetActive(true);

        if (startImage != null && startSprite != null)
            startImage.sprite = startSprite;

        countdownObject.SetActive(false);

        Time.timeScale = 0f;
        isShowingStart = true;
    }

    // ฟังก์ชันนี้ให้ UI Button เรียก
    public void StartGame()
    {
        if (!isShowingStart || isCountingDown) return;

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

        Time.timeScale = 1f;
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

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            rect.localScale = Vector3.Lerp(startScale, overshoot, timer / duration);
            yield return null;
        }

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