using UnityEngine;

public class QuestTrigger : MonoBehaviour
{
    public enum TriggerType
    {
        PickUp,
        Deliver,
        StartMiniGame,
        MiniGameDeliver
    }

    public TriggerType triggerType;
    public QuestBase quest;

    [Header("Optional Image Effect")]
    public bool useImageEffect = false;
    public ImgTriggerEffect imgEffect;

    [Header("Sound")]
    public AudioClip questCompleteSound; //  เสียงตอนเควสสำเร็จ

    private void Start()
    {
        if (quest != null)
        {
            quest.OnQuestCompleted += OnQuestCompleted;
        }

        GetComponent<Collider>().isTrigger = true;
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
        //  เล่นเสียง (ไม่โดนตัด)
        if (questCompleteSound != null)
        {
            AudioSource.PlayClipAtPoint(questCompleteSound, transform.position);
        }

        if (triggerType == TriggerType.Deliver ||
            triggerType == TriggerType.MiniGameDeliver)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (quest == null) return;

        switch (triggerType)
        {
            case TriggerType.PickUp:
                if (quest is DeliveryQuest deliveryQuest)
                    deliveryQuest.PickUpItem();
                break;

            case TriggerType.Deliver:
                if (quest is DeliveryQuest deliveryQuest2)
                {
                    deliveryQuest2.DeliverItem();

                    if (useImageEffect && imgEffect != null)
                        imgEffect.PlayEffect();
                }
                break;

            case TriggerType.StartMiniGame:
                if (quest is MiniGameQuest miniGameQuest)
                    miniGameQuest.StartMiniGame();
                break;

            case TriggerType.MiniGameDeliver:
                if (quest is MiniGameQuest miniGameQuest2)
                {
                    if (!miniGameQuest2.miniGameCompleted)
                    {
                        Debug.Log("MiniGame not finished!");
                        return;
                    }

                    miniGameQuest2.DeliverAfterMiniGame();

                    if (useImageEffect && imgEffect != null)
                        imgEffect.PlayEffect();
                }
                break;
        }
    }
}