using UnityEngine;

public class SimpleSwingDoor : MonoBehaviour
{
    [Header("Door Setting")]
    public float openAngle = 90f;
    public float openSpeed = 3f;
    public bool openToRight = true;

    [Header("Collider")]
    public Collider blockCollider; // ตัวที่กันผู้เล่น

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

        // เช็คว่าเปิดสุดแล้วหรือยัง
        if (Quaternion.Angle(transform.rotation, openRot) < 0.5f)
        {
            transform.rotation = openRot;
            isOpening = false;
            isOpen = true;

            // เปิดทางให้เดินผ่าน
            if (blockCollider != null)
                blockCollider.enabled = false;

            Debug.Log("Door fully opened");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isOpen || isOpening) return;

        if (other.CompareTag("Player"))
        {
            isOpening = true;
            Debug.Log("Door opening...");
        }
    }
}
