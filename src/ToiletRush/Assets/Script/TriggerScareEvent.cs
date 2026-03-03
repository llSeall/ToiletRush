using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ImgTriggerEffect : MonoBehaviour
{
    [Header("UI Image Object")]
    public GameObject imageObject;   // << ใส่ GameObject ของ Image
    public Image scareImage;
    public Sprite imageToShow;

    [Header("NPC Animators")]
    public Animator character1Animator;
    public Animator character2Animator;
    public string scareTriggerName = "Scare";

    [Header("Timing")]
    public float showDuration = 2f;
    public float fadeDuration = 1f;

    private bool playing = false;

    private void Start()
    {
        // ปิดไว้ตั้งแต่ต้น
        if (imageObject != null)
            imageObject.SetActive(false);
    }

    public void PlayEffect()
    {
        if (playing) return;
        playing = true;

        //  เปิด GameObject ก่อนทุกครั้ง
        if (imageObject != null)
            imageObject.SetActive(true);

        if (scareImage != null)
        {
            if (imageToShow != null)
                scareImage.sprite = imageToShow;

            Color c = scareImage.color;
            c.a = 1f;
            scareImage.color = c;
        }

        character1Animator?.SetTrigger(scareTriggerName);
        character2Animator?.SetTrigger(scareTriggerName);

        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(showDuration);

        if (scareImage != null)
        {
            float timer = 0f;
            Color c = scareImage.color;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                c.a = Mathf.Lerp(1f, 0f, timer / fadeDuration);
                scareImage.color = c;
                yield return null;
            }

            c.a = 0f;
            scareImage.color = c;
        }

        //  ปิดกลับหลังจบ
        if (imageObject != null)
            imageObject.SetActive(false);

        playing = false;
    }
}