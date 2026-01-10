using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement3D : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float gravity = -9.81f;
    public float knockbackDamping = 8f;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 knockbackVelocity;
    private CameraShake cameraShake;


    void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraShake = GetComponentInChildren<CameraShake>();

    }

    void Update()
    {
        Move();
        ApplyGravity();
        ApplyKnockback();

    }

    void Move()
    {
        float h = Input.GetAxis("Horizontal"); // A / D
        float v = Input.GetAxis("Vertical");   // W / S

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float speed = isRunning ? runSpeed : walkSpeed;

        Vector3 move = transform.right * h + transform.forward * v;
        controller.Move(move * speed * Time.deltaTime);
    }

    void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // กันตัวละครลอย
        }

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

        // ค่อย ๆ ลดแรง (สมูท)
        knockbackVelocity = Vector3.Lerp(
            knockbackVelocity,
            Vector3.zero,
            knockbackDamping * Time.deltaTime
        );
    }
    public void OnHitByNPC(float shakePower = 1f)
    {
        if (cameraShake != null)
            cameraShake.Shake(shakePower);
    }
}
