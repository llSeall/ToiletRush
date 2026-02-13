using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement3D : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float gravity = -9.81f;
    public float knockbackDamping = 8f;
    [Header("External Speed Modifier")]
    [Range(0.1f, 1f)]
    public float speedMultiplier = 1f;

    [Header("Rotation")]
    public float rotateSpeed = 10f;

    [Header("Camera Settings")]
    public Transform playerCamera;          // Camera
    public Vector3 cameraOffset = new Vector3(0, 10f, -5f); // สูง + ดันหลัง
    public float cameraLeadDistance = 2f;   // นำหน้าทิศที่หัน
    public float cameraFollowSmooth = 5f;

    private Quaternion cameraInitialRotation;
    private Vector3 cameraVelocity;
    private bool isHit = false;

    private Animator animator;
    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 knockbackVelocity;
    private CameraShake cameraShake;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraShake = GetComponentInChildren<CameraShake>();
        animator = GetComponentInChildren<Animator>();

        if (playerCamera != null)
            cameraInitialRotation = playerCamera.rotation;
    }

    void Update()
    {
        if (!isHit)
            Move();
        ApplyGravity();
        ApplyKnockback();
    }

    void LateUpdate()
    {
        if (playerCamera == null) return;

        // ===== ล็อคมุมกล้อง =====
        playerCamera.rotation = cameraInitialRotation;

        // ===== คำนวณตำแหน่งนำหน้า =====
        Vector3 forwardLead = transform.forward * cameraLeadDistance;

        Vector3 targetPos =
            transform.position +
            cameraOffset +
            forwardLead;

        // ===== สมูทกล้อง (ไม่ชน CameraShake) =====
        playerCamera.position = Vector3.SmoothDamp(
            playerCamera.position,
            targetPos,
            ref cameraVelocity,
            1f / cameraFollowSmooth
        );
    }

    void Move()
    {
        if (isHit) return;
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 inputDir = new Vector3(h, 0, v);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float baseSpeed = isRunning ? runSpeed : walkSpeed;
        float speed = baseSpeed * speedMultiplier;


        if (inputDir.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(inputDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * rotateSpeed
            );

            controller.Move(inputDir.normalized * speed * Time.deltaTime);
        }

        // ===== Animation =====
        if (animator != null)
        {
            animator.SetFloat("Speed", inputDir.magnitude);
            animator.SetBool("IsRunning", isRunning && inputDir.magnitude > 0.1f);
        }

        if (knockbackVelocity.magnitude > 0.1f)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsRunning", false);
        }
    }

    void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    public void AddKnockback(Vector3 force)
    {
        knockbackVelocity = force;
    }

    void ApplyKnockback()
    {
        if (knockbackVelocity.magnitude < 0.05f) return;

        controller.Move(knockbackVelocity * Time.deltaTime);

        knockbackVelocity = Vector3.Lerp(
            knockbackVelocity,
            Vector3.zero,
            knockbackDamping * Time.deltaTime
        );
    }
    public void SetSlowZone(bool value)
    {
        speedMultiplier = value ? 0.4f : 1f;

        if (animator != null)
            animator.SetBool("Act", value);
    }
    public void OnHitByNPC(float shakePower = 1f)
    {
        if (isHit) return;

        isHit = true;

        if (cameraShake != null)
            cameraShake.Shake(shakePower);

        if (animator != null)
            animator.SetTrigger("Hit");

        StartCoroutine(HitRecoverRoutine());

    }
    IEnumerator HitRecoverRoutine()
    {
        // รอให้เข้า state Hit ก่อน
        yield return null;

        // ดึงความยาวคลิปจริง
        float clipLength = animator.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(clipLength);

        isHit = false;
    }


}
