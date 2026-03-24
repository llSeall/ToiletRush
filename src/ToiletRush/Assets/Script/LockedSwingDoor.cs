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

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip popClip;       // UI pop
    public AudioClip lockedClip;    //  เสียงล็อค
    public AudioClip unlockClip;    //  ปลดล็อค

    [Header("Open Setting")]
    public float openAngle = 90f;
    public float openSpeed = 3f;
    public bool openToRight = true;

    private bool isOpening;
    private bool isOpen;
    private bool hasUnlocked = false;

    private Quaternion openRot;

    private bool hasShownUI = false;

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

        //  ไม่มี key
        if (!inventory.HasKey(requiredKeyID))
        {
            if (needKeyUI != null)
                needKeyUI.SetActive(true);

            if (!hasShownUI)
            {
                PlayLockedFeedback(); //  เล่นพร้อมกัน
                hasShownUI = true;
            }

            return;
        }

        //  มี key
        if (!hasUnlocked)
        {
            PlayUnlockSound();
            hasUnlocked = true;
        }

        isOpening = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        hasShownUI = false;

        if (needKeyUI != null)
            needKeyUI.SetActive(false);
    }

    //  pop + locked พร้อมกัน
    void PlayLockedFeedback()
    {
        if (audioSource == null) return;

        if (popClip != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(popClip);
        }

        if (lockedClip != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1f);
            audioSource.PlayOneShot(lockedClip);
        }
    }

    //  unlock
    void PlayUnlockSound()
    {
        if (audioSource != null && unlockClip != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(unlockClip);
        }
    }
}