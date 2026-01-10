using UnityEngine;

public class WinTrigger : MonoBehaviour
{
    public GameObject winCanvas;
    public StarUI starUI;
    public string levelName;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        WinGame();
    }

    void WinGame()
    {
        int stars = LevelResultManager.Instance.CalculateStars();

        SaveManager.SaveStars(levelName, stars);
        SaveManager.UnlockLevel(levelName);

        winCanvas.SetActive(true);
        starUI.ShowStars(stars);

        Time.timeScale = 0f;
    }
}
