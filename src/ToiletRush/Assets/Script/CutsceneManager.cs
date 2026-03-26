using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;

public class CutsceneManager : MonoBehaviour
{
    [Header("Cutscene")]
    public GameObject cutsceneCanvas;
    public GameObject mainMenuUI;

    [Header("Video")]
    public VideoPlayer videoPlayer;

    [Header("Fade")]
    public Image fadeImage;
    public float fadeDuration = 1.2f;

    [Header("Scene")]
    public string level1SceneName = "Level1";
    [Header("Main Menu BGM")]
    public AudioSource mainMenuBGM;
    public float bgmFadeDuration = 1f;
    private bool isFading = false;

    void Start()
    {
        cutsceneCanvas.SetActive(false);
        mainMenuUI.SetActive(true);
        

        // ปิด Fade ไว้ก่อน
        if (fadeImage != null)
            fadeImage.gameObject.SetActive(false);


        // ตั้งค่า Video Player ให้รองรับเสียง
        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;

            //  ใช้เสียงจากวิดีโอ
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

            // ถ้ายังไม่มี AudioSource ให้เพิ่มให้เลย
            AudioSource audioSource = videoPlayer.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = videoPlayer.gameObject.AddComponent<AudioSource>();
            }

            videoPlayer.SetTargetAudioSource(0, audioSource);
            audioSource.playOnAwake = false;

            // เมื่อวิดีโอจบ ไปโหลดฉาก
            videoPlayer.loopPointReached += OnVideoEnd;
        }
    }
    IEnumerator FadeOutBGM()
    {
        float startVolume = mainMenuBGM.volume;
        float t = 0;

        while (t < bgmFadeDuration)
        {
            t += Time.deltaTime;
            mainMenuBGM.volume = Mathf.Lerp(startVolume, 0, t / bgmFadeDuration);
            yield return null;
        }

        mainMenuBGM.volume = 0;
        mainMenuBGM.Stop();
    }
    // ===============================
    // START GAME
    // ===============================
    public void StartGame()
    {
        mainMenuUI.SetActive(false);
        cutsceneCanvas.SetActive(true);
        if (mainMenuBGM != null)
        {
            StartCoroutine(FadeOutBGM());
        }
        // เปิด Fade แล้วตั้งค่าเริ่มต้น
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = new Color(0, 0, 0, 1);
        }

        // เล่นวิดีโอ
        if (videoPlayer != null)
        {
            videoPlayer.Play();
        }

        StartCoroutine(FadeIn());
    }

    void Update()
    {
        if (!cutsceneCanvas.activeSelf) return;
        if (isFading) return;

        // กดคลิก / Space เพื่อข้าม
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(FadeOutAndLoad());
        }
    }

    // ===============================
    // VIDEO END
    // ===============================
    void OnVideoEnd(VideoPlayer vp)
    {
        StartCoroutine(FadeOutAndLoad());
    }

    // ===============================
    // FADE IN
    // ===============================
    IEnumerator FadeIn()
    {
        isFading = true;

        float t = 0;
        Color c = fadeImage.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1, 0, t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }

        c.a = 0;
        fadeImage.color = c;
        isFading = false;
    }

    // ===============================
    // FADE OUT + LOAD
    // ===============================
    IEnumerator FadeOutAndLoad()
    {
        if (isFading) yield break;

        isFading = true;

        float t = 0;
        Color c = fadeImage.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0, 1, t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }

        SceneManager.LoadScene(level1SceneName);
    }

    // ===============================
    // EXIT GAME
    // ===============================
    public void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}