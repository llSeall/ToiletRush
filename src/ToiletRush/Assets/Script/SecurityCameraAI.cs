using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SecurityCameraAI : MonoBehaviour
{
    [Header("Rotation")]
    public float rotateAngle = 45f;
    public float rotateSpeed = 30f;
    public Transform rotator;
    private Quaternion baseRotation;

    [Header("Disable")]
    public GameObject visionObject;

    [Header("Vision")]
    public float viewDistance = 8f;
    [Range(0, 180)] public float viewAngle = 60f;
    public LayerMask obstacleMask;

    [Header("Alert")]
    public float alertCooldown = 5f;
    public Renderer visionRenderer;
    public Color normalColor = Color.green;
    public Color alertColor = Color.red;

    [Header("UI Alert Flash")]
    public GameObject alertImageUI;
    public float alertImageDuration = 0.8f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip alertSound;

    private Coroutine alertUIRoutine;

    private Transform player;
    private float currentAngle;
    private int direction = 1;

    private bool canAlert = true;
    private bool isDisabled = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        currentAngle = -rotateAngle;

        if (rotator != null)
            baseRotation = rotator.localRotation;

        UpdateVisionColor(normalColor);

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isDisabled) return;

        RotateCamera();
        CheckVision();
    }

    // ---------- ROTATION ----------
    void RotateCamera()
    {
        currentAngle += direction * rotateSpeed * Time.deltaTime;

        if (Mathf.Abs(currentAngle) >= rotateAngle)
            direction *= -1;

        if (rotator != null)
        {
            rotator.localRotation =
                baseRotation * Quaternion.Euler(0, currentAngle, 0);
        }
    }

    // ---------- VISION ----------
    void CheckVision()
    {
        if (!canAlert || isDisabled || rotator == null || player == null)
            return;

        // ---------- ORIGIN ----------
        Vector3 origin = rotator.position
                       + rotator.forward * 0.6f
                       + Vector3.down * 0.2f; // กดลงเล็กน้อยให้เหมาะกับกล้องสูง

        // ---------- TARGET POINTS (หลายจุด) ----------
        Vector3[] targets = new Vector3[]
        {
        player.position + Vector3.up * 1.6f, // หัว
        player.position + Vector3.up * 1.0f, // ลำตัว
        player.position + Vector3.up * 0.3f  // ขา
        };

        foreach (var target in targets)
        {
            Vector3 dir = (target - origin).normalized;
            float distance = Vector3.Distance(origin, target);

            if (distance > viewDistance)
                continue;

            // ---------- ANGLE (ignore Y) ----------
            Vector3 flatForward = new Vector3(rotator.forward.x, 0, rotator.forward.z);
            Vector3 flatDir = new Vector3(dir.x, 0, dir.z);

            float angle = Vector3.Angle(flatForward, flatDir);
            if (angle > viewAngle * 0.5f)
                continue;

            // ---------- SPHERECAST ----------
            if (Physics.SphereCast(
                origin,
                0.3f, // ความกว้างการมอง (ปรับได้)
                dir,
                out RaycastHit hit,
                distance,
                obstacleMask,
                QueryTriggerInteraction.Ignore
            ))
            {
                if (hit.transform.root.CompareTag("Player"))
                {
                    AlertNearestGuard();
                    return;
                }
            }

            Debug.DrawRay(origin, dir * distance, Color.red);
        }
    }

    // ---------- ALERT ----------
    void AlertNearestGuard()
    {
        canAlert = false;

        UpdateVisionColor(alertColor);
        ShowAlertUI();
        PlayAlertSound();

        GuardAI[] guards = FindObjectsOfType<GuardAI>();
        GuardAI nearest = null;
        float minDist = Mathf.Infinity;

        foreach (GuardAI guard in guards)
        {
            float dist = Vector3.Distance(transform.position, guard.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = guard;
            }
        }

        if (nearest != null)
            nearest.Investigate(transform.position);

        Invoke(nameof(ResetAlert), alertCooldown);
    }

    void PlayAlertSound()
    {
        if (audioSource == null || alertSound == null)
            return;

        audioSource.Stop();
        audioSource.PlayOneShot(alertSound);
    }

    void ShowAlertUI()
    {
        if (alertImageUI == null)
            return;

        if (alertUIRoutine != null)
            StopCoroutine(alertUIRoutine);

        alertUIRoutine = StartCoroutine(AlertUIFlash());
    }

    IEnumerator AlertUIFlash()
    {
        alertImageUI.SetActive(true);
        yield return new WaitForSeconds(alertImageDuration);
        alertImageUI.SetActive(false);
    }

    void ResetAlert()
    {
        canAlert = true;
        UpdateVisionColor(normalColor);
    }

    // ---------- DISABLE ----------
    public void DisableCamera()
    {
        isDisabled = true;
        UpdateVisionColor(Color.gray);
    }

    public void DisableCameraFully()
    {
        isDisabled = true;
        canAlert = false;

        enabled = false;

        if (visionObject != null)
            visionObject.SetActive(false);

        UpdateVisionColor(Color.gray);
    }

    void UpdateVisionColor(Color c)
    {
        if (visionRenderer != null)
            visionRenderer.material.color = c;
    }

    // ---------- DEBUG ----------
    void OnDrawGizmosSelected()
    {
        if (rotator == null) return;

        Gizmos.color = Color.green;

        Vector3 origin = rotator.position + Vector3.up * 0.5f;

        Vector3 left = Quaternion.Euler(0, -viewAngle / 2f, 0) * rotator.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2f, 0) * rotator.forward;

        Gizmos.DrawRay(origin, left * viewDistance);
        Gizmos.DrawRay(origin, right * viewDistance);
    }
}