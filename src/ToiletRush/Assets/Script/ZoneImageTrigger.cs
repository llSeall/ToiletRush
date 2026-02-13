using UnityEngine;

public class ZoneImageTrigger : MonoBehaviour
{
    [Header("UI Image To Show")]
    public GameObject imageUI; // ลากรูป UI มาใส่

    private void Start()
    {
        if (imageUI != null)
            imageUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            imageUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            imageUI.SetActive(false);
        }
    }
}
