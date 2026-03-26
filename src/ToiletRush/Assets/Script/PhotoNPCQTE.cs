using UnityEngine;

public class PhotoNPCQTE : MonoBehaviour
{
    [Header("QTE Settings")]
    public float timeToPress = 1.5f;
    public KeyCode requiredKey = KeyCode.Space;

    [Header("Game Over")]
    public GameObject gameOverCanvas;

    [Header("References")]
    public QTE_UI_Controller qteUI;
    public MonoBehaviour playerMovementScript;
    public Animator playerAnimator;


    [Header("Animation Parameter")]
    public string poseTriggerName = "Pose";

    [Header("Sound")]
    public AudioClip triggerSound;   //  เสียงตอนเข้า QTE

    private bool qteActive = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !qteActive)
        {
            //  เล่นเสียงครั้งเดียวตอนเหยียบ
            if (triggerSound != null)
            {
                AudioSource.PlayClipAtPoint(triggerSound, other.transform.position);
            }

            StartQTE();
        }
    }

    void StartQTE()
    {
        qteActive = true;

        // Freeze Player
        if (playerMovementScript != null)
            playerMovementScript.enabled = false;

        // เล่นท่าเก๊ก
        if (playerAnimator != null)
            playerAnimator.SetBool("Pose", true);

        qteUI.StartQTE(requiredKey, timeToPress, QTESuccess, QTEFail);
    }

    void QTESuccess()
    {
        EndQTE();
        Debug.Log("แอคทัน!");
    }

    void QTEFail()
    {
        EndQTE();
        Debug.Log("โดนถ่ายรูปเฟล!");

        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(true);

        Time.timeScale = 0f;
        enabled = false;
    }

    void EndQTE()
    {
        qteActive = false;

        if (playerAnimator != null)
        {
            playerAnimator.SetBool("Pose", false);
            playerAnimator.SetFloat("Speed", 0f);
        }

        // ปลด Freeze
        if (playerMovementScript != null)
            playerMovementScript.enabled = true;
    }
}