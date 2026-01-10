using UnityEngine;

public class CameraSwitchTrigger : MonoBehaviour
{
    [Header("Cameras to Disable")]
    public SecurityCameraAI[] cameras;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        foreach (SecurityCameraAI cam in cameras)
        {
            if (cam != null)
                cam.DisableCameraFully();
        }
    }
}
