using UnityEngine;

public class LevelResultManager : MonoBehaviour
{
    public static LevelResultManager Instance;

    [Header("Stamina Star")]
    public float staminaStarThreshold = 60f;

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

    public void CollectSecret()
    {
        collectedSecret = true;
    }

    //  ระบบใหม่: คืนค่าเป็น bool[]
    public bool[] GetStarResults()
    {
        bool[] results = new bool[3];

        //  ดวงที่ 1: ชนะด่าน (ได้เสมอ)
        results[0] = true;

        //  ดวงที่ 2: เก็บของลับ
        results[2] = collectedSecret;

        // ดวงที่ 3: Stamina มากพอ
        if (staminaSystem != null)
        {
            results[1] = staminaSystem.currentStamina >= staminaStarThreshold;
        }
        else
        {
            results[1] = false;
        }

        Debug.Log(
            $"Star1={results[0]} | Star2={results[1]} | Star3={results[2]} | Stamina={staminaSystem?.currentStamina}"
        );

        return results;
    }
}