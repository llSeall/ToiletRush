using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class NPCPatrolTalkAI : MonoBehaviour
{
    private Animator animator;

    public enum State { Patrol, Chase, Talk, Stun }
    public State currentState = State.Patrol;

    [Header("Player Animation")]
    public Animator playerAnimator;

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

    [Header("Player Control")]
    public MonoBehaviour playerMovement;

    [Header("Stun")]
    public float stunTime = 3f;

    [Header("QTE")]
    public TalkQTEUI qteUI;
    public StaminaSystem staminaSystem;

    private bool isQTEActive = false;

    [Header("Voice")]
    public AudioSource audioSource;
    public AudioClip maleVoice;
    public AudioClip femaleVoice;
    public bool isMale = true;

    private CharacterController controller;
    private Transform player;

    private int index;
    private int dir = 1;
    private int patrolReturnIndex;

    private float waitTimer;
    private float stunTimer;

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

        if (currentState != State.Talk && audioSource != null && audioSource.isPlaying)
            StopVoice();

        switch (currentState)
        {
            case State.Patrol: Patrol(); break;
            case State.Chase: Chase(); break;
            case State.Talk: Talk(); break;
            case State.Stun: Stun(); break;
        }

        UpdateAnimation();
    }

    // ================= DETECT =================
    void DetectPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectRadius, playerLayer);
        if (hits.Length == 0) return;

        Transform target = hits[0].transform;

        if (target.CompareTag("Player"))
        {
            player = target;
            patrolReturnIndex = index;
            currentState = State.Chase;
        }
    }

    // ================= PATROL =================
    void Patrol()
    {
        if (waypoints == null || waypoints.Length <= 1)
        {
            SetSpeed(0);
            return;
        }

        Vector3 target = waypoints[index].position;
        Vector3 dirMove = target - transform.position;
        dirMove.y = 0;

        if (dirMove.magnitude < 0.1f)
        {
            waitTimer += Time.deltaTime;
            SetSpeed(0);

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
        SetSpeed(1);
    }

    // ================= CHASE =================
    void Chase()
    {
        if (player == null)
        {
            ResetToPatrol();
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > detectRadius * 1.5f)
        {
            ResetToPatrol();
            return;
        }

        if (distance <= talkDistance)
        {
            StartTalk();
            return;
        }

        Vector3 dir = player.position - transform.position;
        dir.y = 0;

        controller.Move(dir.normalized * chaseSpeed * Time.deltaTime);
        Rotate(dir);
        SetSpeed(1);
    }

    // ================= TALK =================
    void StartTalk()
    {
        if (isQTEActive) return;

        currentState = State.Talk;
        isQTEActive = true;

        if (player == null) return;
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsTalking", true);
            playerAnimator.SetFloat("Speed", 0f); // กันค้างวิ่ง
        }
        Vector3 lookDir = transform.position - player.position;
        lookDir.y = 0;
        player.rotation = Quaternion.LookRotation(lookDir);

        if (playerMovement != null)
            playerMovement.enabled = false;

        PlayVoice();

        if (qteUI != null)
            qteUI.StartQTE(this);

        if (staminaSystem != null)
            staminaSystem.StartQTEUI();
    }

    void Talk()
    {
        //  สำคัญ ล็อคไม่ให้เดิน
        SetSpeed(0);
    }

    public void OnQTESuccess()
    {
        
        StopVoice();
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsTalking", false);
        }
        isQTEActive = false;
        currentState = State.Stun;
        stunTimer = stunTime;

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (qteUI != null)
            qteUI.StopQTE();

        if (staminaSystem != null)
            staminaSystem.StopQTEUI();
    }

    // ================= STUN =================
    void Stun()
    {
        stunTimer -= Time.deltaTime;
        SetSpeed(0);

        if (stunTimer <= 0f)
            ResetToPatrol();
    }

    // ================= RESET =================
    void ResetToPatrol()
    {
        StopVoice();
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsTalking", false);
        }
        if (qteUI != null)
            qteUI.StopQTE();

        if (staminaSystem != null)
            staminaSystem.StopQTEUI();

        isQTEActive = false;

        player = null;
        index = patrolReturnIndex;

        currentState = State.Patrol;
    }

    // ================= ANIMATION =================
    void UpdateAnimation()
    {
        if (animator == null) return;

        animator.SetBool("IsTalking", currentState == State.Talk);

        if (currentState == State.Talk || currentState == State.Stun)
            animator.SetFloat("Speed", 0f);
    }

    void SetSpeed(float value)
    {
        if (animator != null && currentState != State.Talk)
            animator.SetFloat("Speed", value);
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
        if (audioSource != null)
            audioSource.Stop();
    }
}