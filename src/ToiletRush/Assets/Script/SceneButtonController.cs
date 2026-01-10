using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButtonController : MonoBehaviour
{
    // ไปด่านถัดไป (ใช้กับปุ่ม Win)
    public void LoadNextLevel()
    {
        Time.timeScale = 1f;

        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        // ถ้าไม่มีด่านถัดไป  กลับเมนู
        if (nextIndex >= SceneManager.sceneCountInBuildSettings)
        {
            LoadMainMenu();
        }
        else
        {
            SceneManager.LoadScene(nextIndex);
        }
    }

    // กลับเมนูหลัก
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    // รีสตาร์ทด่าน (แถมให้ เผื่อใช้)
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ออกจากเกม (ใช้ตอน Build จริง)
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}
