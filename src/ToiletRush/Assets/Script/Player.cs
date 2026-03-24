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
    public Transform playerCamera;
    public Vector3 cameraOffset = new Vector3(0, 10f, -5f);
    public float cameraLeadDistance = 2f;
    public float cameraFollowSmooth = 5f;

    [Header("Zoom Out (Map View)")]
    public KeyCode zoomKey = KeyCode.LeftControl;
    public Vector3 zoomOutOffset = new Vector3(0, 15f, -6f); // สูง + หลังนิดหน่อย
    public float zoomSmooth = 5f;

    [Header("Peek Camera")]
    public float peekDistance = 3f;      // ระยะสูงสุดที่สามารถเหลือบได้
    public float peekSpeed = 2f;         // ความเร็วเหลือบ

    private Vector3 peekOffset;          // offset ปัจจุบันของ peek
    private Vector3 currentCameraOffset;
    private Quaternion cameraInitialRotation;
    private Vector3 cameraVelocity;
    private bool isPeeking = false;

    private bool isHit = false;

    private Animator animator;
    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 knockbackVelocity;
    private CameraShake cameraShake;
    [Header("Comic Dust Trail")]
    public ParticleSystem dustPrefab; // Particle Prefab แบบ World Space
    public float dustSpawnInterval = 0.1f; // spawn ทุก ๆ 0.1 วินาที
    private float dustSpawnTimer = 0f;
    void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraShake = GetComponentInChildren<CameraShake>();
        animator = GetComponentInChildren<Animator>();

        if (playerCamera != null)
        {
            cameraInitialRotation = playerCamera.rotation;
            currentCameraOffset = cameraOffset;
        }
    }

    void Update()
    {
        // ===== Zoom/Peek Check =====
        bool isZooming = Input.GetKey(zoomKey);
        isPeeking = isZooming; // Peek เกิดได้เฉพาะตอน Zoom

        if (!isHit && !isPeeking)
        {
            Move(); // เดินปกติ
            SpawnDustTrail();
        }
        else if (isPeeking)
        {
            HandlePeekCamera(); // เหลือบกล้อง
            StopMovementAnimation(); // ปิด animation เดิน
        }

        ApplyGravity();
        ApplyKnockback();
    }
    void HandlePeekCamera()
    {
        if (playerCamera == null) return;

        float h = Input.GetAxisRaw("Horizontal"); // A/D
        float v = Input.GetAxisRaw("Vertical");   // W/S

        // เลื่อนกล้องตาม local axes ของผู้เล่น
        Vector3 right = transform.right;   // ขวา/ซ้าย
        Vector3 forward = transform.forward; // หน้า/หลัง
        Vector3 move = (right * h + forward * v) * peekSpeed * Time.deltaTime;

        peekOffset += move;

        // จำกัดขอบเขต peek
        peekOffset.x = Mathf.Clamp(peekOffset.x, -peekDistance, peekDistance);
        peekOffset.z = Mathf.Clamp(peekOffset.z, -peekDistance, peekDistance);

        // เมื่อไม่มี input ให้เด้งกลับศูนย์กลาง smoothly
        if (h == 0 && v == 0)
        {
            peekOffset = Vector3.Lerp(peekOffset, Vector3.zero, Time.deltaTime * peekSpeed);
        }
    }
    void StopMovementAnimation()
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsRunning", false);
        }
    }

    void LateUpdate()
    {
        if (playerCamera == null) return;

        // ===== คำนวณ Target Offset =====
        Vector3 targetOffset;
        if (isPeeking)
        {
            targetOffset = zoomOutOffset + peekOffset; // zoom + peek
        }
        else if (Input.GetKey(zoomKey))
        {
            targetOffset = zoomOutOffset; // zoom ปกติ
        }
        else
        {
            targetOffset = cameraOffset; // ปกติ
        }

        // Smooth Offset
        currentCameraOffset = Vector3.Lerp(
            currentCameraOffset,
            targetOffset,
            Time.deltaTime * zoomSmooth
        );

        // ===== ศูนย์กลางเป็นผู้เล่น + นำหน้า =====
        Vector3 forwardLead = transform.forward * cameraLeadDistance;
        Vector3 targetPos = transform.position + currentCameraOffset + forwardLead;

        playerCamera.position = Vector3.SmoothDamp(
            playerCamera.position,
            targetPos,
            ref cameraVelocity,
            1f / cameraFollowSmooth
        );

        // รีเซ็ต peek เมื่อเลิกกด
        if (!isPeeking)
            peekOffset = Vector3.zero;

        // กล้องล็อคการหมุน
        playerCamera.rotation = cameraInitialRotation;
    }

    void Move()
    {
        if (isHit) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 inputDir = new Vector3(h, 0, v);
        bool isZooming = Input.GetKey(zoomKey);
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float baseSpeed = isRunning ? runSpeed : walkSpeed;

        // ซูมแล้วเดินช้าลง
        float speed = baseSpeed * (isZooming ? 0.5f : speedMultiplier);

        if (inputDir.magnitude > 0.1f)
        {
            // ซูมแล้วไม่หมุน
            if (!isZooming)
            {
                Quaternion targetRot = Quaternion.LookRotation(inputDir);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRot,
                    Time.deltaTime * rotateSpeed
                );
            }

            controller.Move(inputDir.normalized * speed * Time.deltaTime);
        }

        // Animation
        if (animator != null)
        {
            animator.SetFloat("Speed", inputDir.magnitude);
            animator.SetBool("IsRunning", isRunning && inputDir.magnitude > 0.1f && !isZooming);
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
        yield return null;

        float clipLength = animator.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(clipLength);

        isHit = false;
    }
    void SpawnDustTrail()
    {
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // เช็คเฉพาะตอนวิ่ง
        if (!isRunning || (Mathf.Abs(h) < 0.1f && Mathf.Abs(v) < 0.1f) || isHit || Input.GetKey(zoomKey))
            return;

        dustSpawnTimer += Time.deltaTime;
        if (dustSpawnTimer >= dustSpawnInterval)
        {
            dustSpawnTimer = 0f;

            if (dustPrefab != null)
            {
                // spawn ฝุ่นใกล้เท้ามากขึ้น
                Vector3 spawnPos = transform.position - transform.forward * 0.1f; // จาก 0.2  0.1
                spawnPos += new Vector3(Random.Range(-0.05f, 0.05f), 0f, Random.Range(-0.05f, 0.05f)); // offset กระจุกกว่าเดิม

                ParticleSystem dust = Instantiate(dustPrefab, spawnPos, Quaternion.identity);

                // ทำให้ฝุ่นหายเอง
                Destroy(dust.gameObject, dust.main.startLifetime.constantMax);
            }
        }
    }
}