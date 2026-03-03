using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("Main Pause Panel")]
    public GameObject pausePanel;

    [Header("ESC Custom Image Panel")]
    public GameObject escImagePanel;
    public Image escImage;              //  ใส่ Image component
    public Sprite levelEscSprite;       //  รูปของด่านนี้

    [Header("Player Control Scripts")]
    public MonoBehaviour[] playerControlScripts;

    private bool isPaused;

    void Start()
    {
        pausePanel.SetActive(false);

        if (escImagePanel != null)
            escImagePanel.SetActive(false);

        //  ตั้งค่ารูปตามด่าน
        if (escImage != null && levelEscSprite != null)
            escImage.sprite = levelEscSprite;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);

        if (escImagePanel != null)
            escImagePanel.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;

        DisablePlayerControl();
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);

        if (escImagePanel != null)
            escImagePanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;

        EnablePlayerControl();
    }

    void DisablePlayerControl()
    {
        foreach (var script in playerControlScripts)
            if (script != null)
                script.enabled = false;
    }

    void EnablePlayerControl()
    {
        foreach (var script in playerControlScripts)
            if (script != null)
                script.enabled = true;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}