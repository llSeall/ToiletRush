using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    public float shakeDuration = 0.2f;
    public float shakeStrength = 0.15f;
    public float shakeDamping = 15f;

    private Vector3 originalLocalPos;
    private float shakeTimer;

    // ===== QTE =====
    bool qteShakeActive;
    public float qteShakeStrength = 0.12f;

    void Start()
    {
        originalLocalPos = transform.localPosition;
    }

    void Update()
    {
        if (qteShakeActive)
        {
            Vector3 offset = Random.insideUnitSphere * qteShakeStrength;
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                originalLocalPos + offset,
                Time.unscaledDeltaTime * shakeDamping
            );
            return;
        }

        if (shakeTimer > 0)
        {
            Vector3 offset = Random.insideUnitSphere * shakeStrength;
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                originalLocalPos + offset,
                Time.deltaTime * shakeDamping
            );

            shakeTimer -= Time.deltaTime;
        }
        else
        {
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                originalLocalPos,
                Time.deltaTime * shakeDamping
            );
        }
    }

    // ===== เดิม (impulse) =====
    public void Shake(float strengthMultiplier = 1f)
    {
        shakeTimer = shakeDuration;
        shakeStrength *= strengthMultiplier;
        Invoke(nameof(ResetStrength), shakeDuration);
    }

    void ResetStrength()
    {
        shakeStrength = Mathf.Abs(shakeStrength);
    }

    // ===== ใหม่ สำหรับ QTE =====
    public void StartQTEShake()
    {
        qteShakeActive = true;
    }

    public void StopQTEShake()
    {
        qteShakeActive = false;
        transform.localPosition = originalLocalPos;
    }
}
