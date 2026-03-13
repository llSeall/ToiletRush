using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
        // ป้องกัน input ตอน QTE ไม่เปิด
        if (!qtePanel.activeSelf) return;

        // ป้องกัน index เกิน
        if (currentIndex >= sequence.Count) return;

        if (Input.GetKeyDown(sequence[currentIndex]))
        {
            arrowSlots[currentIndex].color = Color.green;

            // ซ่อน slot ที่กดแล้ว
            arrowSlots[currentIndex].gameObject.SetActive(false);

            currentIndex++;

            // อัปเดต layout ใหม่
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                qtePanel.GetComponent<RectTransform>()
            );

            if (currentIndex >= sequence.Count)
            {
                CompleteQTE();
            }
        }
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