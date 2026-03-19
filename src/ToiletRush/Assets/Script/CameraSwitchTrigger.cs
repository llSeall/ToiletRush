using UnityEngine;

public class CameraSwitchTrigger : MonoBehaviour
{
    [Header("Cameras to Disable")]
    public SecurityCameraAI[] cameras;

    [Header("Sound")]
    public AudioClip disableSound;

    [Header("Prop Visual")]
    public SpriteRenderer spriteRenderer;   // ตัวพร็อบในฉาก
    public Sprite afterTriggerSprite;       // สไปร์หลังใช้งาน

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (triggered) return;

        triggered = true;

        foreach (SecurityCameraAI cam in cameras)
        {
            if (cam != null)
            {
                cam.DisableCameraFully();

                //  เล่นเสียง (ไม่โดนตัด)
                if (disableSound != null)
                {
                    AudioSource.PlayClipAtPoint(disableSound, cam.transform.position);
                }
            }
        }

        //  เปลี่ยน sprite ของพร็อบ
        if (spriteRenderer != null && afterTriggerSprite != null)
        {
            spriteRenderer.sprite = afterTriggerSprite;
        }

        //  ปิด Trigger ไปเลย (กันชนซ้ำ 100%)
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
    }
}