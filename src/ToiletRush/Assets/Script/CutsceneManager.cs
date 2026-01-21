using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CutsceneManager : MonoBehaviour
{
    [Header("Cutscene")]
    public GameObject cutsceneCanvas;
    public GameObject mainMenuUI;
    public Image comicImage;
    public Sprite[] cutsceneSprites;

    [Header("Fade")]
    public Image fadeImage;
    public float fadeDuration = 1.2f;

    [Header("Scene")]
    public string level1SceneName = "Level1";

    private int currentIndex = 0;
    private bool isFading = false;

    void Start()
    {
        cutsceneCanvas.SetActive(false);
        mainMenuUI.SetActive(true);

        //  ปิด Fade ไว้ก่อน
        if (fadeImage != null)
            fadeImage.gameObject.SetActive(false);
    }

    // ===============================
    // START GAME
    // ===============================
    public void StartGame()
    {
        mainMenuUI.SetActive(false);
        cutsceneCanvas.SetActive(true);

        currentIndex = 0;
        comicImage.sprite = cutsceneSprites[currentIndex];

        //  เปิด Fade แล้วตั้งค่าเริ่มต้น
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(0, 0, 0, 1);

        StartCoroutine(FadeIn());
    }

    void Update()
    {
        if (!cutsceneCanvas.activeSelf) return;
        if (isFading) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            NextImage();
        }
    }

    void NextImage()
    {
        currentIndex++;

        if (currentIndex >= cutsceneSprites.Length)
        {
            StartCoroutine(FadeOutAndLoad());
        }
        else
        {
            comicImage.sprite = cutsceneSprites[currentIndex];
        }
    }

    // ===============================
    // FADE IN
    // ===============================
    IEnumerator FadeIn()
    {
        isFading = true;

        float t = 0;
        Color c = fadeImage.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1, 0, t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }

        c.a = 0;
        fadeImage.color = c;
        isFading = false;
    }

    // ===============================
    // FADE OUT + LOAD
    // ===============================
    IEnumerator FadeOutAndLoad()
    {
        isFading = true;

        float t = 0;
        Color c = fadeImage.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0, 1, t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }

        SceneManager.LoadScene(level1SceneName);
    }

    // ===============================
    // EXIT GAME
    // ===============================
    public void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
