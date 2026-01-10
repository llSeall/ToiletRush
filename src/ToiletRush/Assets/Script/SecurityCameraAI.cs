using UnityEngine;

public class SecurityCameraAI : MonoBehaviour
{
    [Header("Rotation")]
    public float rotateAngle = 45f;
    public float rotateSpeed = 30f;
    public Transform rotator;
    private Quaternion baseRotation;

    [Header("Disable")]
    public GameObject visionObject; // วัตถุแสง/กรวยที่แสดงระยะมองเห็น

    [Header("Vision")]
    public float viewDistance = 8f;
    [Range(0, 180)] public float viewAngle = 60f;
    public LayerMask obstacleMask;

    [Header("Alert")]
    public float alertCooldown = 5f;
    public Renderer visionRenderer;
    public Color normalColor = Color.green;
    public Color alertColor = Color.red;

    private Transform player;
    private float currentAngle;
    private int direction = 1;

    private bool canAlert = true;
    private bool isDisabled = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentAngle = -rotateAngle;

        //  เก็บ rotation ตั้งต้น
        if (rotator != null)
            baseRotation = rotator.localRotation;

        UpdateVisionColor(normalColor);
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
        if (!canAlert) return;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > viewDistance) return;

        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > viewAngle / 2f) return;

        if (!Physics.Raycast(transform.position + Vector3.up,
            dirToPlayer, distance, obstacleMask))
        {
            AlertNearestGuard();
        }
    }

    // ---------- ALERT ----------
    void AlertNearestGuard()
    {
        canAlert = false;
        UpdateVisionColor(alertColor);

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

    void UpdateVisionColor(Color c)
    {
        if (visionRenderer != null)
            visionRenderer.material.color = c;
    }

    // ---------- DEBUG (FOV) ----------
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Vector3 left = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward;

        Gizmos.DrawRay(transform.position, left * viewDistance);
        Gizmos.DrawRay(transform.position, right * viewDistance);

        int segments = 20;
        Vector3 prevPoint = transform.position + left * viewDistance;

        for (int i = 1; i <= segments; i++)
        {
            float angle = -viewAngle / 2f + (viewAngle / segments) * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * transform.forward;
            Vector3 nextPoint = transform.position + dir * viewDistance;

            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }
    public void DisableCameraFully()
    {
        isDisabled = true;
        canAlert = false;

        // หยุดหมุน
        enabled = false;

        // ปิดแสงแสดงระยะมองเห็น
        if (visionObject != null)
            visionObject.SetActive(false);

        // เปลี่ยนสี (ถ้ายังใช้ Renderer)
        UpdateVisionColor(Color.gray);
    }

}
