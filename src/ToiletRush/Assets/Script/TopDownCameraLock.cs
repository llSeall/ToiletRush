using UnityEngine;

public class TopDownCameraLock : MonoBehaviour
{
    public bool lockRotation = true;
    private Quaternion initialRotation;

    void Start()
    {
        initialRotation = transform.rotation;
    }

    void LateUpdate()
    {
        if (lockRotation)
            transform.rotation = initialRotation;
    }
}
