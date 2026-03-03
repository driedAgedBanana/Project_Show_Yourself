using NUnit.Framework.Constraints;
using Pathfinding;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class EnemyStateBehaviour : MonoBehaviour
{
    public enum EnemyState
    {
        Patrol,
        Scream,
        Chase,
        GetHit,
        Investigate,
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

    [Space]
    public AudioList idle;
    public AudioList screamSound;
    public AudioList attack;
    public AudioList dead;

    private Vector3 _lastKnownLocation; // Last known location of the player for Investigate state
    private Coroutine _attackRoutine;

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
            AudioManager.Instance.PlaySounds(dead, transform.position);
            ChangeState(EnemyState.Dead);
            _movement.Stop();
            agent.maxSpeed = 0f;
            agent.destination = transform.position; // Stop movement immediately
            agent.SetPath(null);
            agent.canSearch = false;
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

            case EnemyState.GetHit:
                ChangeState(EnemyState.Chase);
                break;

            case EnemyState.Investigate:
                // 1. Tell the agent to keep moving to the noise location
                _movement.ChasingToShotLocation(_lastKnownLocation);

                // 2. Check: Did we arrive?
                // Using a small buffer (like 1.5f) is often more reliable than reachedDestination
                if (agent.reachedDestination || agent.remainingDistance < 1.5f)
                {
                    ChangeState(EnemyState.Patrol);
                }

                // 3. Check: Did we stumble upon the player?
                if (_vision.CanSeePlayer())
                {
                    ChangeState(EnemyState.Scream);
                }
                break;

            case EnemyState.Chase:
                _movement.ChasingPlayer(_vision.player);

                if (!_vision.CanSeePlayer())
                    ChangeState(EnemyState.Patrol);

                if (_attack.InAttackRange())
                    ChangeState(EnemyState.Attack);
                break;

            case EnemyState.Attack:

                if (!_attack.InAttackRange())
                    ChangeState(EnemyState.Chase);
                break;

            case EnemyState.Dead:
                agent.enabled = false;
                agent.canMove = false;
                seeker.enabled = false;
                _movement.Stop();
                break;
        }
    }

    private void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;

        if (_attackRoutine != null)
        {
            StopCoroutine(_attackRoutine);
            _attackRoutine = null;
        }

        // We still keep the attack bool, as that's a separate action from moving
        _movement.enemyAnimator.SetBool("isAttackingPlayer", false);

        currentState = newState;

        switch (newState)
        {
            case EnemyState.Scream:
                _screamTimer = screamDuration;
                _movement.Stop();
                // Ensure we look at the player during the scream
                transform.LookAt(new Vector3(_vision.player.position.x, transform.position.y, _vision.player.position.z));
                _movement.enemyAnimator.SetTrigger("isScreaming");
                AudioManager.Instance.PlaySounds(screamSound, transform.position);
                break;

            case EnemyState.Investigate:
                AudioManager.Instance.PlaySounds(screamSound, transform.position);
                _movement.ChasingToShotLocation(_lastKnownLocation);
                break;

            case EnemyState.Patrol:
                AudioManager.Instance.PlaySounds(idle, transform.position);
                break;

            case EnemyState.GetHit:
                _movement.enemyAnimator.SetTrigger("GetHit");
                break;

            case EnemyState.Attack:
                _movement.Stop();
                _attackRoutine = StartCoroutine(AttackCoroutine());
                break;
        }
    }

    public void TriggerGetHit()
    {
        if (currentState != EnemyState.Dead)
        {
            ChangeState(EnemyState.GetHit);
        }
    }

    private void OnEnable()
    {
        WeaponNoiseManager.OnNoiseMade += ListenForNoise;
    }

    private void OnDisable()
    {
        WeaponNoiseManager.OnNoiseMade -= ListenForNoise;
    }

    private void ListenForNoise(Vector3 noisePosition, float radius)
    {
        if (currentState == EnemyState.Dead) return;

        float distanceToNoise = Vector3.Distance(transform.position, noisePosition);

        // If the noise is within the sound's travel distance
        if (distanceToNoise <= radius)
        {
            // Only check if we aren't already chasing or attacking the player
            if (currentState != EnemyState.Chase && currentState != EnemyState.Attack)
            {
                _lastKnownLocation = noisePosition;
                ChangeState(EnemyState.Investigate);
            }
        }
    }

    private IEnumerator AttackCoroutine()
    {
        // The Update loop handles the state CHANGE, 
        // the Coroutine ONLY handles the attack ACTIONS.
        while (currentState == EnemyState.Attack)
        {
            _movement.Stop();
            Vector3 targetPos = new Vector3(_vision.player.position.x, transform.position.y, _vision.player.position.z);
            transform.LookAt(targetPos);

            int attackIndex = Random.Range(0, 3);
            _movement.enemyAnimator.SetInteger("attackIndex", attackIndex);
            _movement.enemyAnimator.SetBool("isAttackingPlayer", true);
            AudioManager.Instance.PlaySounds(attack, transform.position);

            // Wait for the next swing
            yield return new WaitForSeconds(_attack.atkInterval);
        }
    }
}
