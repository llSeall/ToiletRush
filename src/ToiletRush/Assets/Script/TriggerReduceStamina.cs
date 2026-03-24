using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class TriggerReduceStamina : MonoBehaviour
{
    public float staminaReduceAmount = 10f;
    public bool destroyAfterTrigger = false;

    [Header("Animation")]
    public string hitTriggerName = "Hit2";

    [Header("UI Effect")]
    public UIShake uiShake;

    // ---------- HIT REACTION UI ----------
    [Header("Hit Reaction UI")]
    public Image hitImage;
    public Sprite hitSprite;
    public float showTime = 0.15f;

    private Coroutine showRoutine;

    [Header("Sound")]
    public AudioClip hitSound;
    private AudioSource audioSource;

    //  กัน trigger ซ้ำ
    private bool hasTriggered = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        //  กันภาพค้างตั้งแต่เริ่ม
        if (hitImage != null)
            hitImage.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        //  กันยิงซ้ำ
        if (hasTriggered) return;

        //  กันหลาย collider
        if (!other.transform.root.CompareTag("Player")) return;

        hasTriggered = true;

        Transform playerRoot = other.transform.root;

        // ===== ลด Stamina =====
        StaminaSystem stamina = playerRoot.GetComponentInChildren<StaminaSystem>();

        if (stamina != null)
        {
            stamina.ReduceStamina(staminaReduceAmount);
            Debug.Log("Reduce stamina by " + staminaReduceAmount);
        }
        else
        {
            Debug.LogError("StaminaSystem NOT FOUND on Player");
        }

        // ===== Animation =====
        Animator anim = playerRoot.GetComponentInChildren<Animator>();
        if (anim != null)
        {
            anim.SetTrigger(hitTriggerName);
        }

        // ===== เสียง =====
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }

        // ===== UI Shake =====
        if (uiShake != null)
        {
            uiShake.Shake();
        }

        // ===== UI Reaction =====
        ShowHitFlash();

        // ===== Destroy =====
        if (destroyAfterTrigger)
        {
            Destroy(gameObject, 0.1f);
        }
    }

    // ---------- UI ----------
    void ShowHitFlash()
    {
        if (hitImage == null) return;

        hitImage.sprite = hitSprite;
        hitImage.gameObject.SetActive(true);

        if (showRoutine != null)
            StopCoroutine(showRoutine);

        showRoutine = StartCoroutine(ShowRoutine());
    }

    IEnumerator ShowRoutine()
    {
        yield return new WaitForSeconds(showTime);

        if (hitImage != null)
            hitImage.gameObject.SetActive(false);

        //  reset ให้ trigger ใช้ซ้ำได้ (ถ้าไม่ destroy)
        hasTriggered = false;
    }
}