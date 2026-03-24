using UnityEngine;

public class SecretItem : MonoBehaviour
{
    [Header("Sound")]
    public AudioClip collectSound; //  เสียงตอนเก็บ

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        //  เล่นเสียง (ไม่โดนตัดแม้ object จะหาย)
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, other.transform.position);
        }

        LevelResultManager.Instance.CollectSecret();
        Destroy(gameObject);
    }
}