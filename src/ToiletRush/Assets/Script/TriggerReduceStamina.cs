using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TriggerReduceStamina : MonoBehaviour
{
    public float staminaReduceAmount = 10f;
    public bool destroyAfterTrigger = false;

    [Header("Animation")]
    public string hitTriggerName = "Hit2";
    [Header("UI Effect")]
    public UIShake uiShake;
    [Header("Sound")]
    public AudioClip hitSound;   // เสียงตอนโดน
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // ===== ลด Stamina =====
        StaminaSystem stamina = other.GetComponentInParent<StaminaSystem>();

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
        Animator anim = other.GetComponentInParent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger(hitTriggerName);
        }
        else
        {
            Debug.LogError("Animator NOT FOUND on Player");
        }

        // =====  เล่นเสียง =====
        //  เสียง (ไม่โดนตัด)
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }

        //  เขย่า UI
        if (uiShake != null)
        {
            uiShake.Shake();
        }

        // ===== ทำลายวัตถุ =====
        if (destroyAfterTrigger)
        {
            Destroy(gameObject, 0.1f); // delay นิดนึงให้เสียงติดก่อน
        }
    }
}