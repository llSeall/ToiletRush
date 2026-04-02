using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ChargerAI : MonoBehaviour
{
    public enum AIState { Charging, Grab }

    [Header("Waypoints")]
    public Transform pointA;
    public Transform pointB;

    [Header("Speed")]
    public float chargeSpeed = 8f;

    [Header("Gravity")]
    public float gravity = -20f;
    private float verticalVelocity;

    private CharacterController controller;
    private AIState currentState;

    private Transform chargeTarget;
    private Vector3 lockedDirection;

    private GameObject grabbedPlayer;
    private CharacterController grabbedPlayerController;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        controller.enabled = false;
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
        controller.enabled = true;
        SetNextTarget();
        StartCharge();
    }

    void Update()
    {
        switch (currentState)
        {
            case AIState.Charging:
                Charge();
                break;

            case AIState.Grab:
                DragPlayer();
                break;
        }
    }

    // =========================
    // เลือกเป้าหมาย A/B
    // =========================
    void SetNextTarget()
    {
        if (chargeTarget == null || chargeTarget == pointB)
            chargeTarget = pointA;
        else
            chargeTarget = pointB;
    }

    // =========================
    // เริ่ม Charge
    // =========================
    void StartCharge()
    {
        currentState = AIState.Charging;

        Vector3 dir = chargeTarget.position - transform.position;
        dir.y = 0f; //  ตัดแกน Y

        lockedDirection = dir.normalized;

        if (lockedDirection != Vector3.zero)
            transform.forward = lockedDirection;
    }

    // =========================
    // APPLY MOVEMENT + GRAVITY
    // =========================
    void ApplyMovement(Vector3 horizontalMove)
    {
        if (controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 move = horizontalMove;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }

    // =========================
    // CHARGING
    // =========================
    void Charge()
    {
        ApplyMovement(lockedDirection * chargeSpeed);

        if (Vector3.Distance(transform.position, chargeTarget.position) < 0.8f)
        {
            SetNextTarget();
            StartCharge();
        }
    }

    // =========================
    // GRAB PLAYER
    // =========================
    void GrabPlayer(GameObject player)
    {
        if (grabbedPlayer != null) return;

        grabbedPlayer = player;
        grabbedPlayerController = player.GetComponent<CharacterController>();

        if (grabbedPlayerController != null)
            grabbedPlayerController.enabled = false;

        player.transform.SetParent(transform);

        currentState = AIState.Grab;
    }

    // =========================
    // DRAG PLAYER
    // =========================
    void DragPlayer()
    {
        ApplyMovement(lockedDirection * chargeSpeed);

        if (Vector3.Distance(transform.position, chargeTarget.position) < 0.8f)
        {
            ReleasePlayer();

            SetNextTarget();
            StartCharge();
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
    // ชนแล้วจับ
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