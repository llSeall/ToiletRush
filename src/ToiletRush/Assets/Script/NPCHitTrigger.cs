using UnityEngine;

public class NPCHitTrigger : MonoBehaviour
{
    public NPCPatrolReduceStamina npc;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        npc.OnHitPlayer(other);
        Debug.Log("NPC HIT PLAYER");

    }
}
