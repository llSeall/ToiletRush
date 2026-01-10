using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class NPCPatrolTalkAI : MonoBehaviour
{
    public enum State { Patrol, Chase, Talk, Stun }
    public State currentState = State.Patrol;

    [Header("Patrol")]
    public Transform[] waypoints;
    public float moveSpeed = 2f;
    public float waitTimeAtPoint = 0.5f;

    [Header("Detect")]
    public float detectRadius = 5f;
    public float talkDistance = 1.5f;
    public LayerMask playerLayer;

    [Header("Chase")]
    public float chaseSpeed = 4f;

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

    void Start()
    {
        controller = GetComponent<CharacterController>();
        transform.position = waypoints[0].position;
    }

    void Update()
    {
        DetectPlayer();

        switch (currentState)
        {
            case State.Patrol: Patrol(); break;
            case State.Chase: Chase(); break;
            case State.Talk: break;
            case State.Stun: Stun(); break;
        }
    }

    // ---------------- DETECT ----------------
    void DetectPlayer()
    {
        if (currentState == State.Talk || currentState == State.Stun) return;

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            detectRadius,
            playerLayer
        );

        if (hits.Length > 0)
        {
            player = hits[0].transform;
            currentState = State.Chase;
        }
        else if (currentState == State.Chase)
        {
            player = null;
            currentState = State.Patrol;
        }
    }

    // ---------------- PATROL ----------------
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
                waitTimer = 0;
            }
            return;
        }

        controller.Move(dirMove.normalized * moveSpeed * Time.deltaTime);
        Rotate(dirMove);
    }

    // ---------------- CHASE ----------------
    void Chase()
    {
        if (player == null) return;

        Vector3 dirMove = player.position - transform.position;
        dirMove.y = 0;

        if (dirMove.magnitude <= talkDistance)
        {
            StartTalk();
            return;
        }

        controller.Move(dirMove.normalized * chaseSpeed * Time.deltaTime);
        Rotate(dirMove);
    }

    // ---------------- TALK ----------------
    void StartTalk()
    {
        currentState = State.Talk;
        qteUI.StartQTE(this);
    }

    public void OnQTESuccess()
    {
        currentState = State.Stun;
        stunTimer = stunTime;
    }

    // ---------------- STUN ----------------
    void Stun()
    {
        stunTimer -= Time.deltaTime;
        if (stunTimer <= 0f)
        {
            currentState = State.Patrol;
        }
    }

    // ---------------- UTILS ----------------
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
