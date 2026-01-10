using UnityEngine;

public class StaminaUIShake : MonoBehaviour
{
    [Header("Hit Shake")]
    public float hitShakeDuration = 0.25f;
    public float hitShakeStrength = 12f;

    [Header("Low Stamina Shake")]
    public float lowShakeStrength = 6f;
    public float lowShakeSpeed = 25f;

    private Vector3 originalPos;
    private float hitTimer;
    private bool lowStamina;

    void Awake()
    {
        originalPos = transform.localPosition;
    }

    void Update()
    {
        //  โดนชน / หลอดลดแรง
        if (hitTimer > 0)
        {
            transform.localPosition =
                originalPos + (Vector3)Random.insideUnitCircle * hitShakeStrength;

            hitTimer -= Time.unscaledDeltaTime;
            return;
        }

        //  ใกล้หมด = สั่นถี่
        if (lowStamina)
        {
            float x = Mathf.Sin(Time.unscaledTime * lowShakeSpeed) * lowShakeStrength;
            transform.localPosition = originalPos + new Vector3(x, 0, 0);
            return;
        }

        // กลับตำแหน่งเดิม
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            originalPos,
            Time.unscaledDeltaTime * 20f
        );
    }

    // ====== API ที่ StaminaSystem เรียก ======

    public void PlayHitShake()
    {
        hitTimer = hitShakeDuration;
    }

    public void SetLowStamina(bool value)
    {
        lowStamina = value;
    }

    // ปุ่มเทส
    public void TestShake()
    {
        PlayHitShake();
    }
}
