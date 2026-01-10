using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PatrolAI : MonoBehaviour
{
    public Transform[] waypoints;
    public float moveSpeed = 2f;
    public float waitTimeAtPoint = 0.5f;

    private CharacterController controller;
    private int currentIndex = 0;
    private int direction = 1; // 1 = ไปข้างหน้า, -1 = ย้อนกลับ
    private float waitTimer;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        transform.position = waypoints[0].position;
    }

    void Update()
    {
        Patrol();
    }

    void Patrol()
    {
        if (waypoints.Length == 0) return;

        Vector3 target = waypoints[currentIndex].position;
        Vector3 moveDir = target - transform.position;
        moveDir.y = 0;

        if (moveDir.magnitude < 0.1f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtPoint)
            {
                NextPoint();
                waitTimer = 0f;
            }
            return;
        }

        controller.Move(moveDir.normalized * moveSpeed * Time.deltaTime);

        // หันหน้าไปทิศที่เดิน
        if (moveDir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(moveDir),
                Time.deltaTime * 5f
            );
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
}
