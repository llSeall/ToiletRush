using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class NPCPatrolReduceStamina : MonoBehaviour
{
    [Header("Patrol")]
    public Transform[] waypoints;
    public float moveSpeed = 2f;
    public float waitTimeAtPoint = 0.5f;

    [Header("Hit")]
    public float staminaReduceAmount = 10f;
    public float knockbackForce = 6f;
    public float hitCooldown = 5f;

    private CharacterController controller;
    private int index;
    private int dir = 1;
    private float waitTimer;
    private bool canHit = true;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (waypoints.Length > 0)
            transform.position = waypoints[0].position;
    }

    void Update()
    {
        Patrol();
    }

    // ---------- PATROL ----------
    void Patrol()
    {
        if (waypoints.Length == 0) return;

        Vector3 target = waypoints[index].position;
        Vector3 moveDir = target - transform.position;
        moveDir.y = 0;

        if (moveDir.magnitude < 0.1f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtPoint)
            {
                index += dir;

                if (index >= waypoints.Length)
                {
                    dir = -1;
                    index = waypoints.Length - 2;
                }
                else if (index < 0)
                {
                    dir = 1;
                    index = 1;
                }

                waitTimer = 0f;
            }
            return;
        }

        controller.Move(moveDir.normalized * moveSpeed * Time.deltaTime);

        if (moveDir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(moveDir),
                Time.deltaTime * 5f
            );
    }

    // ---------- HIT PLAYER ----------
    // เรียกจาก Trigger
    public void OnHitPlayer(Collider other)
    {
        if (!canHit) return;

        PlayerMovement3D player = other.GetComponentInParent<PlayerMovement3D>();
        StaminaSystem stamina = other.GetComponentInParent<StaminaSystem>();

        if (player == null || stamina == null) return;

        // ลด stamina
        stamina.ReduceStamina(staminaReduceAmount);

        // knockback
        Vector3 dir = (other.transform.position - transform.position).normalized;
        dir.y = 0;
        player.AddKnockback(dir * knockbackForce);

        canHit = false;
        Invoke(nameof(ResetHit), hitCooldown);
        player.OnHitByNPC(1f);

    }

    void ResetHit()
    {
        canHit = true;
    }
}
