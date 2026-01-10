using UnityEngine;

public class SecretItem : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        LevelResultManager.Instance.CollectSecret();
        Destroy(gameObject);
    }
}
