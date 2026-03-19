using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SimpleSwingDoor : MonoBehaviour
{
    public enum DoorMode
    {
        OpenOnTrigger,
        RequireQTE
    }

    [Header("Door Mode")]
    public DoorMode doorMode = DoorMode.OpenOnTrigger;

    [Header("Door Setting")]
    public float openAngle = 90f;
    public float openSpeed = 3f;
    public bool openToRight = true;

    [Header("Collider")]
    public Collider blockCollider;

    [Header("Sound")]
    public AudioClip openSound;   // เสียงเปิดประตู
    private AudioSource audioSource;

    private bool isOpening = false;
    private bool isOpen = false;

    private Quaternion closedRot;
    private Quaternion openRot;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

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
            OpenDoor();
        }
    }

    // ใช้ทั้ง QTE และ Trigger
    public void OpenDoor()
    {
        if (isOpen || isOpening) return;

        isOpening = true;

        //  เล่นเสียง
        if (openSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(openSound);
        }

        Debug.Log("Door opening");
    }
}