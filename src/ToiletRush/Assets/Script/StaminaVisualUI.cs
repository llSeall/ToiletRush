using UnityEngine;
using UnityEngine.UI;

public class StaminaVisualUI : MonoBehaviour
{
    [Header("Main Stamina UI")]
    [Tooltip("Root ของหลอด stamina (เอาไว้ซ่อน)")]
    public GameObject staminaBarRoot;

    [Tooltip("ซ่อนหลอด stamina ตอนเริ่มเกม")]
    public bool hideStaminaBarOnStart = true;

    [Header("Face / Visual UI")]
    public Image faceImage;

    [System.Serializable]
    public class StaminaFaceState
    {
        [Range(0f, 1f)]
        public float minPercent;   // ต่ำสุดที่ภาพนี้จะแสดง
        public Sprite faceSprite;
    }

    [Header("Face States (เรียงจากมาก -> น้อย)")]
    public StaminaFaceState[] faceStates;

    void Start()
    {
        if (staminaBarRoot != null)
            staminaBarRoot.SetActive(!hideStaminaBarOnStart);
    }

    // ====== API ให้ StaminaSystem เรียก ======
    // percent = 0–1
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

    // ใช้กรณีอยากโชว์หลอดกลับมา
    public void SetStaminaBarVisible(bool visible)
    {
        if (staminaBarRoot != null)
            staminaBarRoot.SetActive(visible);
    }
}
