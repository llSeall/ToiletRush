using UnityEngine;
using System;

public abstract class QuestBase : MonoBehaviour
{
    public string questName;
    public bool IsCompleted { get; protected set; }

    public event Action OnQuestCompleted;

    protected void CompleteQuest()
    {
        if (IsCompleted) return;

        IsCompleted = true;
        Debug.Log("Quest Completed: " + questName);
        OnQuestCompleted?.Invoke();
    }
}