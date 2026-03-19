using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StaminaVisualUI : MonoBehaviour
{
    [Header("Main Stamina UI")]
    public GameObject staminaBarRoot;
    public bool hideStaminaBarOnStart = true;

    [Header("Face / Visual UI")]
    public Image faceImage;

    [System.Serializable]
    public class StaminaFaceState
    {
        [Range(0f, 1f)]
        public float minPercent;
        public Sprite faceSprite;
    }

    [Header("Face States (เรียงจากมาก -> น้อย)")]
    public StaminaFaceState[] faceStates;

    //  EFFECT
    private RectTransform rect;
    private Vector3 originalScale;
    private Vector3 originalPos;

    private float shakeAmount = 0f;
    private float shakeSpeed = 0f;
    private bool heavyShake = false;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        originalScale = rect.localScale;
        originalPos = rect.localPosition;

        if (staminaBarRoot != null)
            staminaBarRoot.SetActive(!hideStaminaBarOnStart);
    }

    void Update()
    {
        //  สั่น
        if (shakeAmount > 0f)
        {
            float x = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
            float y = Mathf.Cos(Time.time * shakeSpeed * 0.8f) * shakeAmount;

            if (heavyShake)
            {
                x *= 2f;
                y *= 2f;
            }

            rect.localPosition = originalPos + new Vector3(x, y, 0);
        }
    }

    // ====== API ======
    public void UpdateStaminaVisual(float percent)
    {
        if (faceImage == null || faceStates == null) return;

        for (int i = 0; i < faceStates.Length; i++)
        {
            if (percent >= faceStates[i].minPercent)
            {
                faceImage.sprite = faceStates[i].faceSprite;
                return;
            }
        }
    }

    public void SetStaminaBarVisible(bool visible)
    {
        if (staminaBarRoot != null)
            staminaBarRoot.SetActive(visible);
    }

    //  สะดุ้ง
    public void PlayShock(float strength)
    {
        StopAllCoroutines();
        StartCoroutine(ShockRoutine(strength));
    }

    IEnumerator ShockRoutine(float strength)
    {
        rect.localScale = originalScale * (1f + 0.3f * strength);
        yield return new WaitForSeconds(0.08f);
        rect.localScale = originalScale;
    }

    //  Phase 2
    public void StartLightShake(float amount)
    {
        shakeAmount = amount;
        shakeSpeed = 6f;
        heavyShake = false;
    }

    //  Phase 3
    public void StartHeavyShake()
    {
        shakeAmount = 1.5f;
        shakeSpeed = 10f;
        heavyShake = true;
    }

    public void StopAllEffects()
    {
        shakeAmount = 0f;
        rect.localPosition = originalPos;
        rect.localScale = originalScale;
    }
}