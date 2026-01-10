using UnityEngine;
using UnityEngine.UI;

public class TalkQTEUI : MonoBehaviour
{
    [Header("UI")]
    public Image progressCircle;   // วงใน (ขยาย)
    public Image targetCircle;     // วงนอก (คงที่)

    [Header("QTE")]
    [Range(0, 1)] public float progress;
    public float increasePerPress = 0.1f;
    public float decayPerSecond = 0.3f;
    public CameraShake cameraShake;

    [Header("Progress Scale")]
    public float startScale = 0.15f; // เล็กกว่าทาเก็ต
    public float endScale = 1f;      // = ทาเก็ตพอดี

    [Header("Shake When Near Success")]
    public float shakeThreshold = 0.85f;
    public float shakeStrength = 6f;
    public float shakeSpeed = 30f;

    private NPCPatrolTalkAI npc;
    private bool active;
    private Vector3 originalPos;

    void Awake()
    {
        originalPos = transform.localPosition;
    }

    void Update()
    {
        if (!active) return;

        // ---------- INPUT ----------
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            progress += increasePerPress;
        }

        // ---------- DECAY ----------
        progress -= decayPerSecond * Time.deltaTime;
        progress = Mathf.Clamp01(progress);

        UpdateVisual();

        // ---------- SUCCESS ----------
        if (progress >= 1f)
        {
            Success();
        }
    }

    void UpdateVisual()
    {
        // Fill
        progressCircle.fillAmount = progress;

        // Scale (ขยายเข้าหา Target)
        float scale = Mathf.Lerp(startScale, endScale, progress);
        progressCircle.transform.localScale = Vector3.one * scale;

        // Shake เมื่อใกล้สำเร็จ
        if (progress >= shakeThreshold)
        {
            float shake = Mathf.Sin(Time.time * shakeSpeed) * shakeStrength;
            transform.localPosition = originalPos + new Vector3(shake, 0, 0);
        }
        else
        {
            transform.localPosition = originalPos;
        }
    }
    public void StartQTE(NPCPatrolTalkAI owner)
    {
        npc = owner;
        progress = 0f;
        active = true;

        progressCircle.fillAmount = 0f;
        progressCircle.transform.localScale = Vector3.one * startScale;

        transform.localPosition = originalPos;
        gameObject.SetActive(true);

        if (cameraShake != null)
            cameraShake.StartQTEShake();

        Time.timeScale = 0.8f;
    }
    void Success()
    {
        active = false;
        gameObject.SetActive(false);
        Time.timeScale = 1f;

        if (cameraShake != null)
            cameraShake.StopQTEShake();

        npc.OnQTESuccess();
    }

}
