using System.Collections.Generic;
using UnityEngine;

public class WinTrigger : MonoBehaviour
{
    public enum WinCondition
    {
        NoQuestRequired,
        RequireQuest
    }
    [Header("Incomplete Quest Dialogue")]
    public DialogueSystem dialogueSystem;
    [TextArea(3, 5)]
    public string[] incompleteQuestLines;
    public Sprite portrait;

    [Header("Win Condition")]
    public WinCondition winCondition = WinCondition.NoQuestRequired;
    public QuestBase quest;

    [Header("UI")]
    public GameObject winCanvas;
    public StarUI starUI;
    public string levelName;

    private bool alreadyWon = false;

    private void Start()
    {
        if (winCondition == WinCondition.RequireQuest && quest != null)
        {
            quest.OnQuestCompleted += OnQuestCompleted;
        }
    }

    private void OnDestroy()
    {
        if (quest != null)
        {
            quest.OnQuestCompleted -= OnQuestCompleted;
        }
    }

    private void OnQuestCompleted()
    {
        Debug.Log("Quest completed, WinTrigger unlocked");

    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (alreadyWon) return;

        if (winCondition == WinCondition.NoQuestRequired)
        {
            WinGame();
        }
        else if (winCondition == WinCondition.RequireQuest)
        {
            if (quest != null && quest.IsCompleted)
            {
                WinGame();
            }
            else
            {
                ShowIncompleteQuestDialogue();
            }
        }
    }
    void ShowIncompleteQuestDialogue()
    {
        if (dialogueSystem == null) return;

        List<string> lines = new List<string>(incompleteQuestLines);
        dialogueSystem.StartDialogue(lines, portrait);
    }
    void WinGame()
    {
        alreadyWon = true;

        int stars = LevelResultManager.Instance.CalculateStars();

        SaveManager.SaveStars(levelName, stars);
        SaveManager.UnlockLevel(levelName);

        winCanvas.SetActive(true);
        starUI.ShowStars(stars);

        Time.timeScale = 0f;
    }
}