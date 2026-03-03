using Pathfinding;
using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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

    private void Awake()
    {
        agent = GetComponent<AIPath>();
        enemyAnimator = GetComponent<Animator>();

        agent.canMove = true;
        agent.updateRotation = false;

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

        Vector3 delta = enemyAnimator.deltaPosition;

        Vector3 desiredDir = agent.desiredVelocity;
        desiredDir.y = 0;

        if (desiredDir.sqrMagnitude < 0.001f)
            return;

        desiredDir.Normalize();

        // Project animation forward movement onto AI desired direction
        Vector3 projected = Vector3.Project(delta, desiredDir);

        // This moves both the transform and keeps AIPath synced
        agent.Move(projected);
    }

    public void DisableRootMotionMovement()
    {
        agent.updatePosition = false;
        agent.updateRotation = false;
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

    #region Chasing player

    public void ChasingPlayer(Transform target)
    {
        if (target == null) return;

        agent.isStopped = false;
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