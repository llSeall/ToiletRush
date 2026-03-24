using UnityEngine;

public class ZoneImageTrigger : MonoBehaviour
{
    [Header("UI Image To Show")]
    public GameObject imageUI;

    [Header("Sound")]
    public AudioClip enterSound;

    private bool hasPlayed = false;

    private void Start()
    {
        if (imageUI != null)
            imageUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (imageUI != null)
            imageUI.SetActive(true);

        //  เล่นเสียงครั้งเดียวต่อการเข้าโซน
        if (!hasPlayed && enterSound != null)
        {
            AudioSource.PlayClipAtPoint(enterSound, transform.position);
            hasPlayed = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (imageUI != null)
            imageUI.SetActive(false);

        // รีเซ็ตให้เข้าใหม่แล้วเล่นเสียงอีก
        hasPlayed = false;
    }
}