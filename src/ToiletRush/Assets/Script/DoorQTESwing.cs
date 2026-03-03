using UnityEngine;
using UnityEngine.UI;

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
        if (hasTriggered) return;   //  กันชนซ้ำ

        hasTriggered = true;        //  บันทึกว่าเคยชนแล้ว
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

        if (playerAnimator != null)

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

        if (playerAnimator != null)

        door.OpenDoor();
    }
}