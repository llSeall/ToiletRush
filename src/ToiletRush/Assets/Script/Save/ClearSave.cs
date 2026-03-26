using UnityEngine;
using UnityEngine.SceneManagement;

public class ClearSaveButton : MonoBehaviour
{
    public void ClearSaveData()
    {
        //  ลบเซฟทั้งหมด
        SaveManager.ClearSave();
        Debug.Log("Save Cleared!");

        //  โหลดฉากใหม่ (รีสตาร์ทเกม)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}