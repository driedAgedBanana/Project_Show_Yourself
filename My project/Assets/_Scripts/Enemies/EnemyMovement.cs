using Pathfinding;
using System.Collections;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public Transform patrolCenter;
    public float patrolRadius = 10f;
    public Animator enemyAnimator;

    private AIPath agent;

    private int _walkCount = 0;
    private int _maxWalkCount;
    private bool _isAllowedToWalk = true;
    private bool _isScreaming = false;

    [Space]
    public float walkSpeed = 1.5f;
    public float runSpeed = 4f;

    private void Awake()
    {
        agent = GetComponent<AIPath>();
        enemyAnimator = GetComponent<Animator>();

        // IMPORTANT: Let AIPath handle the movement
        agent.canMove = true;

        // Let AIPath handle rotation, or set to false if you use a custom script to rotate
        agent.updateRotation = true;

        // Explicitly disable Root Motion in code to be safe
        enemyAnimator.applyRootMotion = false;
    }

    private void Start()
    {
        if (patrolCenter == null)
            patrolCenter = transform;

        _maxWalkCount = Random.Range(3, 10);
    }

    private void Update()
    {
        UpdateAnimation();
    }

    public void Patrol()
    {
        if (!_isAllowedToWalk || _isScreaming) return;

        agent.isStopped = false;
        agent.maxSpeed = walkSpeed;

        // Check if we reached destination or don't have one
        if (!agent.pathPending && (agent.reachedDestination || !agent.hasPath))
        {
            if (TryGetRandomPointOnGraph(patrolCenter.position, patrolRadius, out Vector3 validPoint))
            {
                agent.destination = validPoint;
                _walkCount++;
            }

            if (_walkCount >= _maxWalkCount)
            {
                StartCoroutine(StopWalkingForSeconds());
            }
        }
    }

    private void UpdateAnimation()
    {
        // Just send the raw speed. 
        // 0 = Idle, 1.5 = Walk animation, 4 = Run animation.
        float currentSpeed = agent.velocity.magnitude;
        enemyAnimator.SetFloat("Speed", currentSpeed, 0.1f, Time.deltaTime);
    }


    private bool TryGetRandomPointOnGraph(Vector3 center, float radius, out Vector3 result)
    {
        // Note: A* Project has a built-in helper for this, but your method works well too!
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * radius;
            randomPoint.y = center.y;

            NNInfo nearest = AstarPath.active.GetNearest(randomPoint, NNConstraint.Default);

            if (nearest.node != null && nearest.node.Walkable)
            {
                result = nearest.position; // NNInfo.position is already a Vector3
                return true;
            }
        }

        result = center;
        return false;
    }

    private IEnumerator StopWalkingForSeconds()
    {
        _isAllowedToWalk = false;
        agent.isStopped = true; // Make sure the agent stops moving physically

        yield return new WaitForSeconds(Random.Range(3f, 10f));

        _walkCount = 0;
        _maxWalkCount = Random.Range(3, 10);
        _isAllowedToWalk = true;
        agent.isStopped = false;
    }

    #region Chasing player

    public void ChasingPlayer(Transform target)
    {
        if (target == null) return;
        agent.isStopped = false;
        agent.maxSpeed = runSpeed;
        agent.destination = target.position;
    }

    public void ChasingToShotLocation(Vector3 location)
    {
        agent.isStopped = false;
        agent.destination = location;
    }

    public void Stop()
    {
        agent.isStopped = true;
    }

    #endregion
}