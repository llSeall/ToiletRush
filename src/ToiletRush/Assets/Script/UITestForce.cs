using UnityEngine;

public class UITestForce : MonoBehaviour
{
    public GameObject needKeyUI;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("FORCE SHOW UI");
            needKeyUI.SetActive(true);
        }
    }
}
