using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIAudioManager : MonoBehaviour
{
    [Header("Sounds")]
    public AudioClip clickSound;
    public AudioClip hoverSound;

    [Header("Settings")]
    public float hoverCooldown = 0.05f;

    private AudioSource audioSource;
    private float lastHoverTime;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.ignoreListenerPause = true;
        SetupAllButtons();
    }

    void SetupAllButtons()
    {
        Button[] allButtons = FindObjectsOfType<Button>(true);

        foreach (Button btn in allButtons)
        {
            if (btn == null) continue;

            //  Click
            btn.onClick.AddListener(() =>
            {
                PlayClick();
            });

            //  Hover (ใช้ custom component)
            UIHoverHandler hover = btn.gameObject.GetComponent<UIHoverHandler>();
            if (hover == null)
                hover = btn.gameObject.AddComponent<UIHoverHandler>();

            hover.manager = this;
        }
    }

    public void PlayClick()
    {
        if (clickSound != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(clickSound);
        }
    }

    public void PlayHover()
    {
        if (Time.unscaledTime - lastHoverTime < hoverCooldown) return;
        lastHoverTime = Time.unscaledTime;

        if (hoverSound != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(hoverSound);
        }
    }
}

public class UIHoverHandler : MonoBehaviour, IPointerEnterHandler
{
    public UIAudioManager manager;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (manager != null)
        {
            manager.PlayHover();
        }
        Debug.Log(gameObject.name);
    }
}