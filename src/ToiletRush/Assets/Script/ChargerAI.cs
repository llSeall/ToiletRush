using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ChargerAI : MonoBehaviour
{
    public enum AIState { Patrol, Charging, Grab }

    [Header("Waypoints")]
    public Transform pointA;
    public Transform pointB;

    [Header("Speed Settings")]
    public float patrolSpeed = 2f;
    public float chargeSpeed = 8f;

    [Header("Vision")]
    public Collider visionTrigger;

    private CharacterController controller;
    private AIState currentState;

    private Transform currentTarget;
    private Transform chargeTarget;

    private GameObject grabbedPlayer;
    private CharacterController grabbedPlayerController;

    private Vector3 lockedChargeDirection;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentTarget = pointA;
        currentState = AIState.Patrol;
    }

    void Update()
    {
        switch (currentState)
        {
            case AIState.Patrol:
                Patrol();
                break;

            case AIState.Charging:
                Charge();
                break;

            case AIState.Grab:
                DragPlayer();
                break;
        }
    }

    // =========================
    // PATROL
    // =========================
    void Patrol()
    {
        MoveTo(currentTarget.position, patrolSpeed);

        if (Vector3.Distance(transform.position, currentTarget.position) < 0.5f)
        {
            currentTarget = (currentTarget == pointA) ? pointB : pointA;
        }
    }

    // =========================
    // START CHARGE
    // =========================
    void StartCharge(Transform finalPoint)
    {
        currentState = AIState.Charging;

        chargeTarget = finalPoint;

        // ÅçÍ¤·ÔÈ·Ò§µÍ¹àÃÔèÁªÒÃì¨
        lockedChargeDirection =
            (chargeTarget.position - transform.position).normalized;

        transform.forward = lockedChargeDirection;
    }

    // =========================
    // CHARGING
    // =========================
    void Charge()
    {
        controller.Move(lockedChargeDirection * chargeSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, chargeTarget.position) < 0.5f)
        {
            currentState = AIState.Patrol;
            currentTarget = (chargeTarget == pointA) ? pointB : pointA;
        }
    }

    // =========================
    // GRAB PLAYER
    // =========================
    void GrabPlayer(GameObject player)
    {
        grabbedPlayer = player;
        grabbedPlayerController = player.GetComponent<CharacterController>();

        if (grabbedPlayerController != null)
            grabbedPlayerController.enabled = false;

        grabbedPlayer.transform.SetParent(transform);

        currentState = AIState.Grab;
    }

    // =========================
    // DRAG PLAYER
    // =========================
    void DragPlayer()
    {
        controller.Move(lockedChargeDirection * chargeSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, chargeTarget.position) < 0.5f)
        {
            ReleasePlayer();
            currentTarget = (chargeTarget == pointA) ? pointB : pointA;
            currentState = AIState.Patrol;
        }
    }

    // =========================
    // RELEASE
    // =========================
    void ReleasePlayer()
    {
        if (grabbedPlayer != null)
        {
            grabbedPlayer.transform.SetParent(null);

            if (grabbedPlayerController != null)
                grabbedPlayerController.enabled = true;
        }

        grabbedPlayer = null;
    }

    // =========================
    // MOVE HELPER
    // =========================
    void MoveTo(Vector3 target, float speed)
    {
        Vector3 direction = (target - transform.position).normalized;
        controller.Move(direction * speed * Time.deltaTime);

        if (direction != Vector3.zero)
            transform.forward = direction;
    }

    // =========================
    // VISION TRIGGER
    // =========================
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && currentState == AIState.Patrol)
        {
            Transform finalPoint =
                Vector3.Distance(transform.position, pointA.position) <
                Vector3.Distance(transform.position, pointB.position)
                ? pointB
                : pointA;

            StartCharge(finalPoint);
        }
    }

    // =========================
    // BODY COLLISION
    // =========================
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Player") &&
            currentState == AIState.Charging)
        {
            GrabPlayer(hit.gameObject);
        }
    }
}