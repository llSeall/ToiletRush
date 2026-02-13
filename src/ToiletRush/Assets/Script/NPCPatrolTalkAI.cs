using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class NPCPatrolTalkAI : MonoBehaviour
{
    private Animator animator;

    public enum State { Patrol, Chase, Talk, Stun }
    public State currentState = State.Patrol;

    [Header("Player Animation")]
    public Animator playerAnimator;
    private State previousState;

    [Header("Patrol")]
    public Transform[] waypoints;
    public float moveSpeed = 2f;
    public float waitTimeAtPoint = 0.5f;

    [Header("Detect (AI Zone)")]
    public float detectRadius = 5f;
    public float talkDistance = 1.5f;
    public LayerMask playerLayer;
    public LayerMask obstacleMask;

    [Header("Chase")]
    public float chaseSpeed = 4f;
    public float lostSightDelay = 0.6f; // หลบหลังสิ่งกีดขวางได้ชั่วคราว

    [Header("Player Control")]
    public MonoBehaviour playerMovement;

    [Header("Stun")]
    public float stunTime = 3f;

    [Header("QTE")]
    public TalkQTEUI qteUI;

    private CharacterController controller;
    private Transform player;

    private int index;
    private int dir = 1;
    private float waitTimer;
    private float stunTimer;
    private float lostSightTimer;

    private int patrolReturnIndex;

    // ================= START =================
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        transform.position = waypoints[0].position;
    }


    // ================= UPDATE =================
    void Update()
    {
        if (currentState == State.Patrol)
            DetectPlayer();

        if (currentState != previousState)
        {
            UpdateAnimationByState();
            previousState = currentState;
        }

        switch (currentState)
        {
            case State.Patrol: Patrol(); break;
            case State.Chase: Chase(); break;
            case State.Talk: break;
            case State.Stun: Stun(); break;
        }
    }


    // ================= DETECT =================
    void DetectPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            detectRadius,
            playerLayer
        );

        if (hits.Length == 0) return;

        Transform target = hits[0].transform;

        Vector3 origin = transform.position + Vector3.up * 0.8f;
        Vector3 targetPos = target.position + Vector3.up * 0.8f;
        Vector3 dir = targetPos - origin;
        float distance = dir.magnitude;

        RaycastHit hit;
        if (Physics.Raycast(origin, dir.normalized, out hit, distance, ~0))
        {
            if (hit.transform.CompareTag("Player"))
            {
                player = target;
                patrolReturnIndex = index;
                lostSightTimer = 0f;
                currentState = State.Chase;
            }
        }
    }

    // ================= PATROL =================
    void Patrol()
    {
        Vector3 target = waypoints[index].position;
        Vector3 dirMove = target - transform.position;
        dirMove.y = 0;

        if (dirMove.magnitude < 0.1f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtPoint)
            {
                index += dir;
                if (index >= waypoints.Length || index < 0)
                {
                    dir *= -1;
                    index += dir * 2;
                }
                waitTimer = 0f;
            }
            return;
        }

        controller.Move(dirMove.normalized * moveSpeed * Time.deltaTime);
        Rotate(dirMove);
        if (animator != null)
        {
            animator.SetBool("IsTalking", false);
            animator.SetFloat("Speed", dirMove.magnitude > 0.1f ? 1f : 0f);
        }

    }

    // ================= CHASE =================
    void Chase()
    {
        if (animator != null)
        {
            animator.SetBool("IsTalking", false);
            animator.SetFloat("Speed", 1f);
        }

        if (player == null)
        {
            ResetToPatrol();
            return;
        }

        float distanceToPlayer = Vector3.Distance(
            transform.position,
            player.position
        );

        //  หลุดโซน AI
        if (distanceToPlayer > detectRadius)
        {
            ResetToPatrol();
            return;
        }

        Vector3 origin = transform.position + Vector3.up * 0.8f;
        Vector3 targetPos = player.position + Vector3.up * 0.8f;
        Vector3 dir = targetPos - origin;

        //  เช็คกำแพงบัง
        RaycastHit hit;
        if (Physics.Raycast(origin, dir.normalized, out hit, dir.magnitude, ~0))
        {
            if (!hit.transform.CompareTag("Player"))
            {
                lostSightTimer += Time.deltaTime;
                if (lostSightTimer >= lostSightDelay)
                {
                    ResetToPatrol();
                    return;
                }
            }
            else
            {
                lostSightTimer = 0f;
            }
        }

        // เข้า Talk
        if (distanceToPlayer <= talkDistance)
        {
            StartTalk();
            return;
        }

        //  ไล่
        dir.y = 0;
        controller.Move(dir.normalized * chaseSpeed * Time.deltaTime);
        Rotate(dir);
    }

    // ================= TALK =================
    void StartTalk()
    {
        Debug.Log("StartTalk called");

        currentState = State.Talk;

        // ให้ผู้เล่นหันมาหา NPC
        Vector3 lookDir = transform.position - player.transform.position;
        lookDir.y = 0;
        player.transform.rotation = Quaternion.LookRotation(lookDir);

        if (playerMovement != null)
            playerMovement.enabled = false;

        // ===== PLAYER ANIMATION =====
        if (playerAnimator != null)
        {
            playerAnimator.SetFloat("Speed", 0f);
            playerAnimator.SetBool("IsRunning", false);
            playerAnimator.SetBool("IsTalking", true);
        }

        // ===== NPC ANIMATION =====
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsTalking", true);   //  ตัวสำคัญ
        }

        qteUI.StartQTE(this);
    }



    public void OnQTESuccess()
    {
        currentState = State.Stun;
        stunTimer = stunTime;

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (playerAnimator != null)
            playerAnimator.SetBool("IsTalking", false); //  ปิดท่าคุย

        if (animator != null)
            animator.SetBool("IsTalking", false);
    }


    // ================= STUN =================
    void Stun()
    {
        stunTimer -= Time.deltaTime;
        if (stunTimer <= 0f)
        {
            ResetToPatrol();
        }
    }

    // ================= RESET =================
    void ResetToPatrol()
    {
        player = null;
        lostSightTimer = 0f;
        index = patrolReturnIndex;
        currentState = State.Patrol;
    }

    // ================= UTILS =================
    void Rotate(Vector3 dir)
    {
        if (dir == Vector3.zero) return;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            Time.deltaTime * 5f
        );
    }
    void UpdateAnimationByState()
    {
        if (animator == null) return;

        switch (currentState)
        {
            case State.Patrol:
                animator.SetBool("IsTalking", false);
                animator.SetFloat("Speed", 1f);
                break;

            case State.Chase:
                animator.SetBool("IsTalking", false);
                animator.SetFloat("Speed", 1f);
                break;

            case State.Talk:
                animator.SetFloat("Speed", 0f);
                animator.SetBool("IsTalking", true);
                break;

            case State.Stun:
                animator.SetBool("IsTalking", false);
                animator.SetFloat("Speed", 0f);
                break;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
#endif
}
