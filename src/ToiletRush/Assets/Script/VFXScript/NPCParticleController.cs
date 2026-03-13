using UnityEngine;

public class NPCParticleController : MonoBehaviour
{
    public NPCPatrolTalkAI npcAI;
    public ParticleSystem particleFX;

    void Start()
    {
        if (particleFX != null)
            particleFX.gameObject.SetActive(false);
    }

    void Update()
    {
        if (npcAI == null || particleFX == null) return;

        if (npcAI.currentState == NPCPatrolTalkAI.State.Chase ||
            npcAI.currentState == NPCPatrolTalkAI.State.Talk)
        {
            if (!particleFX.gameObject.activeSelf)
                particleFX.gameObject.SetActive(true);
        }
        else
        {
            if (particleFX.gameObject.activeSelf)
                particleFX.gameObject.SetActive(false);
        }
    }
}