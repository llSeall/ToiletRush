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

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip winSound;

    private bool alreadyWon = false;

    private void Start()
    {
        if (winCondition == WinCondition.RequireQuest && quest != null)
        {
            quest.OnQuestCompleted += OnQuestCompleted;
        }

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
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

        PlayWinSound();

        // ใช้ระบบใหม่
        bool[] starResults = LevelResultManager.Instance.GetStarResults();

        int starCount = 0;
        foreach (bool s in starResults)
        {
            if (s) starCount++;
        }

        SaveManager.SaveStars(levelName, starCount);
        SaveManager.UnlockLevel(levelName);

        winCanvas.SetActive(true);
        starUI.ShowStars(starResults);

        Time.timeScale = 0f;
    }

    void PlayWinSound()
    {
        if (audioSource == null || winSound == null)
            return;

        audioSource.Stop();
        audioSource.PlayOneShot(winSound);
    }
}