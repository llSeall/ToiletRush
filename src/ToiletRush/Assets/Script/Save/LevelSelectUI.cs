using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

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
        public Image[] stars;
        public Sprite starOn;
        public Sprite starOff;
    }

    public LevelButton[] levels;

    [Header("Fade")]
    public Image fadeImage;
    public float fadeDuration = 1f;

    private bool isLoading = false;

    void Start()
    {
        //  ห้ามเปิด fadeImage ตรงนี้
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(false);
        }

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
                string scene = level.sceneName;

                level.button.onClick.AddListener(() =>
                {
                    if (!isLoading)
                        StartCoroutine(FadeAndLoad(scene));
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
    IEnumerator FadeAndLoad(string sceneName)
    {
        isLoading = true;

        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(0, 0, 0, 0);

        float t = 0f;
        Color c = fadeImage.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0, 1, t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }

        SceneManager.LoadScene(sceneName);
    }


    public void BackToMenu()
    {
        gameObject.SetActive(false);
    }
}
