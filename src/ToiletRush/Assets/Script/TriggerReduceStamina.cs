using UnityEngine;

public class TriggerReduceStamina : MonoBehaviour
{
    public float staminaReduceAmount = 10f;
    public bool destroyAfterTrigger = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // หา StaminaSystem จาก Parent ด้วย
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

        if (destroyAfterTrigger)
        {
            Destroy(gameObject);
        }
    }
}
