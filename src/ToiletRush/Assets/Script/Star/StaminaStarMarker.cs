using UnityEngine;
using UnityEngine.UI;

public class StaminaStarMarker : MonoBehaviour
{
    [Header("Refs")]
    public Slider staminaSlider;
    public RectTransform fillArea;

    RectTransform markerRect;

    void Awake()
    {
        markerRect = GetComponent<RectTransform>();
    }

    void Update()
    {
        UpdateMarker();
    }

    void UpdateMarker()
    {
        if (staminaSlider == null || fillArea == null) return;
        if (LevelResultManager.Instance == null) return;

        float threshold =
            LevelResultManager.Instance.staminaStarThreshold;

        float normalized = threshold / staminaSlider.maxValue;
        float width = fillArea.rect.width;

        float xPos = (normalized * width) - (width * 0.5f);

        markerRect.anchoredPosition =
            new Vector2(xPos, markerRect.anchoredPosition.y);
    }
}
