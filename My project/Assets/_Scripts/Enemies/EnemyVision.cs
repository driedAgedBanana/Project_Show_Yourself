using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class EnemyVision : MonoBehaviour
{
    public Transform player;
    public float visionDistance = 15f;
    public float visionDegree = 80f;
    public LayerMask blockingLayers;

    private void Start()
    {
        player = FindAnyObjectByType<PlayerController>()?.transform;
    }

    public bool CanSeePlayer()
    {
        if (player == null) return false;

        // 1. Check the distance
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        if (directionToPlayer.magnitude > visionDistance) return false;

        // 2. Check the angle with vision degree
        float angleBetween = Vector3.Angle(transform.forward, directionToPlayer.normalized);

        if (angleBetween < visionDegree / 2f)
        {
            // Throw in a raycast for Line of Sight (LoS)
            // Only fire if the player is within the vision cone
            if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, visionDistance, blockingLayers))
            {
                if (hit.collider.transform == player)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionDistance);

        Vector3 leftBoundary = Quaternion.Euler(0, -visionDegree / 2f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, visionDegree / 2f, 0) * transform.forward;

        Gizmos.DrawRay(transform.position, leftBoundary * visionDistance);
        Gizmos.DrawRay(transform.position, rightBoundary * visionDistance);
    }
}
