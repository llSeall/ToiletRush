using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    public string keyID = "RedKey"; // ตั้งชื่อกุญแจได้

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerKeyInventory inventory = other.GetComponentInParent<PlayerKeyInventory>();
        if (inventory != null)
        {
            inventory.AddKey(keyID);
            Debug.Log("Picked up key: " + keyID);
            Destroy(gameObject);
        }
    }
}
