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

    [Header("Chase")]
    public float chaseSpeed = 4f;
    public float lostSightDelay = 0.6f;

    [Header("Player Control")]
    public MonoBehaviour playerMovement;

    [Header("Stun")]
    public float stunTime = 3f;

    [Header("QTE")]
    public TalkQTEUI qteUI;

    //  VOICE
    [Header("Voice")]
    public AudioSource audioSource;
    public AudioClip maleVoice;
    public AudioClip femaleVoice;
    public bool isMale = true;

    private CharacterController controller;
    private Transform player;

    private int index;
    private int dir = 1;
    private float waitTimer;
    private float stunTimer;
    private float lostSightTimer;
    private int patrolReturnIndex;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        if (waypoints != null && waypoints.Length > 0)
            transform.position = waypoints[0].position;
    }

    void Update()
    {
        if (currentState == State.Patrol)
            DetectPlayer();

        //  กันเสียงค้าง
        if (currentState != State.Talk && audioSource != null && audioSource.isPlaying)
            StopVoice();

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
        Collider[] hits = Physics.OverlapSphere(transform.position, detectRadius, playerLayer);
        if (hits.Length == 0) return;

        Transform target = hits[0].transform;

        Vector3 origin = transform.position + Vector3.up * 0.8f;
        Vector3 targetPos = target.position + Vector3.up * 0.8f;
        Vector3 dir = targetPos - origin;

        RaycastHit hit;
        if (Physics.Raycast(origin, dir.normalized, out hit, dir.magnitude))
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
        if (waypoints == null || waypoints.Length <= 1)
        {
            if (animator != null)
                animator.SetFloat("Speed", 0f);
            return;
        }

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
            animator.SetFloat("Speed", 1f);
    }

    // ================= CHASE =================
    void Chase()
    {
        if (player == null)
        {
            ResetToPatrol();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > detectRadius)
        {
            ResetToPatrol();
            return;
        }

        Vector3 origin = transform.position + Vector3.up * 0.8f;
        Vector3 targetPos = player.position + Vector3.up * 0.8f;
        Vector3 dir = targetPos - origin;

        RaycastHit hit;
        if (Physics.Raycast(origin, dir.normalized, out hit, dir.magnitude))
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

        if (distanceToPlayer <= talkDistance)
        {
            StartTalk();
            return;
        }

        dir.y = 0;
        controller.Move(dir.normalized * chaseSpeed * Time.deltaTime);
        Rotate(dir);

        if (animator != null)
            animator.SetFloat("Speed", 1f);
    }

    // ================= TALK =================
    void StartTalk()
    {
        currentState = State.Talk;

        Vector3 lookDir = transform.position - player.position;
        lookDir.y = 0;
        player.rotation = Quaternion.LookRotation(lookDir);

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (playerAnimator != null)
            playerAnimator.SetBool("IsTalking", true);

        if (animator != null)
            animator.SetBool("IsTalking", true);

        PlayVoice();

        qteUI.StartQTE(this);
    }

    public void OnQTESuccess()
    {
        StopVoice();

        currentState = State.Stun;
        stunTimer = stunTime;

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (playerAnimator != null)
            playerAnimator.SetBool("IsTalking", false);

        if (animator != null)
            animator.SetBool("IsTalking", false);
    }

    void Stun()
    {
        stunTimer -= Time.deltaTime;
        if (stunTimer <= 0f)
            ResetToPatrol();
    }

    void ResetToPatrol()
    {
        StopVoice();

        player = null;
        lostSightTimer = 0f;
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

    // ================= VOICE =================
    void PlayVoice()
    {
        if (audioSource == null) return;

        AudioClip clip = isMale ? maleVoice : femaleVoice;

        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    void StopVoice()
    {
        if (audioSource == null) return;
        audioSource.Stop();
    }
}