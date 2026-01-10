using UnityEngine;

public static class SaveManager
{
    private const string HAS_SAVE_KEY = "HasSave";
    private const string UNLOCK_KEY = "LevelUnlocked_";

    // ---------- STAR ----------
    public static void SaveStars(string levelName, int stars)
    {
        int oldStars = PlayerPrefs.GetInt(levelName + "_Stars", 0);

        // เก็บเฉพาะค่าที่มากกว่าเดิม
        if (stars > oldStars)
        {
            PlayerPrefs.SetInt(levelName + "_Stars", stars);
            PlayerPrefs.Save();
        }
    }

    public static int GetStars(string levelName)
    {
        return PlayerPrefs.GetInt(levelName + "_Stars", 0);
    }

    // ---------- UNLOCK ----------
    public static void UnlockLevel(string levelName)
    {
        PlayerPrefs.SetInt(levelName + "_Unlocked", 1);
        PlayerPrefs.Save();
    }

    public static bool IsLevelUnlocked(string levelName)
    {
        return PlayerPrefs.GetInt(levelName + "_Unlocked", 0) == 1;
    }
    // ปลดล็อคด่าน
    public static void UnlockLevel(int levelIndex)
    {
        PlayerPrefs.SetInt(UNLOCK_KEY + levelIndex, 1);
        PlayerPrefs.SetInt(HAS_SAVE_KEY, 1);
        PlayerPrefs.Save();
    }

    // เช็คว่าด่านปลดล็อคไหม
    public static bool IsLevelUnlocked(int levelIndex)
    {
        return PlayerPrefs.GetInt(UNLOCK_KEY + levelIndex, 0) == 1;
    }

    // มีเซฟหรือไม่
    public static bool HasSave()
    {
        return PlayerPrefs.GetInt(HAS_SAVE_KEY, 0) == 1;
    }

    // ล้างเซฟ
    public static void ClearSave()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
