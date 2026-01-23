using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class GuardAI : MonoBehaviour
{
    public enum State { Patrol, Chase, Search, Return }
    public State currentState = State.Patrol;

    [Header("Patrol")]
    public Transform[] waypoints;
    public float patrolSpeed = 2f;
    public float waitAtPoint = 0.5f;

    [Header("Chase")]
    public float chaseSpeed = 4f;
    public float catchDistance = 1.2f;

    [Header("Search")]
    public float searchDuration = 3f;
    public float searchRadius = 2f;
    public float searchSpeed = 2.5f;

    [Header("Vision")]
    public float viewDistance = 6f;
    [Range(0, 180)] public float viewAngle = 60f;
    public LayerMask obstacleMask;

    [Header("UI")]
    public GameObject gameOverCanvas;
    public Renderer visionRenderer;
    public Color normalColor = Color.yellow;
    public Color alertColor = Color.red;

    private CharacterController controller;
    private Transform player;

    private int currentIndex = 0;
    private int direction = 1;
    private float waitTimer;

    private int patrolReturnIndex;
    private Vector3 lastSeenPosition;

    private float searchTimer;
    private Vector3 searchTarget;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        transform.position = waypoints[0].position;
        UpdateVisionColor(normalColor);
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
                break;

            case State.Return:
                ReturnToPatrol();
                break;
        }
    }

    // ---------- PATROL ----------
    void Patrol()
    {
        Vector3 target = waypoints[currentIndex].position;

        Vector3 flatPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 flatTarget = new Vector3(target.x, 0, target.z);

        MoveTo(target, patrolSpeed);

        if (Vector3.Distance(flatPos, flatTarget) < 0.4f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitAtPoint)
            {
                NextPoint();
                waitTimer = 0f;
            }
        }
    }


    void NextPoint()
    {
        currentIndex += direction;

        if (currentIndex >= waypoints.Length)
        {
            direction = -1;
            currentIndex = waypoints.Length - 2;
        }
        else if (currentIndex < 0)
        {
            direction = 1;
            currentIndex = 1;
        }
    }

    // ---------- VISION ----------
    void CheckVision()
    {
        Vector3 origin = transform.position + Vector3.up * 0.8f;
        Vector3 target = player.position + Vector3.up * 0.8f;

        Vector3 dirToPlayer = (target - origin).normalized;
        float distance = Vector3.Distance(origin, target);

        if (distance > viewDistance) return;

        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > viewAngle * 0.5f) return;

        RaycastHit hit;

        // ยิง Raycast ตรวจว่าโดนอะไรเป็นอันดับแรก
        if (Physics.Raycast(
            origin,
            dirToPlayer,
            out hit,
            distance,
            ~0, // ชนทุก Layer
            QueryTriggerInteraction.Ignore
        ))
        {
            if (hit.transform.CompareTag("Player"))
            {
                patrolReturnIndex = currentIndex;
                lastSeenPosition = player.position;

                currentState = State.Chase;
                UpdateVisionColor(alertColor);
            }
            // ถ้าโดนอย่างอื่นก่อน (เช่น กำแพง)  ไม่ทำอะไร
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (currentState != State.Chase) return;

        if (other.CompareTag("Player"))
        {
            CatchPlayer();
        }
    }

    // ---------- CHASE ----------
    void ChasePlayer()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= catchDistance)
        {
            CatchPlayer();
            return;
        }

        Vector3 dir = (player.position - transform.position).normalized;
        controller.Move(dir * chaseSpeed * Time.deltaTime);
        transform.rotation = Quaternion.LookRotation(dir);

        if (distance > viewDistance)
        {
            // หลุดสายตา เริ่มค้นหา
            searchTimer = 0f;
            PickNewSearchPoint();

            currentState = State.Search;
            UpdateVisionColor(normalColor);
        }
    }

    // ---------- SEARCH ----------
    void SearchArea()
    {
        searchTimer += Time.deltaTime;

        MoveTo(searchTarget, searchSpeed);
        CheckVision(); // ระหว่างค้นหายังสามารถเห็นผู้เล่นได้

        if (Vector3.Distance(transform.position, searchTarget) < 0.3f)
        {
            PickNewSearchPoint();
        }

        if (searchTimer >= searchDuration)
        {
            currentState = State.Return;
        }
    }

    void PickNewSearchPoint()
    {
        Vector2 randomCircle = Random.insideUnitCircle * searchRadius;
        searchTarget = lastSeenPosition + new Vector3(randomCircle.x, 0, randomCircle.y);
    }

    // ---------- RETURN ----------
    void ReturnToPatrol()
    {
        Vector3 target = waypoints[patrolReturnIndex].position;

        Vector3 flatPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 flatTarget = new Vector3(target.x, 0, target.z);

        MoveTo(target, patrolSpeed);

        if (Vector3.Distance(flatPos, flatTarget) < 0.4f)
        {
            currentIndex = patrolReturnIndex;
            currentState = State.Patrol;
        }
    }

    // ---------- COMMON ----------
    void MoveTo(Vector3 target, float speed)
    {
        Vector3 dir = target - transform.position;
        dir.y = 0;

        if (dir.magnitude < 0.05f) return;

        controller.Move(dir.normalized * speed * Time.deltaTime);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            Time.deltaTime * 5f
        );
    }

    void CatchPlayer()
    {
        Debug.Log("PLAYER CAUGHT!");

        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(true);

        Time.timeScale = 0f;
        enabled = false;
    }

    void UpdateVisionColor(Color c)
    {
        if (visionRenderer != null)
            visionRenderer.material.color = c;
    }

    // ---------- DEBUG ----------
    void OnDrawGizmosSelected()
    {
        // วาดระยะมองเห็น
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        // วาดมุมมอง (FOV)
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewDistance);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewDistance);

        // วาดเส้นไปหาผู้เล่น (ถ้ามี)
        if (player != null)
        {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToPlayer);

            if (angle < viewAngle / 2f)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, player.position);
            }
        }

        // Search area
        if (currentState == State.Search)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(lastSeenPosition, searchRadius);
        }
    }
    public void Investigate(Vector3 alertPosition)
    {
        lastSeenPosition = alertPosition;
        searchTimer = 0f;
        PickNewSearchPoint();
        currentState = State.Search;
        UpdateVisionColor(alertColor);
    }


}
