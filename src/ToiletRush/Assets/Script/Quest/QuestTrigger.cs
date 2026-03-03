using UnityEngine;

public class QuestTrigger : MonoBehaviour
{
    public enum TriggerType
    {
        PickUp,
        Deliver,
        StartMiniGame
    }

    public TriggerType triggerType;
    public QuestBase quest;

    [Header("Optional Image Effect")]
    public bool useImageEffect = false;   // << เปิดปิดการใช้ภาพ
    public ImgTriggerEffect imgEffect;    // << ลากสคริปต์ภาพมาใส่

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
        if (triggerType == TriggerType.StartMiniGame)
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

                    //  เรียกภาพเฉพาะกรณีที่เปิดไว้
                    if (useImageEffect && imgEffect != null)
                        imgEffect.PlayEffect();
                }
                break;

            case TriggerType.StartMiniGame:
                if (quest is MiniGameQuest miniGameQuest)
                    miniGameQuest.StartMiniGame();
                break;
        }
    }
}