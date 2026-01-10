using UnityEngine;

public class LevelUnlockManager : MonoBehaviour
{
    public int levelIndex;

    void Start()
    {
        SaveManager.UnlockLevel(levelIndex);
    }
}
