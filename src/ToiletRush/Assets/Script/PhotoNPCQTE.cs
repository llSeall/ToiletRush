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
    public MonoBehaviour playerMovementScript;   // ใส่สคริปต์เดินของผู้เล่น
    public Animator playerAnimator;
    [Header("Game Over Image")]
    public UnityEngine.UI.Image gameOverImage;   // ตัว Image บน Canvas
    public Sprite gameOverSprite;                // รูปเหตุผลของฉากนี้

    [Header("Animation Parameter")]
    public string poseTriggerName = "Pose"; // Trigger ใน Animator

    private bool qteActive = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !qteActive)
        {
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
        if (gameOverImage != null && gameOverSprite != null)
            gameOverImage.sprite = gameOverSprite;
        Time.timeScale = 0f;
        enabled = false;

    }


    void EndQTE()
    {
        qteActive = false;
        playerAnimator.SetBool("Pose", false);

        playerAnimator.SetFloat("Speed", 0f);

        // ปลด Freeze
        if (playerMovementScript != null)
            playerMovementScript.enabled = true;
    }
}
