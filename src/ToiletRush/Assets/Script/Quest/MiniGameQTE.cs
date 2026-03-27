using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniGameQTE : MonoBehaviour
{
    public GameObject qtePanel;
    public Image[] arrowSlots;
    public Sprite upSprite;
    public Sprite downSprite;
    public Sprite leftSprite;
    public Sprite rightSprite;
    public MonoBehaviour playerMovementScript;
    public int qteLength = 5;
    private List<KeyCode> sequence = new List<KeyCode>();
    private int currentIndex = 0;
    private MiniGameQuest currentQuest;
    private bool isResetting = false;
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip successClip;
    public AudioClip failClip;
    [Header("Shake")]
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 10f;

    public void StartQTE(MiniGameQuest quest)
    {
        Debug.Log("StartQTE called");

        currentQuest = quest;

     
        GenerateSequence();
        ShowSequence();

        qtePanel.SetActive(true);

        // บังคับ layout คำนวณใหม่
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            qtePanel.GetComponent<RectTransform>()
        );

        if (playerMovementScript != null)
            playerMovementScript.enabled = false;
    }
    void GenerateSequence()
    {
        sequence.Clear();
        currentIndex = 0;

        KeyCode[] keys =
        {
        KeyCode.UpArrow,
        KeyCode.DownArrow,
        KeyCode.LeftArrow,
        KeyCode.RightArrow
    };

        for (int i = 0; i < qteLength; i++)
        {
            KeyCode randomKey = keys[Random.Range(0, keys.Length)];
            sequence.Add(randomKey);
        }
    }
    void ShowSequence()
    {
        for (int i = 0; i < arrowSlots.Length; i++)
        {
            if (i < sequence.Count)
            {
                arrowSlots[i].gameObject.SetActive(true);
                arrowSlots[i].sprite = GetSprite(sequence[i]);
                arrowSlots[i].color = Color.white;
            }
            else
            {
                arrowSlots[i].gameObject.SetActive(false);
            }
        }
    }

    Sprite GetSprite(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.UpArrow: return upSprite;
            case KeyCode.DownArrow: return downSprite;
            case KeyCode.LeftArrow: return leftSprite;
            case KeyCode.RightArrow: return rightSprite;
        }
        return null;
    }

    void Update()
    {
        if (!qtePanel.activeSelf) return;
        if (currentIndex >= sequence.Count) return;

        //  เช็คว่ามีการกดปุ่มก่อน
        KeyCode pressedKey = GetPressedArrowKey();

        if (pressedKey == KeyCode.None) return;

        //  กดถูก
        if (pressedKey == sequence[currentIndex])
        {
            if (audioSource && successClip)
                audioSource.PlayOneShot(successClip);

            arrowSlots[currentIndex].color = Color.green;
            arrowSlots[currentIndex].gameObject.SetActive(false);

            currentIndex++;

            LayoutRebuilder.ForceRebuildLayoutImmediate(
                qtePanel.GetComponent<RectTransform>()
            );

            if (currentIndex >= sequence.Count)
            {
                CompleteQTE();
            }
        }
        else
        if (!isResetting)
        {
            if (audioSource && failClip)
                audioSource.PlayOneShot(failClip);

            StartCoroutine(ResetQTE());
        }
        if (isResetting) return;

    }
    IEnumerator ShakeSelf()
    {
        RectTransform rect = GetComponent<RectTransform>();
        Vector3 originalPos = rect.anchoredPosition;

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            rect.anchoredPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        rect.anchoredPosition = originalPos;
    }
    KeyCode GetPressedArrowKey()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) return KeyCode.UpArrow;
        if (Input.GetKeyDown(KeyCode.DownArrow)) return KeyCode.DownArrow;
        if (Input.GetKeyDown(KeyCode.LeftArrow)) return KeyCode.LeftArrow;
        if (Input.GetKeyDown(KeyCode.RightArrow)) return KeyCode.RightArrow;

        return KeyCode.None;
    }
    IEnumerator ResetQTE()
    {
        isResetting = true;

        // สีแดง
        arrowSlots[currentIndex].color = Color.red;

        //  สั่น "ตัวมันเอง"
        StartCoroutine(ShakeSelf());

        yield return new WaitForSeconds(0.3f);

        GenerateSequence();
        ShowSequence();

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            GetComponent<RectTransform>()
        );

        isResetting = false;
    }
    void CompleteQTE()
    {
        qtePanel.SetActive(false);

        if (playerMovementScript != null)
            playerMovementScript.enabled = true;

        if (currentQuest != null)
            currentQuest.MiniGameSuccess();
    }
}