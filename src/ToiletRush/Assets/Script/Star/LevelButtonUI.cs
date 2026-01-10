using UnityEngine;
using UnityEngine.UI;

public class LevelButtonUI : MonoBehaviour
{
    public string levelName;
    public Button playButton;
    public StarUI starUI;

    void Start()
    {
        bool unlocked = SaveManager.IsLevelUnlocked(levelName);
        playButton.interactable = unlocked;

        int stars = SaveManager.GetStars(levelName);
        starUI.ShowStars(stars);
    }
}
