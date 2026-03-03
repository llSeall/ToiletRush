using UnityEngine;

public class MiniGameQuest : QuestBase
{
    public Animator playerAnimator;
    public MiniGameQTE qteSystem;

    private bool isMiniGameRunning = false;

    public void StartMiniGame()
    {
        AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);

        if (!stateInfo.IsName("MiniGame_Loop"))
        {
            playerAnimator.CrossFade("MiniGame_Loop", 0.1f);
            playerAnimator.SetBool("IsPlayingMiniGame", true);
        }
        if (IsCompleted) return;
        if (isMiniGameRunning) return;   // กันเรียกซ้ำ

        isMiniGameRunning = true;

        Debug.Log("QTE REF: " + qteSystem);
        Debug.Log("StartMiniGame called");

        if (qteSystem != null)
            qteSystem.StartQTE(this);
    }

    public void MiniGameSuccess()
    {
        if (!isMiniGameRunning) return;

        isMiniGameRunning = false;

        playerAnimator.SetBool("IsPlayingMiniGame", false);

        if (IsCompleted) return;

        CompleteQuest();
    }
}