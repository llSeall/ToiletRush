using UnityEngine;
using UnityEngine.SceneManagement;

public class ClearSaveButton : MonoBehaviour
{
    [Header("Level Setting")]
    public string currentLevelName = "Level1";
    public int currentLevelIndex = 1;

    [Header("Reward")]
    public int stars = 3;

    // -------------------------------
    //  ล้างเซฟ + รีฉาก (ของเดิม)
    // -------------------------------
    public void ClearSaveData()
    {
        SaveManager.ClearSave();
        Debug.Log("Save Cleared!");

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // -------------------------------
    //  เริ่มใหม่โดยปลดล็อคด่าน 1 + รีฉาก
    // -------------------------------
    public void UnlockLevel1AndRestart()
    {
        SaveManager.ClearSave();

        SaveManager.UnlockLevel(1);

        Debug.Log("Start from Level 1 unlocked");

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // -------------------------------
    //  ผ่านด่านทันที (ไม่รีฉาก)
    // -------------------------------
    public void CompleteLevelInstant()
    {
        // ให้ดาว
        SaveManager.SaveStars(currentLevelName, stars);

        // ปลดล็อคด่านปัจจุบัน (กันพลาด)
        SaveManager.UnlockLevel(currentLevelIndex);

        // ปลดล็อคด่านถัดไป
        SaveManager.UnlockLevel(currentLevelIndex + 1);

        Debug.Log($"DEBUG: Completed {currentLevelName} with {stars} stars");

        //  ถ้ามี UI จบด่าน ใส่ตรงนี้ได้
        // FindObjectOfType<LevelCompleteUI>()?.Show();
    }

    // -------------------------------
    //  ปลดล็อคหลายด่าน (ไม่รีฉาก)
    // -------------------------------
    public void UnlockUpToLevel(int maxLevel)
    {
        for (int i = 1; i <= maxLevel; i++)
        {
            SaveManager.UnlockLevel(i);
        }

        Debug.Log("Unlocked up to level " + maxLevel);
    }

    // -------------------------------
    // ล้างเซฟอย่างเดียว (ไม่รีฉาก)
    // -------------------------------
    public void ClearSaveOnly()
    {
        SaveManager.ClearSave();
        Debug.Log("Save Cleared (No Restart)");
    }
}