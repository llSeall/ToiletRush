using UnityEngine;

public class NPCSlowZone : MonoBehaviour
{
    [Range(0.1f, 1f)]
    public float slowAmount = 0.4f;

    [Header("Zone Music")]
    public AudioClip zoneMusic;

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement3D player = other.GetComponent<PlayerMovement3D>();

        if (player != null)
        {
            player.SetSlowZone(true);

            //  เข้าโซน  เล่นเพลง
            ZoneAudioController.Instance.EnterZone(zoneMusic);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerMovement3D player = other.GetComponent<PlayerMovement3D>();

        if (player != null)
        {
            player.SetSlowZone(false);

            // ออกจากโซน คืนเสียงปกติ
            ZoneAudioController.Instance.ExitZone();
        }
    }
}