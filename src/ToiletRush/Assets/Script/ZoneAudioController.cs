using UnityEngine;
using System.Collections;

public class ZoneAudioController : MonoBehaviour
{
    public static ZoneAudioController Instance;

    [Header("Audio Sources")]
    public AudioSource[] normalAudioSources;
    public AudioSource zoneMusicSource;

    [Header("Fade Settings")]
    public float fadeInDuration = 0.4f;   // เข้าโซน (เร็ว)
    public float fadeOutDuration = 0.8f;  // ออกโซน (นุ่ม)

    [Header("Volume Settings")]
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
        float duration = entering ? fadeInDuration : fadeOutDuration;
        float timer = 0f;

        float startNormal = entering ? normalVolume : 0f;
        float endNormal = entering ? 0f : normalVolume;

        float startZone = entering ? 0f : 1f;
        float endZone = entering ? 1f : 0f;

        //  ถ้าออกโซน  เปิดเสียงปกติก่อน
        if (!entering)
        {
            foreach (var src in normalAudioSources)
            {
                if (src != null && !src.isPlaying)
                    src.UnPause();
            }
        }

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, timer / duration);

            float normalVol = Mathf.Lerp(startNormal, endNormal, t);
            float zoneVol = Mathf.Lerp(startZone, endZone, t);

            foreach (var src in normalAudioSources)
            {
                if (src != null)
                    src.volume = normalVol;
            }

            zoneMusicSource.volume = zoneVol;

            yield return null;
        }

        // set ค่าสุดท้าย
        foreach (var src in normalAudioSources)
        {
            if (src != null)
                src.volume = endNormal;
        }

        zoneMusicSource.volume = endZone;

        //  เข้าโซน  ปิดเสียงปกติจริง ๆ
        if (entering)
        {
            foreach (var src in normalAudioSources)
            {
                if (src != null)
                    src.Pause(); // หายจริง
            }
        }
        else
        {
            zoneMusicSource.Stop(); // ออกจากโซน  ปิดเพลงโซน
        }
    }
}