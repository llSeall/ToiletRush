using UnityEngine;

public class LevelResultManager : MonoBehaviour
{
    public static LevelResultManager Instance;

    [Header("Stamina Star")]
    public float staminaStarThreshold = 60f; // ต้องเหลือเท่านี้ถึงได้ดาว

    [Header("Result")]
    public bool collectedSecret = false;

    private StaminaSystem staminaSystem;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        staminaSystem = FindObjectOfType<StaminaSystem>();
    }

    // เรียกเมื่อเก็บของลับ
    public void CollectSecret()
    {
        collectedSecret = true;
    }

    // เรียกตอนชนะด่าน
    public int CalculateStars()
    {
        int stars = 1; // จบด่าน = ได้ 1 ดาว

        if (collectedSecret)
            stars++;

        if (staminaSystem != null &&
            staminaSystem.currentStamina >= staminaStarThreshold)
        {
            stars++;
        }

        Debug.Log(
            $"Stars={stars} | Stamina={staminaSystem.currentStamina}/{staminaStarThreshold}"
        );

        return stars;
    }
}
