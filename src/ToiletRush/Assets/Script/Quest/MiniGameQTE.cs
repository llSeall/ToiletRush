using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MiniGameQTE : MonoBehaviour
{
    public GameObject qtePanel;
    public Image[] arrowSlots; // 5 ช่อง
    public Sprite upSprite;
    public Sprite downSprite;
    public Sprite leftSprite;
    public Sprite rightSprite;

    public MonoBehaviour playerMovementScript; // ลากสคริปต์ควบคุมเดินใส่

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

        if (playerMovementScript != null)
            playerMovementScript.enabled = false;
    }

    void GenerateSequence()
    {
        sequence.Clear();
        currentIndex = 0;

        KeyCode[] keys = { KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow };

        for (int i = 0; i < 5; i++)
        {
            KeyCode randomKey = keys[Random.Range(0, keys.Length)];
            sequence.Add(randomKey);
        }
    }

    void ShowSequence()
    {
        for (int i = 0; i < arrowSlots.Length; i++)
        {
            if (i >= sequence.Count) break;

            arrowSlots[i].sprite = GetSprite(sequence[i]);
            arrowSlots[i].color = Color.white;
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

        if (Input.GetKeyDown(sequence[currentIndex]))
        {
            arrowSlots[currentIndex].color = Color.green;
            currentIndex++;

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