using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public int nextLevelIndex;
    public string nextLevelScene;

    public void LoadNextLevel()
    {
        SaveManager.UnlockLevel(nextLevelIndex);
        SceneManager.LoadScene(nextLevelScene);
    }
}
