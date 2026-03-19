using UnityEngine;
using System.Collections;

public class ZoneAudioController : MonoBehaviour
{
    public static ZoneAudioController Instance;

    [Header("Audio Sources")]
    public AudioSource[] normalAudioSources; // เสียงเกมทั้งหมด (BGM, ambient, etc.)
    public AudioSource zoneMusicSource;      // เพลงโซน

    [Header("Settings")]
    public float fadeDuration = 1.5f;
    public float normalVolume = 1f;
    public float mutedVolume = 0f;

    private Coroutine currentFade;

    void Awake()
    {
        Instance = this;
    }

    public void EnterZone(AudioClip zoneClip)
    {
        if (currentFade != null)
            StopCoroutine(currentFade);

        zoneMusicSource.clip = zoneClip;
        zoneMusicSource.volume = 0f;
        zoneMusicSource.loop = true;
        zoneMusicSource.Play();

        currentFade = StartCoroutine(FadeAudio(true));
    }

    public void ExitZone()
    {
        if (currentFade != null)
            StopCoroutine(currentFade);

        currentFade = StartCoroutine(FadeAudio(false));
    }

    IEnumerator FadeAudio(bool entering)
    {
        float timer = 0f;

        float startNormal = entering ? normalVolume : mutedVolume;
        float endNormal = entering ? mutedVolume : normalVolume;

        float startZone = entering ? 0f : 1f;
        float endZone = entering ? 1f : 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;

            float normalVol = Mathf.Lerp(startNormal, endNormal, t);
            float zoneVol = Mathf.Lerp(startZone, endZone, t);

            //  ปรับเสียงทั้งหมดในเกม
            foreach (var src in normalAudioSources)
            {
                if (src != null)
                    src.volume = normalVol;
            }

            //  เพลงโซน
            zoneMusicSource.volume = zoneVol;

            yield return null;
        }

        // ปิดเพลงเมื่อออกโซน
        if (!entering)
        {
            zoneMusicSource.Stop();
        }
    }
}