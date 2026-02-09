using UnityEngine;
using UnityEngine.UI;

public class StaminaSystem : MonoBehaviour
{
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina;
    private StaminaUIShake uiShake;

    [Header("Drain Rates")]
    public float baseDrainPerSecond = 1f;     // ลดตลอดเวลา
    public float runExtraDrainPerSecond = 2f; // ลดเพิ่มตอนวิ่ง

    [Header("UI")]
    public Slider staminaSlider;
    public StaminaVisualUI staminaVisualUI;
    [Header("Game Over")]
    public GameObject gameOverCanvas;

    void Start()
    {
        uiShake = staminaSlider.GetComponentInParent<StaminaUIShake>();
        currentStamina = maxStamina;

        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
        }
    }
    void Update()
    {
        DrainStamina();
        UpdateUI();
        float percent = currentStamina / maxStamina;

        staminaVisualUI.UpdateStaminaVisual(percent);
        if (uiShake != null)
        {
            bool low = currentStamina / maxStamina <= 0.2f;
            uiShake.SetLowStamina(low);
            uiShake.SetLowStamina(currentStamina <= maxStamina * 0.25f);

        }

        if (currentStamina <= 0)
            GameOver();
    }


    //  ลดจาก Trigger / Event ภายนอก
    public void ReduceStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        UpdateUI();

        if (uiShake != null)
            uiShake.PlayHitShake();
    }


    void DrainStamina()
    {
        float drain = baseDrainPerSecond;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            drain += runExtraDrainPerSecond;
        }

        currentStamina -= drain * Time.deltaTime;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }

    void UpdateUI()
    {
        if (staminaSlider != null)
        {
            staminaSlider.value = currentStamina;
        }
    }

    void GameOver()
    {
        Debug.Log("GAME OVER : Stamina หมด");

        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(true);

        Time.timeScale = 0f;
        enabled = false;
    }
}
