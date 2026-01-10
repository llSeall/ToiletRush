using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectUI : MonoBehaviour
{
    [System.Serializable]
    public class LevelButton
    {
        public int levelIndex;
        public string sceneName;
        public Button button;
        public GameObject lockIcon;
        [Header("Star UI")]
        public Image[] stars;        // 3 รูป
        public Sprite starOn;
        public Sprite starOff;

    }

    public LevelButton[] levels;

    void Start()
    {
        foreach (var level in levels)
        {
            bool unlocked = SaveManager.IsLevelUnlocked(level.levelIndex);

            level.button.interactable = unlocked;

            if (level.lockIcon != null)
                level.lockIcon.SetActive(!unlocked);

            if (unlocked)
            {
                int starCount = SaveManager.GetStars(level.sceneName);
                UpdateStars(level, starCount);

                level.button.onClick.RemoveAllListeners();
                level.button.onClick.AddListener(() =>
                {
                    SceneManager.LoadScene(level.sceneName);
                });
            }
            else
            {
                UpdateStars(level, 0);
            }
        }
    }
    void UpdateStars(LevelButton level, int count)
    {
        if (level.stars == null || level.stars.Length == 0) return;

        for (int i = 0; i < level.stars.Length; i++)
        {
            level.stars[i].sprite =
                i < count ? level.starOn : level.starOff;
        }
    }


    public void BackToMenu()
    {
        gameObject.SetActive(false);
    }
}
