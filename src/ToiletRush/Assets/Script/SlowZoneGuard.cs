using UnityEngine;

public class NPCSlowZone : MonoBehaviour
{
    [Range(0.1f, 1f)]
    public float slowAmount = 0.4f; // 40% ของความเร็วปกติ

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement3D player = other.GetComponent<PlayerMovement3D>();

        if (player != null)
        {
            player.SetSlowZone(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerMovement3D player = other.GetComponent<PlayerMovement3D>();

        if (player != null)
        {
            player.SetSlowZone(false);
        }
    }
}
