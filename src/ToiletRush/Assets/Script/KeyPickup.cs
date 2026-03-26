using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    public string keyID = "RedKey"; // ตั้งชื่อกุญแจได้

    [Header("Sound")]
    public AudioClip pickupSound;
    [Range(0f, 1f)] public float volume = 1f;

    private bool isPicked = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isPicked) return;
        if (!other.CompareTag("Player")) return;

        PlayerKeyInventory inventory = other.GetComponentInParent<PlayerKeyInventory>();
        if (inventory != null)
        {
            isPicked = true;

            inventory.AddKey(keyID);
            Debug.Log("Picked up key: " + keyID);

            //  เล่นเสียง
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, volume);
            }

            //  ซ่อน object แทนการลบทันที
            GetComponent<Collider>().enabled = false;
            GetComponent<MeshRenderer>().enabled = false;

            //  รอให้เสียงเล่นจบแล้วค่อยลบ
            Destroy(gameObject, pickupSound != null ? pickupSound.length : 0f);
        }
    }
}