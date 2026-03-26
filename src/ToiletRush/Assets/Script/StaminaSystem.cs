using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

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

    // ================= AUDIO =================
    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioSource heartbeatSource;

    [Header("Phase Sounds")]
    public AudioClip shockPhase2Clip;
    public AudioClip shockPhase3Clip;
    public AudioClip heartbeatPhase2;
    public AudioClip heartbeatPhase3;

    //  MUSIC SYSTEM
    [Header("Music")]
    public AudioSource musicSource;
    public AudioClip phase1Music;
    public AudioClip phase2Music;
    public AudioClip phase3Music;

    [Range(0f, 1f)] public float phase1Volume = 0.2f;
    [Range(0f, 1f)] public float phase2Volume = 0.5f;
    [Range(0f, 1f)] public float phase3Volume = 1f;

    public float musicFadeSpeed = 1.5f;
    private float targetMusicVolume = 0f;

    // ================= UI SYSTEM =================
    [Header("Main UI Image")]
    public Image reactionImage;
    private Sprite defaultSprite;

    [Header("QTE UI Override")]
    public Sprite qteSprite;
    private bool isQTEActive = false;

    [System.Serializable]
    public class ReactionMapping
    {
        public string tag;
        public Sprite sprite;
    }

    [Header("Hit Reaction UI")]
    public float reactionTime = 0.15f;
    public ReactionMapping[] reactionMappings;

    private Coroutine reactionRoutine;
    private bool isShowingReaction = false;
    private bool allowMusic = false;
    private int currentPhase = 0;

    [Header("Sweat Particles")]
    public ParticleSystem sweatParticleInstance; // ใช้เป็น instance ติดตัวผู้เล่น
    public Transform sweatSpawnPoint;             // จุด spawn บนตัวละคร (เช่นไหล่/หัว)
    public float sweatIntervalPhase2 = 1.5f;      // วินาทีระหว่าง spawn ในเฟส 2
    public float sweatIntervalPhase3 = 0.5f;      // วินาทีระหว่าง spawn ในเฟส 3
    private float sweatTimer = 0f;

    void Start()
    {
        if (staminaSlider != null)
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

        if (reactionImage != null)
            defaultSprite = reactionImage.sprite;

        // ตรวจสอบ Particle System instance
        if (sweatParticleInstance != null && sweatSpawnPoint != null)
        {
            // ให้เป็น child ของตัวผู้เล่น
            sweatParticleInstance.transform.SetParent(sweatSpawnPoint);
            sweatParticleInstance.transform.localPosition = Vector3.zero;
            sweatParticleInstance.Stop();
            var main = sweatParticleInstance.main;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
        }

        //  เริ่มเพลง
    }

    void Update()
    {
        DrainStamina();
        UpdateUI();

        float percent = currentStamina / maxStamina;

        //  ไม่ให้ QTE ไปทับ UI stamina
        if (!isShowingReaction && !isQTEActive && staminaVisualUI != null)
        {
            staminaVisualUI.UpdateStaminaVisual(percent);
        }

        if (uiShake != null)
        {
            bool low = percent <= 0.25f;
            uiShake.SetLowStamina(low);
        }

        HandlePhaseEffects(percent);
        UpdatePostProcessing(percent);

        //  Smooth music fade
        if (allowMusic && musicSource != null)
        {
            musicSource.volume = Mathf.Lerp(
                musicSource.volume,
                targetMusicVolume,
                Time.deltaTime * musicFadeSpeed
            );
        }

        if (currentStamina <= 0)
            GameOver();

        // ======= Control Sweat Particle by Phase =======
        if (sweatParticleInstance != null)
        {
            if (currentPhase == 2)
            {
                sweatTimer += Time.deltaTime;
                if (sweatTimer >= sweatIntervalPhase2)
                {
                    sweatTimer = 0f;
                    PlaySweat();
                }
            }
            else if (currentPhase == 3)
            {
                sweatTimer += Time.deltaTime;
                if (sweatTimer >= sweatIntervalPhase3)
                {
                    sweatTimer = 0f;
                    PlaySweat();
                }
            }
            else
            {
                sweatTimer = 0f;
                StopSweat();
            }
        }
    }

    // ================= MUSIC =================
    void SetMusic(AudioClip clip, float volume)
    {
        if (!allowMusic) return;

        if (musicSource == null || clip == null) return;

        if (musicSource.clip != clip)
        {
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }

        targetMusicVolume = volume;
    }
    public void EnableMusic()
    {
        allowMusic = true;
        SetMusic(phase1Music, phase1Volume);
    }
    // ================= AUDIO =================
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

    // ================= PHASE =================
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

            if (currentPhase == 2)
            {
                staminaVisualUI?.PlayShock(1f);
                staminaVisualUI?.StartLightShake(0.5f);

                PlayShock(shockPhase2Clip);
                PlayHeartbeat(heartbeatPhase2, 0.4f, 1f);

                SetMusic(phase2Music, phase2Volume);
            }
            else if (currentPhase == 3)
            {
                staminaVisualUI?.PlayShock(1.5f);
                staminaVisualUI?.StartHeavyShake();

                PlayShock(shockPhase3Clip);
                PlayHeartbeat(heartbeatPhase3, 0.8f, 1.2f);

                SetMusic(phase3Music, phase3Volume);
            }
            else
            {
                staminaVisualUI?.StopAllEffects();
                StopHeartbeat();

                SetMusic(phase1Music, phase1Volume);
            }
        }
    }

    // ================= QTE =================
    public void StartQTEUI()
    {
        if (reactionImage == null || qteSprite == null) return;

        isQTEActive = true;
        isShowingReaction = true;

        reactionImage.sprite = qteSprite;
    }

    public void StopQTEUI()
    {
        isQTEActive = false;
        isShowingReaction = false;

        if (reactionImage != null && defaultSprite != null)
            reactionImage.sprite = defaultSprite;
    }

    public void PlayQTEShake()
    {
        uiShake?.PlayHitShake();
    }

    // ================= HIT REACTION =================
    private void OnTriggerEnter(Collider other)
    {
        if (isQTEActive) return;

        foreach (var map in reactionMappings)
        {
            if (map != null && other.CompareTag(map.tag) && map.sprite != null)
            {
                ShowHitReaction(map.sprite);
                break;
            }
        }
    }

    void ShowHitReaction(Sprite sprite)
    {
        if (reactionImage == null) return;

        isShowingReaction = true;
        reactionImage.sprite = sprite;

        if (reactionRoutine != null)
            StopCoroutine(reactionRoutine);

        reactionRoutine = StartCoroutine(ReactionRoutine());
    }

    IEnumerator ReactionRoutine()
    {
        yield return new WaitForSeconds(reactionTime);

        if (!isQTEActive)
        {
            isShowingReaction = false;

            if (reactionImage != null && defaultSprite != null)
                reactionImage.sprite = defaultSprite;
        }
    }

    // ================= POST PROCESS =================
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

    // ================= STAMINA =================
    public void ReduceStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        UpdateUI();

        uiShake?.PlayHitShake();
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

    // ================= GAME OVER =================
    void GameOver()
    {
        Debug.Log("GAME OVER : Stamina หมด");

        StopHeartbeat();

        if (sfxSource != null)
            sfxSource.Stop();

        if (musicSource != null)
            musicSource.Stop();

        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(true);

        
        Time.timeScale = 0f;
        enabled = false;
    }

    // ================= SWEAT PARTICLE =================
    void PlaySweat()
    {
        if (sweatParticleInstance == null) return;

        if (!sweatParticleInstance.isPlaying)
        {
            sweatParticleInstance.Play();
        }
    }

    void StopSweat()
    {
        if (sweatParticleInstance == null) return;

        if (sweatParticleInstance.isPlaying)
        {
            sweatParticleInstance.Stop();
        }
    }
}