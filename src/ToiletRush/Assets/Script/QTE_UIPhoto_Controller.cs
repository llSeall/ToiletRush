using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class QTE_UI_Controller : MonoBehaviour
{
    [Header("Success Popup")]
    public Image successImage;
    public Sprite[] successSprites;
    public float popupDuration = 0.8f;
    public float popupBounceSpeed = 10f;
    public float popupBounceAmount = 0.3f;


    [Header("Bounce Effect")]
    public float bounceSpeed = 6f;
    public float bounceAmount = 0.2f;
    private Coroutine popupRoutine;

    [Header("Main UI")]
    public GameObject panel;
    public Image fillImage;
    public TextMeshProUGUI keyText;

    private float timer;
    private float maxTime;
    private KeyCode currentKey;
    private Action onSuccess;
    private Action onFail;
    private bool isRunning = false;
    private Vector3 panelOriginalScale;

    public void StartQTE(KeyCode key, float duration, Action success, Action fail)
    {
        successImage.gameObject.SetActive(false);

        currentKey = key;
        maxTime = duration;
        timer = duration;
        onSuccess = success;
        onFail = fail;

        keyText.text = key.ToString();
        fillImage.fillAmount = 1f;

        panelOriginalScale = panel.transform.localScale;

        panel.SetActive(true);
        isRunning = true;
    }

    void Update()
    {
        if (!isRunning) return;

        timer -= Time.deltaTime;

        fillImage.fillAmount = timer / maxTime;

        // เด้งปุ่ม
        float scaleOffset = Mathf.Sin(Time.time * bounceSpeed) * bounceAmount;
        panel.transform.localScale = panelOriginalScale * (1f + scaleOffset);

        if (Input.GetKeyDown(currentKey))
        {
            isRunning = false;
            StartCoroutine(SuccessSequence());
            return; // กัน Fail ทำงานซ้ำ
        }

        if (timer <= 0f)
        {
            Fail();
        }

    }
    IEnumerator SuccessSequence()
    {
        //  ปลดล็อก Player ก่อน
        onSuccess?.Invoke();

        //  ปิด QTE UI
        panel.SetActive(false);
        panel.transform.localScale = panelOriginalScale;

        //  แสดงภาพ
        if (popupRoutine != null)
            StopCoroutine(popupRoutine);

        popupRoutine = StartCoroutine(ShowSuccessPopup());
        yield return ShowSuccessPopup();
    }



    void Fail()
    {
        isRunning = false;

        panel.SetActive(false);
        keyText.transform.localScale = panelOriginalScale;

        onFail?.Invoke();
    }
    System.Collections.IEnumerator ShowSuccessPopup()
    {
        successImage.gameObject.SetActive(true);

        if (successImage == null || successSprites.Length == 0)
            yield break;

        // สุ่มภาพ
        int randomIndex = UnityEngine.Random.Range(0, successSprites.Length);
        successImage.sprite = successSprites[randomIndex];

        // รีเซ็ต scale ก่อนเปิด
        successImage.transform.localScale = Vector3.zero;

        // เปิด popup (มันถูกปิดไว้ตั้งแต่ต้นเกม)
        successImage.gameObject.SetActive(true);

        float elapsed = 0f;
        float showTime = popupDuration;

        // เด้งตอนเปิด
        while (elapsed < 0.25f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.25f;
            float bounce = Mathf.Sin(t * Mathf.PI);
            successImage.transform.localScale = Vector3.one * (1f + bounce * popupBounceAmount);
            yield return null;
        }

        // ค้างให้เห็นชัด ๆ
        yield return new WaitForSeconds(showTime);

        // รีเซ็ต scale
        successImage.transform.localScale = Vector3.one;

        // ปิด popup
        successImage.gameObject.SetActive(false);
    }


}
