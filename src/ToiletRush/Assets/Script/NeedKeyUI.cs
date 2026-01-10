using UnityEngine;

public class NeedKeyUI : MonoBehaviour
{
    public float showDuration = 2f;
    float timer;


    void OnEnable()
    {
        timer = showDuration;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}
