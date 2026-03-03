using UnityEngine;

public class DeliveryQuest : QuestBase
{
    [Header("Delivery Objects")]
    public GameObject pickupObject;   // ตัวของที่ให้เก็บ
    public GameObject dropoffObject;  // จุดส่งของ

    private bool hasItem = false;

    private void Start()
    {
        // ตอนเริ่มเกม ซ่อนจุดส่ง
        if (dropoffObject != null)
            dropoffObject.SetActive(false);
    }

    public void PickUpItem()
    {
        if (IsCompleted) return;

        hasItem = true;
        Debug.Log("Item Picked Up");

        //  ซ่อนของที่เก็บ
        if (pickupObject != null)
            pickupObject.SetActive(false);

        //  แสดงจุดส่ง
        if (dropoffObject != null)
            dropoffObject.SetActive(true);
    }

    public void DeliverItem()
    {
        if (IsCompleted) return;
        if (!hasItem) return;

        CompleteQuest();

        // ส่งของเสร็จ อาจซ่อนจุดส่งด้วย
        if (dropoffObject != null)
            dropoffObject.SetActive(false);
    }
}