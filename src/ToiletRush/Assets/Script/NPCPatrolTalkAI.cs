using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class NPCPatrolTalkAI : MonoBehaviour
{
    public enum State { Patrol, Chase, Talk, Stun }
    public State currentState = State.Patrol;

    [Header("Player Animation")]
    public Animator playerAnimator;

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
    public float lostSightDelay = 0.6f; // À≈∫À≈—ß ‘Ëß°’¥¢«“ß‰¥È™—Ë«§√“«

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
        transform.position = waypoints[0].position;
    }

    // ================= UPDATE =================
    void Update()
    {
        if (currentState == State.Patrol)
            DetectPlayer();

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
    }

    // ================= CHASE =================
    void Chase()
    {
        if (player == null)
        {
            ResetToPatrol();
            return;
        }

        float distanceToPlayer = Vector3.Distance(
            transform.position,
            player.position
        );

        //  À≈ÿ¥‚´π AI
        if (distanceToPlayer > detectRadius)
        {
            ResetToPatrol();
            return;
        }

        Vector3 origin = transform.position + Vector3.up * 0.8f;
        Vector3 targetPos = player.position + Vector3.up * 0.8f;
        Vector3 dir = targetPos - origin;

        //  ‡™Á§°”·æß∫—ß
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

        // ‡¢È“ Talk
        if (distanceToPlayer <= talkDistance)
        {
            StartTalk();
            return;
        }

        //  ‰≈Ë
        dir.y = 0;
        controller.Move(dir.normalized * chaseSpeed * Time.deltaTime);
        Rotate(dir);
    }

    // ================= TALK =================
    void StartTalk()
    {
        currentState = State.Talk;

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (playerAnimator != null)
        {
            playerAnimator.SetFloat("Speed", 0f);
            playerAnimator.Play("Idle", 0, 0f);
        }

        qteUI.StartQTE(this);
    }

    public void OnQTESuccess()
    {
        currentState = State.Stun;
        stunTimer = stunTime;

        if (playerMovement != null)
            playerMovement.enabled = true;
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

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
#endif
}
