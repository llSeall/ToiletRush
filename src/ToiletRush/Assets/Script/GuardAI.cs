using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GuardAI : MonoBehaviour
{
    public enum State { Patrol, Chase, Search, Return }
    public State currentState = State.Patrol;

    [Header("Patrol")]
    public Transform[] waypoints;
    public float waitAtPoint = 1f;

    [Header("Chase")]
    public float catchDistance = 1.2f;

    [Header("Search")]
    public float searchDuration = 4f;
    public float searchRadius = 4f;

    [Header("Vision")]
    public float viewDistance = 6f;
    [Range(0, 180)] public float viewAngle = 60f;
    Vector3 currentSearchTarget;
    bool hasSearchTarget = false;
    [Header("Vision Visual")]
    public GameObject visionObject;

    [Header("Alert VFX")]
    public GameObject alertVFXPrefab;

    [Header("Shout VFX")]
    public GameObject shoutWavePrefab;

    private GameObject currentAlertVFX;
    private GameObject currentShoutWave;

    [Header("UI")]
    public GameObject gameOverCanvas;

    public Renderer visionRenderer;
    public Color normalColor = Color.yellow;
    public Color alertColor = Color.red;


    [Header("Alert Sound")]
    public AudioClip alertLoopSound;
    private AudioSource alertAudio;

    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;

    private int currentWaypointIndex = 0;
    private int waypointDirection = 1;
    private int lastWaypointIndex = 0;

    private float waitTimer;
    private float searchTimer;
    private float loseSightTimer;

    public float loseSightDelay = 2f;

    private Vector3 lastSeenPosition;
    private Vector3 chaseTarget;

    private float repathTimer = 0f;
    private float repathRate = 0.2f;

    float stuckTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        agent.speed = 4.5f;
        agent.angularSpeed = 300f;
        agent.acceleration = 50f;
        agent.stoppingDistance = 0f;
        agent.autoBraking = false;

        alertAudio = gameObject.AddComponent<AudioSource>();
        alertAudio.clip = alertLoopSound;
        alertAudio.loop = true;
        alertAudio.playOnAwake = false;
        alertAudio.spatialBlend = 1f;

        UpdateVisionColor(normalColor);

        if (waypoints.Length > 0)
            agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                CheckVision();
                break;

            case State.Chase:
                ChasePlayer();
                break;

            case State.Search:
                SearchArea();
                CheckVision();
                break;

            case State.Return:
                ReturnToPatrol();
                break;
        }

        // กัน path พังระหว่าง Chase
        if (currentState == State.Chase)
        {
            if (agent.pathStatus == NavMeshPathStatus.PathPartial ||
                agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                ForceReturnToPatrol();
            }
        }

        CheckStuck();
        UpdateAnimation(agent.velocity.magnitude);
    }

    // ---------- PATROL ----------
    void Patrol()
    {
        if (waypoints.Length == 0) return;

        if (agent.remainingDistance < 0.3f)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitAtPoint)
            {
                lastWaypointIndex = currentWaypointIndex;

                currentWaypointIndex += waypointDirection;

                if (currentWaypointIndex >= waypoints.Length)
                {
                    currentWaypointIndex = waypoints.Length - 2;
                    waypointDirection = -1;
                }
                else if (currentWaypointIndex < 0)
                {
                    currentWaypointIndex = 1;
                    waypointDirection = 1;
                }

                agent.SetDestination(waypoints[currentWaypointIndex].position);
                waitTimer = 0f;
            }
        }
    }

    // ---------- VISION ----------
    void CheckVision()
    {
        Vector3 origin = transform.position + Vector3.up * 0.8f;
        Vector3 target = player.position + Vector3.up * 0.8f;

        Vector3 dir = (target - origin).normalized;
        float dist = Vector3.Distance(origin, target);

        if (dist > viewDistance) return;

        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > viewAngle * 0.5f) return;

        RaycastHit hit;
        int mask = ~LayerMask.GetMask("Ignore Raycast");

        if (Physics.Raycast(origin, dir, out hit, dist, mask))
        {
            if (hit.transform.root.CompareTag("Player"))
            {
                ChangeState(State.Chase);

                UpdateVisionColor(alertColor);
                ShowAlertVFX();
                SpawnShoutWave();

                if (visionObject != null)
                    visionObject.SetActive(false);

                if (!alertAudio.isPlaying)
                    alertAudio.Play();
            }
        }
    }

    // ---------- CHASE ----------
    void ChasePlayer()
    {
        repathTimer += Time.deltaTime;

        if (repathTimer >= repathRate)
        {
            repathTimer = 0f;

            NavMeshHit hit;

            if (NavMesh.SamplePosition(player.position, out hit, 2f, NavMesh.AllAreas))
            {
                if (CanReach(hit.position))
                {
                    chaseTarget = hit.position;
                    agent.SetDestination(chaseTarget);
                }
                else
                {
                    ForceReturnToPatrol();
                    return;
                }
            }
            else
            {
                ForceReturnToPatrol();
                return;
            }
        }

        UpdateShoutWavePosition();

        float dist = Vector3.Distance(transform.position, chaseTarget);

        if (dist <= catchDistance)
        {
            CatchPlayer();
            return;
        }

        if (dist > viewDistance)
        {
            loseSightTimer += Time.deltaTime;

            if (loseSightTimer >= loseSightDelay)
            {
                lastSeenPosition = chaseTarget;

                ChangeState(State.Search);
                UpdateVisionColor(normalColor);

                if (visionObject != null)
                    visionObject.SetActive(true);

                HideAlertVFX();
                RemoveShoutWave();
                alertAudio.Stop();

                loseSightTimer = 0f;
            }
        }
        else
        {
            loseSightTimer = 0f;
        }
    }

    // ---------- SEARCH ----------
    void SearchArea()
    {
        if (!CanReach(lastSeenPosition))
        {
            ForceReturnToPatrol();
            return;
        }

        // ถ้ายังไม่มี target  สร้างใหม่
        if (!hasSearchTarget)
        {
            Vector3 randomPoint = lastSeenPosition + Random.insideUnitSphere * searchRadius;
            randomPoint.y = transform.position.y;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, searchRadius, NavMesh.AllAreas))
            {
                currentSearchTarget = hit.position;
                agent.SetDestination(currentSearchTarget);
                hasSearchTarget = true;
            }
        }

        // ถ้าเดินถึงจุดแล้ว
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            hasSearchTarget = false; // ไปหาจุดใหม่รอบหน้า
        }

        searchTimer += Time.deltaTime;

        if (searchTimer >= searchDuration)
        {
            hasSearchTarget = false;
            ChangeState(State.Return);
        }
    }

    // ---------- RETURN ----------
    void ReturnToPatrol()
    {
        if (waypoints.Length == 0) return;

        agent.SetDestination(waypoints[lastWaypointIndex].position);

        if (agent.remainingDistance < 0.3f)
        {
            currentWaypointIndex = lastWaypointIndex;
            ChangeState(State.Patrol);

            if (visionObject != null)
                visionObject.SetActive(true);

            UpdateVisionColor(normalColor);
        }
    }

    // ---------- INVESTIGATE ----------
    public void Investigate(Vector3 alertPosition)
    {
        NavMeshHit hit;

        if (NavMesh.SamplePosition(alertPosition, out hit, 5f, NavMesh.AllAreas))
        {
            if (CanReach(hit.position))
            {
                lastSeenPosition = hit.position;
                agent.SetDestination(hit.position);

                ChangeState(State.Search);
                UpdateVisionColor(alertColor);

                if (!alertAudio.isPlaying)
                    alertAudio.Play();
            }
        }
    }

    // ---------- FORCE RETURN ----------
    void ForceReturnToPatrol()
    {
        RemoveShoutWave();
        HideAlertVFX();
        alertAudio.Stop();

        agent.ResetPath();
        ChangeState(State.Return);
    }

    // ---------- STATE ----------
    void ChangeState(State newState)
    {
        currentState = newState;
        waitTimer = 0f;
        searchTimer = 0f;
        loseSightTimer = 0f;
        hasSearchTarget = false;
    }

    // ---------- CATCH ----------
    void CatchPlayer()
    {
        HideAlertVFX();
        RemoveShoutWave();
        alertAudio.Stop();

        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(true);


        Time.timeScale = 0f;
        enabled = false;
    }

    // ---------- VFX ----------
    void ShowAlertVFX()
    {
        if (alertVFXPrefab == null || currentAlertVFX != null) return;

        currentAlertVFX = Instantiate(alertVFXPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
        currentAlertVFX.transform.SetParent(transform);
    }

    void HideAlertVFX()
    {
        if (currentAlertVFX != null)
            Destroy(currentAlertVFX);
    }

    void SpawnShoutWave()
    {
        if (shoutWavePrefab == null || currentShoutWave != null) return;

        currentShoutWave = Instantiate(shoutWavePrefab);
    }

    void UpdateShoutWavePosition()
    {
        if (currentShoutWave == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        Vector3 pos = (dist <= viewDistance)
            ? Vector3.Lerp(transform.position, player.position, 0.5f)
            : transform.position + transform.forward * 1.5f;

        pos.y = transform.position.y + 1.5f;

        currentShoutWave.transform.position = pos;
    }

    void RemoveShoutWave()
    {
        if (currentShoutWave != null)
            Destroy(currentShoutWave);
    }

    // ---------- UTILITY ----------
    bool CanReach(Vector3 target)
    {
        NavMeshPath path = new NavMeshPath();
        return agent.CalculatePath(target, path) && path.status == NavMeshPathStatus.PathComplete;
    }

    void CheckStuck()
    {
        if (agent.velocity.sqrMagnitude < 0.1f)
        {
            stuckTimer += Time.deltaTime;

            if (stuckTimer > 2f)
            {
                ForceReturnToPatrol();
            }
        }
        else
        {
            stuckTimer = 0f;
        }
    }

    void UpdateAnimation(float speed)
    {
        if (animator != null)
            animator.SetFloat("Speed", speed);
    }

    void UpdateVisionColor(Color c)
    {
        if (visionRenderer != null)
            visionRenderer.material.color = c;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);
    }
}