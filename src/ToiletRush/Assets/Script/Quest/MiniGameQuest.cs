using UnityEngine;

public class MiniGameQuest : QuestBase
{
    public Animator playerAnimator;
    public MiniGameQTE qteSystem;
    public bool requireDeliveryAfterMiniGame = false;
    public QuestTrigger startMiniGameTrigger;
    private bool isMiniGameRunning = false;
    public bool miniGameCompleted = false;

    public void StartMiniGame()
    {
        if (IsCompleted) return;
        if (miniGameCompleted) return;
        if (isMiniGameRunning) return;
        AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsName("MiniGame_Loop"))
        {
            playerAnimator.CrossFade("MiniGame_Loop", 0.1f);
            playerAnimator.SetBool("IsPlayingMiniGame", true);
        }
        if (IsCompleted) return;
        if (isMiniGameRunning) return;
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
        miniGameCompleted = true;

        playerAnimator.SetBool("IsPlayingMiniGame", false);

        // ปิด Trigger เริ่มมินิเกม
        if (startMiniGameTrigger != null)
            startMiniGameTrigger.gameObject.SetActive(false);

        if (!requireDeliveryAfterMiniGame)
        {
            CompleteQuest();
        }
    }

    public void DeliverAfterMiniGame()
    {
        if (!miniGameCompleted) return;

        if (IsCompleted) return;

        CompleteQuest();
    }
}