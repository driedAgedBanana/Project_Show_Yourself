using UnityEngine;
using Pathfinding;
using System.Collections;

public class EnemyMovement : MonoBehaviour
{
    public Transform patrolCenter;
    public float patrolRadius = 10f;
    public Animator enemyAnimator;

    private AIPath agent;

    private int _walkCount = 0;
    private int _maxWalkCount;
    private bool _isAllowedToWalk = true;

    private void Awake()
    {
        agent = GetComponent<AIPath>();
        enemyAnimator = GetComponent<Animator>();

        agent.canMove = false;
        agent.updateRotation = true;
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
        if (!_isAllowedToWalk) return;

        agent.isStopped = false;

        if (!agent.hasPath || agent.reachedDestination)
        {
            if (TryGetRandomPointOnGraph(patrolCenter.position, patrolRadius, out Vector3 validPoint))
            {
                agent.destination = validPoint;
            }

            _walkCount++;

            if (_walkCount >= _maxWalkCount)
            {
                StartCoroutine(StopWalkingForSeconds());
            }
        }
    }

    private void UpdateAnimation()
    {
        Vector3 velocity = agent.desiredVelocity;

        // Remove the y component for animation purposes
        velocity.y = 0;

        float speed = velocity.magnitude;
        float normalizedSpeed = speed / agent.maxSpeed;

        enemyAnimator.SetFloat("Speed", normalizedSpeed);
    }

    private void OnAnimatorMove()
    {
        if (agent == null) return;

        transform.position += enemyAnimator.deltaPosition;

        agent.MovementUpdate(Time.deltaTime, out Vector3 nextPosition, out Quaternion nextRotation);
        agent.FinalizeMovement(nextPosition, nextRotation);
    }

    private bool TryGetRandomPointOnGraph(Vector3 center, float radius, out Vector3 result)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * radius;
            randomPoint.y = center.y;

            NNInfo nearest = AstarPath.active.GetNearest(randomPoint, NNConstraint.Default);

            if (nearest.node != null && nearest.node.Walkable)
            {
                result = (Vector3)nearest.node.position;
                return true;
            }
        }

        result = center;
        return false;
    }

    private IEnumerator StopWalkingForSeconds()
    {
        _isAllowedToWalk = false;

        yield return new WaitForSeconds(Random.Range(3f, 10f));

        _walkCount = 0;
        _maxWalkCount = Random.Range(3, 10);
        _isAllowedToWalk = true;
    }

    public void ChasingPlayer(Transform target)
    {
        if (target == null) return;

        agent.isStopped = false;
        agent.destination = target.position;
    }

    public void Stop()
    {
        agent.isStopped = true;
    }
}