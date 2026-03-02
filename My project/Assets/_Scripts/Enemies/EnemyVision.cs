using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    public Transform player;
    public float visionDistance = 15f;
    public LayerMask blockingLayers;

    private void Start()
    {
        player = FindAnyObjectByType<PlayerController>()?.transform;
    }

    public bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 direction = (player.position - transform.position).normalized;

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, visionDistance, blockingLayers))
        {
            if (hit.transform == player)
            {
                return true;
            }
        }

        return false;
    }
}
