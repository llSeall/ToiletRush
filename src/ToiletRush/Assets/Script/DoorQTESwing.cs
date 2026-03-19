using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(SimpleSwingDoor))]
public class DoorQTESwing : MonoBehaviour
{
    [Header("UI")]
    public GameObject qteUI;
    public Image progressCircle;

    [Header("Animation")]
    public Animator playerAnimator;

    [Header("QTE Setting")]
    public float increasePerPress = 0.1f;
    public float decayPerSecond = 0.3f;
    public float startScale = 0.15f;
    public float endScale = 1f;

    [Header("Control")]
    public MonoBehaviour playerMovement;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hitClip;
    public AudioClip successClip;

    [Header("Audio Timing")]
    public float openDelay = 0.05f; //  ปรับตรงนี้

    [Header("Audio Setting")]
    public float hitCooldown = 0.1f;
    private float lastHitTime;

    private bool hasTriggered = false;
    private float progress;
    private bool active;
    private SimpleSwingDoor door;

    void Start()
    {
        door = GetComponent<SimpleSwingDoor>();
        qteUI.SetActive(false);
    }

    void Update()
    {
        if (!active) return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            progress += increasePerPress;

            if (Time.time - lastHitTime > hitCooldown)
            {
                PlayHitSound();
                lastHitTime = Time.time;
            }
        }

        progress -= decayPerSecond * Time.deltaTime;
        progress = Mathf.Clamp01(progress);

        progressCircle.fillAmount = progress;
        progressCircle.transform.localScale =
            Vector3.one * Mathf.Lerp(startScale, endScale, progress);

        if (progress >= 1f)
        {
            Success();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (active) return;
        if (hasTriggered) return;

        hasTriggered = true;
        StartQTE();
    }

    void StartQTE()
    {
        if (playerAnimator != null)
            playerAnimator.SetBool("IsShakingDoor", true);

        progress = 0f;
        active = true;

        if (playerMovement != null)
            playerMovement.enabled = false;

        progressCircle.fillAmount = 0f;
        progressCircle.transform.localScale = Vector3.one * startScale;

        qteUI.SetActive(true);
    }

    void Success()
    {
        if (playerAnimator != null)
            playerAnimator.SetBool("IsShakingDoor", false);

        active = false;
        qteUI.SetActive(false);

        if (playerMovement != null)
            playerMovement.enabled = true;

        //  ใช้ coroutine sync เสียงกับประตู
        StartCoroutine(OpenDoorWithSound());
    }

    IEnumerator OpenDoorWithSound()
    {
        //  เล่นเสียงก่อน
        if (audioSource != null && successClip != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(successClip);
        }

        //  รอเล็กน้อยให้ตรงจังหวะ animation
        yield return new WaitForSeconds(openDelay);

        door.OpenDoor();
    }

    void PlayHitSound()
    {
        if (audioSource != null && hitClip != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(hitClip);
        }
    }
}