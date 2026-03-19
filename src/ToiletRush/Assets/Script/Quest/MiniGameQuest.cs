using UnityEngine;

public class MiniGameQuest : QuestBase
{
    public Animator playerAnimator;
    public MiniGameQTE qteSystem;

    [Header("Trigger References")]
    public QuestTrigger startMiniGameTrigger;
    public QuestTrigger miniGameDeliverTrigger; //  สำคัญ

    public bool requireDeliveryAfterMiniGame = false;

    private bool isMiniGameRunning = false;
    public bool miniGameCompleted = false;

    public void StartMiniGame()
    {
        if (IsCompleted) return;
        if (miniGameCompleted) return;
        if (isMiniGameRunning) return;

        isMiniGameRunning = true;

        // เล่น animation
        AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsName("MiniGame_Loop"))
        {
            playerAnimator.CrossFade("MiniGame_Loop", 0.1f);
            playerAnimator.SetBool("IsPlayingMiniGame", true);
        }

        Debug.Log("Start MiniGame");

        if (qteSystem != null)
            qteSystem.StartQTE(this);
    }
    public void MiniGameSuccess()
    {
        Debug.Log("MiniGame Success!");

        if (!isMiniGameRunning) return;

        isMiniGameRunning = false;
        miniGameCompleted = true;

        playerAnimator.SetBool("IsPlayingMiniGame", false);

        // ปิด trigger เริ่ม
        if (startMiniGameTrigger != null)
            startMiniGameTrigger.gameObject.SetActive(false);

        //  เช็คก่อนว่าต้องส่งของไหม
        if (requireDeliveryAfterMiniGame)
        {
            if (miniGameDeliverTrigger != null)
            {
                miniGameDeliverTrigger.gameObject.SetActive(true);
                Debug.Log("Deliver Trigger ENABLED");
            }
            else
            {
                Debug.LogWarning("Delivery required but trigger not set!");
            }
        }
        else
        {
            // จบเควสทันที (ไม่มี error)
            CompleteQuest();
        }
    }

    public void DeliverAfterMiniGame()
    {
        if (!miniGameCompleted) return;
        if (IsCompleted) return;

        Debug.Log("Deliver After MiniGame");

        CompleteQuest();
    }
}