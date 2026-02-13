using UnityEngine;
using System.Collections.Generic;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueSystem dialogueSystem;
    [TextArea(3, 5)]
    public string[] dialogueLines;
    public Sprite portrait;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;

            List<string> lines = new List<string>(dialogueLines);
            dialogueSystem.StartDialogue(lines, portrait);
        }
    }
}
