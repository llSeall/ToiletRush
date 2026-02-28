using UnityEngine;

public class TriggerReduceStamina : MonoBehaviour
{
    public float staminaReduceAmount = 10f;
    public bool destroyAfterTrigger = false;

    //[Header("Animation")]
    //public string hitTriggerName = "Hit2";

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

        // ===== เรียก Animation =====
        //Animator anim = other.GetComponentInParent<Animator>();
        //if (anim != null)
        //{
        //    anim.SetTrigger(hitTriggerName);
        //}
        //else
        //{
        //    Debug.LogError("Animator NOT FOUND on Player");
        //}

        // ===== ทำลายวัตถุ =====
        if (destroyAfterTrigger)
        {
            Destroy(gameObject);
        }
    }
}