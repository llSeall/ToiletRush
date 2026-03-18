using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;


public class PauseManager : MonoBehaviour
{
    [Header("Main Pause Panel")]
    public GameObject pausePanel;

    [Header("Post Processing")]
    public Volume volume;
    private DepthOfField dof;

    [Header("ESC Custom Image Panel")]
    public GameObject escImagePanel;
    public Image escImage;
    public Sprite levelEscSprite;

    [Header("Player Control Scripts")]
    public MonoBehaviour[] playerControlScripts;

    private bool isPaused;

    void Start()
    {
        pausePanel.SetActive(false);

        if (escImagePanel != null)
            escImagePanel.SetActive(false);

        // ตั้งค่ารูป
        if (escImage != null && levelEscSprite != null)
            escImage.sprite = levelEscSprite;

        // ดึง Depth of Field จาก Volume
        if (volume != null && volume.profile.TryGet(out dof))
        {
            dof.active = false;

            // ตั้งค่าเริ่มต้น (ไม่เบลอ)
            dof.aperture.value = 0f;
        }
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
        StartCoroutine(BlurTransition(20f));
        if (escImagePanel != null)
            escImagePanel.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;

        EnableBlur(true);
        DisablePlayerControl();
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        StartCoroutine(BlurTransition(0f));
        if (escImagePanel != null)
            escImagePanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;

        EnableBlur(false);
        EnablePlayerControl();
    }

    void EnableBlur(bool state)
    {
        if (dof == null) return;

        dof.active = state;

        if (state)
        {
            // ตอน Pause  เบลอ
            dof.focusDistance.value = 0.1f;
            dof.aperture.value = 20f;
            dof.focalLength.value = 50f;
        }
        else
        {
            // ตอน Resume  หายเบลอ
            dof.aperture.value = 0f;
        }
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
    IEnumerator BlurTransition(float targetAperture)
    {
        float duration = 0.3f;
        float time = 0f;

        float start = dof.aperture.value;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            dof.aperture.value = Mathf.Lerp(start, targetAperture, time / duration);
            yield return null;
        }
    }
}