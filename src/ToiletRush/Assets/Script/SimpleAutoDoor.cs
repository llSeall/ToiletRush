using UnityEngine;

public class SimpleSwingDoor : MonoBehaviour
{
    public enum DoorMode
    {
        OpenOnTrigger,   // ชนแล้วเปิดเลย
        RequireQTE       // ต้องเรียก OpenDoor() เอง
    }

    [Header("Door Mode")]
    public DoorMode doorMode = DoorMode.OpenOnTrigger;

    [Header("Door Setting")]
    public float openAngle = 90f;
    public float openSpeed = 3f;
    public bool openToRight = true;

    [Header("Collider")]
    public Collider blockCollider;

    private bool isOpening = false;
    private bool isOpen = false;

    private Quaternion closedRot;
    private Quaternion openRot;

    void Start()
    {
        closedRot = transform.rotation;

        float angle = openToRight ? openAngle : -openAngle;
        openRot = Quaternion.Euler(
            transform.eulerAngles.x,
            transform.eulerAngles.y + angle,
            transform.eulerAngles.z
        );
    }

    void Update()
    {
        if (!isOpening) return;

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            openRot,
            Time.deltaTime * openSpeed
        );

        if (Quaternion.Angle(transform.rotation, openRot) < 0.5f)
        {
            transform.rotation = openRot;
            isOpening = false;
            isOpen = true;

            if (blockCollider != null)
                blockCollider.enabled = false;

            Debug.Log("Door fully opened");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (doorMode != DoorMode.OpenOnTrigger) return;
        if (isOpen || isOpening) return;

        if (other.CompareTag("Player"))
        {
            isOpening = true;
            Debug.Log("Door opening (Normal Mode)");
        }
    }

    //  เรียกจาก QTE เท่านั้น
    public void OpenDoor()
    {
        if (isOpen || isOpening) return;

        isOpening = true;
        Debug.Log("Door opening (QTE Mode)");
    }
}