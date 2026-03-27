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

    private CharacterController controller;
    private AIState currentState;

    private Transform chargeTarget;
    private Vector3 lockedDirection;

    private GameObject grabbedPlayer;
    private CharacterController grabbedPlayerController;

    void Start()
    {
        controller = GetComponent<CharacterController>();

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

        lockedDirection =
            (chargeTarget.position - transform.position).normalized;

        transform.forward = lockedDirection;
    }

    // =========================
    // CHARGING
    // =========================
    void Charge()
    {
        controller.Move(lockedDirection * chargeSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, chargeTarget.position) < 0.8f)
        {
            SetNextTarget();
            StartCharge(); // วิ่งต่อทันที
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
        controller.Move(lockedDirection * chargeSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, chargeTarget.position) < 0.8f)
        {
            ReleasePlayer();

            //  สำคัญ: เปลี่ยนเป้าหมายแล้ววิ่งต่อทันที
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