using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    [Header("Cutscene")]
    public GameObject cutsceneCanvas;   // Canvas ของคัตซีน
    public GameObject mainMenuUI;        // UI เมนูหลัก
    public Image comicImage;
    public Sprite[] cutsceneSprites;

    [Header("Scene")]
    public string level1SceneName = "Level1";

    private int currentIndex = 0;

    void Start()
    {
        cutsceneCanvas.SetActive(false);
        mainMenuUI.SetActive(true);
    }

    // ===============================
    // START GAME (กดปุ่ม Start)
    // ===============================
    public void StartGame()
    {
        mainMenuUI.SetActive(false);
        cutsceneCanvas.SetActive(true);

        currentIndex = 0;

        if (cutsceneSprites.Length > 0)
            comicImage.sprite = cutsceneSprites[currentIndex];
    }

    void Update()
    {
        if (!cutsceneCanvas.activeSelf) return;

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
            LoadLevel1();
        }
        else
        {
            comicImage.sprite = cutsceneSprites[currentIndex];
        }
    }

    void LoadLevel1()
    {
        SceneManager.LoadScene(level1SceneName);
    }

    // ===============================
    // EXIT GAME (ปุ่ม Exit)
    // ===============================
    public void ExitGame()
    {
        Debug.Log("EXIT GAME");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
