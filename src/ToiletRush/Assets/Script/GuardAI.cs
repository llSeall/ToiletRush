using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GuardAI : MonoBehaviour
{
    public enum State { Patrol, Chase, Search, Return }
    public State currentState = State.Patrol;

    [Header("Patrol (Smart Random)")]
    public Transform[] waypoints;
    public float patrolRadius = 6f;
    public float waitAtPoint = 1f;
    [Range(0, 1)] public float waypointBias = 0.6f;

    [Header("Chase")]
    public float catchDistance = 1.2f;

    [Header("Search")]
    public float searchDuration = 4f;
    public float searchRadius = 4f;

    [Header("Vision")]
    public float viewDistance = 6f;
    [Range(0, 180)] public float viewAngle = 60f;

    [Header("Vision Visual")]
    public GameObject visionObject; //  เพิ่มตัวนี้

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

    [Header("Game Over Image")]
    public UnityEngine.UI.Image gameOverImage;
    public Sprite gameOverSprite;

    [Header("Alert Sound")]
    public AudioClip alertLoopSound;
    private AudioSource alertAudio;

    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;

    private float waitTimer;
    private Vector3 lastSeenPosition;
    private float searchTimer;

    //  ใหม่ delay การมองหาย
    private float loseSightTimer = 0f;
    public float loseSightDelay = 2f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        UpdateVisionColor(normalColor);

        agent.speed = 4.5f;
        agent.angularSpeed = 180f;
        agent.acceleration = 8f;
        agent.autoBraking = false;
        agent.stoppingDistance = 0f;
        agent.acceleration = 20f;
        agent.angularSpeed = 300f;
        agent.stoppingDistance = 0f;


        alertAudio = gameObject.AddComponent<AudioSource>();
        alertAudio.clip = alertLoopSound;
        alertAudio.loop = true;
        alertAudio.playOnAwake = false;
        alertAudio.spatialBlend = 1f;

        PickNextPatrolPoint();
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

        UpdateAnimation(agent.velocity.magnitude);
    }

    // ---------- PATROL ----------
    void Patrol()
    {
        if (agent.pathPending) return;

        if (agent.remainingDistance < 0.4f)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitAtPoint)
            {
                PickNextPatrolPoint();
                waitTimer = 0f;
            }
        }
    }

    void PickNextPatrolPoint()
    {
        Vector3 target;

        if (waypoints.Length > 0 && Random.value < 0.5f)
        {
            target = waypoints[Random.Range(0, waypoints.Length)].position;
        }
        else
        {
            do
            {
                target = GetRandomPointOnNavMesh(50f);
            }
            while (Vector3.Distance(transform.position, target) < 5f);
        }

        agent.SetDestination(target);
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

        //  สำคัญ: ไม่ใช้ obstacleMask แล้ว
        int mask = ~LayerMask.GetMask("Ignore Raycast");

        if (Physics.Raycast(origin, dir, out hit, dist, mask, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform.root.CompareTag("Player"))
            {
                currentState = State.Chase;
                UpdateVisionColor(alertColor);

                ShowAlertVFX();
                SpawnShoutWave();

                if (visionObject != null)
                    visionObject.SetActive(false); // ปิด cone

                if (!alertAudio.isPlaying)
                    alertAudio.Play();
            }
        }
    }

    // ---------- CHASE ----------
    void ChasePlayer()
    {
        agent.SetDestination(player.position);
        agent.speed = 4.5f;

        float dist = Vector3.Distance(transform.position, player.position);

        UpdateShoutWavePosition();

        if (dist <= catchDistance)
        {
            CatchPlayer();
            return;
        }

        //  ระบบ delay การมองหาย
        if (dist > viewDistance)
        {
            loseSightTimer += Time.deltaTime;

            if (loseSightTimer >= loseSightDelay)
            {
                lastSeenPosition = player.position;
                searchTimer = 0f;

                currentState = State.Search;
                UpdateVisionColor(normalColor);

                if (visionObject != null)
                    visionObject.SetActive(true); //  เปิด cone กลับ

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
        agent.SetDestination(lastSeenPosition);

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            searchTimer += Time.deltaTime;

            if (searchTimer >= searchDuration)
            {
                currentState = State.Return;
            }
        }
    }

    // ---------- RETURN ----------
    void ReturnToPatrol()
    {
        PickNextPatrolPoint();
        currentState = State.Patrol;

        if (visionObject != null)
            visionObject.SetActive(true);
    }

    // ---------- INVESTIGATE ----------
    public void Investigate(Vector3 alertPosition)
    {
        lastSeenPosition = alertPosition;
        currentState = State.Search;
        searchTimer = 0f;

        agent.SetDestination(alertPosition);
        UpdateVisionColor(alertColor);
    }

    // ---------- CATCH ----------
    void CatchPlayer()
    {
        HideAlertVFX();
        RemoveShoutWave();
        alertAudio.Stop();

        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(true);

        if (gameOverImage != null && gameOverSprite != null)
            gameOverImage.sprite = gameOverSprite;

        Time.timeScale = 0f;
        enabled = false;
    }

    // ---------- VISUAL ----------
    void UpdateVisionColor(Color c)
    {
        if (visionRenderer != null)
            visionRenderer.material.color = c;
    }

    void UpdateAnimation(float speed)
    {
        if (animator != null)
            animator.SetFloat("Speed", speed);
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
        if (currentShoutWave == null || player == null) return;

        Vector3 mid = Vector3.Lerp(transform.position, player.position, 0.5f);
        mid.y = transform.position.y + 1.5f;

        currentShoutWave.transform.position = mid;
    }

    void RemoveShoutWave()
    {
        if (currentShoutWave != null)
            Destroy(currentShoutWave);
    }

    Vector3 GetRandomPointOnNavMesh(float range)
    {
        Vector3 randomDirection = Random.insideUnitSphere * range + transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, range, NavMesh.AllAreas))
            return hit.position;

        return transform.position;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);
    }
}