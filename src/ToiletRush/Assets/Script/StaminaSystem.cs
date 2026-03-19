using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class StaminaSystem : MonoBehaviour
{
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina;
    private StaminaUIShake uiShake;

    [Header("Drain Rates")]
    public float baseDrainPerSecond = 1f;
    public float runExtraDrainPerSecond = 2f;

    [Header("UI")]
    public Slider staminaSlider;
    public StaminaVisualUI staminaVisualUI;

    [Header("Game Over")]
    public GameObject gameOverCanvas;
    public Image gameOverImage;
    public Sprite gameOverSprite;

    [Header("Post Processing")]
    public Volume volume;
    private Vignette vignette;
    private ChromaticAberration chromatic;

    [Header("Phase Threshold")]
    public float phase2Threshold = 0.4f;
    public float phase3Threshold = 0.2f;

    [Header("Pulse")]
    public float pulseSpeed = 1.5f;
    public float pulseAmount = 0.1f;
    private float pulseTimer;
    [Header("Audio")]
    public AudioSource sfxSource;        // เสียงกระชาก (one shot)
    public AudioSource heartbeatSource;  // เสียงหัวใจ (loop)

    [Header("Phase Sounds")]
    public AudioClip shockPhase2Clip;
    public AudioClip shockPhase3Clip;

    public AudioClip heartbeatPhase2;
    public AudioClip heartbeatPhase3;
    private int currentPhase = 0;

    void Start()
    {
        uiShake = staminaSlider.GetComponentInParent<StaminaUIShake>();
        currentStamina = maxStamina;

        if (volume != null)
        {
            volume.profile.TryGet(out vignette);
            volume.profile.TryGet(out chromatic);
        }

        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
        }
    }
    void PlayShock(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip);
    }

    void PlayHeartbeat(AudioClip clip, float volume, float pitch)
    {
        if (heartbeatSource == null || clip == null) return;

        heartbeatSource.clip = clip;
        heartbeatSource.loop = true;
        heartbeatSource.volume = volume;
        heartbeatSource.pitch = pitch;

        heartbeatSource.Play();
    }

    void StopHeartbeat()
    {
        if (heartbeatSource != null)
            heartbeatSource.Stop();
    }
    void Update()
    {
        DrainStamina();
        UpdateUI();

        float percent = currentStamina / maxStamina;

        staminaVisualUI.UpdateStaminaVisual(percent);

        if (uiShake != null)
        {
            bool low = percent <= 0.25f;
            uiShake.SetLowStamina(low);
        }

        HandlePhaseEffects(percent);
        UpdatePostProcessing(percent);

        if (currentStamina <= 0)
            GameOver();
    }

    //  Phase Control
    void HandlePhaseEffects(float percent)
    {
        int newPhase = 0;

        if (percent <= phase3Threshold)
            newPhase = 3;
        else if (percent <= phase2Threshold)
            newPhase = 2;

        if (newPhase != currentPhase)
        {
            currentPhase = newPhase;

            //  Phase 2
            if (currentPhase == 2)
            {
                staminaVisualUI.PlayShock(1f);

                //  เสียงกระชาก sync กับภาพ
                PlayShock(shockPhase2Clip);

                staminaVisualUI.StartLightShake(0.5f);

                // heartbeat เบา
                PlayHeartbeat(heartbeatPhase2, 0.4f, 1f);
            }

            //  Phase 3
            else if (currentPhase == 3)
            {
                staminaVisualUI.PlayShock(1.5f);

                //  เสียงกระชากแรง
                PlayShock(shockPhase3Clip);

                staminaVisualUI.StartHeavyShake();

                //  heartbeat หนักขึ้น
                PlayHeartbeat(heartbeatPhase3, 0.8f, 1.2f);
            }

            // กลับปกติ
            else
            {
                staminaVisualUI.StopAllEffects();
                StopHeartbeat();
            }
        }
    }

    //  Post Processing
    void UpdatePostProcessing(float percent)
    {
        if (vignette == null || chromatic == null) return;

        float targetVignette = 0f;
        float targetChromatic = 0f;

        if (currentPhase == 2)
        {
            targetVignette = 0.35f;
            targetChromatic = 0.2f;
        }
        else if (currentPhase == 3)
        {
            targetVignette = 0.6f;
            targetChromatic = 0.5f;

            pulseTimer += Time.deltaTime * pulseSpeed;
            float pulse = Mathf.Sin(pulseTimer) * pulseAmount;

            targetVignette += pulse;
        }

        vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, targetVignette, Time.deltaTime * 3f);
        chromatic.intensity.value = Mathf.Lerp(chromatic.intensity.value, targetChromatic, Time.deltaTime * 3f);
    }

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
            drain += runExtraDrainPerSecond;

        currentStamina -= drain * Time.deltaTime;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }

    void UpdateUI()
    {
        if (staminaSlider != null)
            staminaSlider.value = currentStamina;
    }

    void GameOver()
    {
        Debug.Log("GAME OVER : Stamina หมด");

        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(true);

        if (gameOverImage != null && gameOverSprite != null)
            gameOverImage.sprite = gameOverSprite;

        Time.timeScale = 0f;
        enabled = false;
    }
}