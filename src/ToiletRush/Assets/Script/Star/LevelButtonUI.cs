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

        //  แปลง int  bool[]
        bool[] starResults = ConvertStarsToBoolArray(stars);

        starUI.ShowStars(starResults);
    }

    bool[] ConvertStarsToBoolArray(int starCount)
    {
        bool[] results = new bool[3];

        for (int i = 0; i < results.Length; i++)
        {
            results[i] = i < starCount;
        }

        return results;
    }
}