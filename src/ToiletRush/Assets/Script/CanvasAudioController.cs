using UnityEngine;

public class CanvasAudioController : MonoBehaviour
{
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        //  เล่นเพลงตอน canvas โผล่
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    void OnDisable()
    {
        //  หยุดตอน canvas หาย
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}