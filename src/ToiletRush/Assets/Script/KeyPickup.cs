using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    public string keyID = "RedKey";

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

            //  สร้างตัวเล่นเสียงแยก
            if (pickupSound != null)
            {
                GameObject soundObj = new GameObject("PickupSound");
                soundObj.transform.position = transform.position;

                AudioSource audioSource = soundObj.AddComponent<AudioSource>();
                audioSource.clip = pickupSound;
                audioSource.volume = volume;
                audioSource.Play();

                Destroy(soundObj, pickupSound.length);
            }

            //  ลบทันที (หายไว)
            Destroy(gameObject);
        }
    }
}