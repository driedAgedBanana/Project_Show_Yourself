using Pathfinding;
using UnityEngine;

public class EnemyStateBehaviour : MonoBehaviour
{
    public enum EnemyState
    {
        Patrol,
        Scream,
        Chase,
        Attack,
        Dead
    }

    public EnemyState currentState { get; private set; }

    private EnemyMovement _movement;
    private EnemyVision _vision;
    private EnemyAttack _attack;
    private EnemyHealth _health;

    [Space]

    public AIPath agent;
    public Seeker seeker;

    private float _screamTimer = 0f;
    public float screamDuration = 1.5f;

    private void Awake()
    {
        _movement = GetComponent<EnemyMovement>();
        _vision = GetComponent<EnemyVision>();
        _attack = GetComponent<EnemyAttack>();
        _health = GetComponent<EnemyHealth>();
    }

    private void Start()
    {
        agent = GetComponent<AIPath>();
        seeker = GetComponent<Seeker>();

        agent.enabled = true;
        seeker.enabled = true;
    }

    private void Update()
    {
        if (_health.isDead)
        {
            ChangeState(EnemyState.Dead);
            agent.enabled = false;
            seeker.enabled = false;
            return;
        }

        switch (currentState)
        {
            case EnemyState.Patrol:
                _movement.Patrol();

                if (_vision.CanSeePlayer())
                    ChangeState(EnemyState.Scream);
                break;

            case EnemyState.Scream:
                _movement.Stop();
                _screamTimer -= Time.deltaTime;

                if (_screamTimer <= 0f)
                    ChangeState(EnemyState.Chase);
                break;

            case EnemyState.Chase:
                _movement.ChasingPlayer(_vision.player);

                if (!_vision.CanSeePlayer())
                    ChangeState(EnemyState.Patrol);

                if (_attack.InAttackRange())
                    ChangeState(EnemyState.Attack);
                break;

            case EnemyState.Attack:
                _attack.Attack();

                if (!_attack.InAttackRange())
                    ChangeState(EnemyState.Chase);
                break;

            case EnemyState.Dead:
                _movement.Stop();
                break;
        }
    }

    private void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        switch (newState)
        {
            case EnemyState.Scream:
                _screamTimer = screamDuration;
                _movement.Stop();
                _movement.patrolCenter.transform.LookAt(target: _vision.player);
                _movement.enemyAnimator.SetTrigger("isScreaming");
                break;

            case EnemyState.Chase:
                _movement.enemyAnimator.SetBool("isChasingPlayer", true);
                break;

            case EnemyState.Patrol:
                _movement.enemyAnimator.SetBool("isChasingPlayer", false);
                break;
        }
    }
}
