using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pausePanel;
    public MonoBehaviour[] playerControlScripts;

    private bool isPaused;

    void Start()
    {
        pausePanel.SetActive(false);
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

        Time.timeScale = 0f;
        isPaused = true;

        DisablePlayerControl();
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);

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

    // ปุ่มเทส (ไม่ใช้ ESC)
    public void TestPauseButton()
    {
        TogglePause();
    }
}
