using UnityEngine;

public class LockedSwingDoor : MonoBehaviour
{
    [Header("References")]
    public Transform doorPivot;
    public Collider blockCollider;

    [Header("Key")]
    public string requiredKeyID = "RedKey";

    [Header("UI")]
    public GameObject needKeyUI;

    [Header("Open Setting")]
    public float openAngle = 90f;
    public float openSpeed = 3f;
    public bool openToRight = true;

    private bool isOpening;
    private bool isOpen;

    private Quaternion openRot;

    void Start()
    {
        if (doorPivot == null)
            doorPivot = transform.parent;

        float angle = openToRight ? openAngle : -openAngle;
        openRot = Quaternion.Euler(
            doorPivot.eulerAngles.x,
            doorPivot.eulerAngles.y + angle,
            doorPivot.eulerAngles.z
        );
    }

    void Update()
    {
        if (!isOpening) return;

        doorPivot.rotation = Quaternion.Lerp(
            doorPivot.rotation,
            openRot,
            Time.deltaTime * openSpeed
        );

        if (Quaternion.Angle(doorPivot.rotation, openRot) < 0.5f)
        {
            doorPivot.rotation = openRot;
            isOpening = false;
            isOpen = true;

            if (blockCollider != null)
                blockCollider.enabled = false;
        }
    }
    void OnTriggerStay(Collider other)
    {
        if (isOpen || isOpening) return;
        if (!other.CompareTag("Player")) return;

        PlayerKeyInventory inventory =
            other.GetComponentInParent<PlayerKeyInventory>();

        if (inventory == null) return;

        // ‰¡Ë¡’°ÿ≠·®
        if (!inventory.HasKey(requiredKeyID))
        {
            Debug.Log("Door locked, need key: " + requiredKeyID);

            if (needKeyUI != null)
                needKeyUI.SetActive(true);

            return;
        }

        // ¡’°ÿ≠·® ‡ª‘¥ª√–µŸ
        Debug.Log("Key accepted, opening door");
        isOpening = true;
    }

}
