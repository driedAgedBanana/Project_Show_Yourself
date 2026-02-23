using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public enum EnemyState
    {
        Patrol,
        Chase,
        Attack,
        Control,
        Dead
    }

    public enum EnemyType
    {
        Undead,
        Feral
    }

    [SerializeField] private EnemyState _currentState;

    [Header("Basic setup")]
    public string enemyID; // For quest system
    [Space]
    public NavMeshAgent agent;
    public float targetSpeed;
    private float _distanceToPlayer;
    public EnemyType enemyType;
    public Rigidbody enemyRB;

    [Header("Health and ragdoll system")]
    public ParticleSystem disappearParticle;
    public SkinnedMeshRenderer enemyMesh;
    public RagdollController enemyRagdollController;
    public int maxHealth = 100;
    public int currentHealth;
    public GameObject bloodHitParticle;
    public BoxCollider bodyCollider;
    public Transform headBone;
    public BoxCollider headCollider;

    [Header("Loot after death")]
    public List<EnemiesDeadLoot> lootTable = new List<EnemiesDeadLoot>();

    [Header("Line of sight")]
    public GameObject player;
    public float visionDegree;
    [HideInInspector] public bool isDead = false;
    [SerializeField] private LayerMask visionBlockingLayers;


    [Header("Patrolling")]
    public float stopSafeDistance;
    public int maxWalkCount;
    private int _randomWalkCount;
    public Transform centerPoint;
    private float _walkTimeCount;
    private bool _isAllowedToWalk;
    private Vector3 _point;

    [Header("Chasing")]
    public Animator enemyAnimator;
    private float _currentChaseRange;
    public float normalChaseRange;
    public float _enragedChaseRange = 30f;
    public float loseSightRange;
    private bool _isScreaming = false;
    private bool _hasScreamed = false;
    public float screamDuration = 2f;

    private float _lastScreamTime;
    [SerializeField] private float screamCooldown = 10f;

    [Header("Attacking and Damage")]
    public float bufferDistance;
    public float attackRange;
    private bool _isPlayerInAttackZone = false;
    [SerializeField] private float _atkInterval = 2f;
    private Coroutine _attackCoroutine;
    private bool _isBeingAttacked = false;

    public int minDamageAmount = 10;
    public int maxDamageAmount = 15;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyAnimator = GetComponent<Animator>();

        // Disable agent auto-movement, use root motion instead
        agent.updatePosition = false;
        agent.updateRotation = false;

        agent.stoppingDistance = stopSafeDistance;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _isAllowedToWalk = true;
        currentHealth = maxHealth;
        // _currentState = EnemyState.Patrol;

        bodyCollider.enabled = true;
        headCollider.enabled = true;

        _currentChaseRange = normalChaseRange;

        enemyRB.GetComponent<Rigidbody>();
        enemyRB.mass = 2.5f;

        enemyRagdollController.GetComponent<RagdollController>();

        player = GameObject.FindGameObjectWithTag("Player"); // ?

        _distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        agent.stoppingDistance = attackRange - bufferDistance;

        if(disappearParticle != null)
        {
            disappearParticle.Stop();
        }

        // Debug.Log("Enemy will walk for " + maxWalkCount + " times!");
    }

    private void LateUpdate()
    {
        headCollider.transform.position = headBone.position;
        headCollider.transform.rotation = headBone.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return;

        agent.speed = targetSpeed;

        switch (_currentState)
        {
            case EnemyState.Patrol:
                Patrolling();
                CheckForPlayer();
                break;

            case EnemyState.Chase:
                CheckForAttack();
                ChasingPlayer();
                CheckForPlayer();
                break;

            case EnemyState.Control:
                // Reserved for future use
                break;

            case EnemyState.Attack:
                CheckForAttack();
                PlayerInAttackZone();
                break;

            case EnemyState.Dead:
                CheckForHealth();
                break;
        }
    }


    #region Checking for player
    private void CheckForPlayer()
    {
        if (_isPlayerInAttackZone)
        {
            _currentState = EnemyState.Attack;
            return;
        }

        Vector3 origin = transform.position + Vector3.up * 0.5f; // eye height
        Vector3 direction = (player.transform.position - origin).normalized;

        _distanceToPlayer = Vector3.Distance(origin, player.transform.position);

        // Angle check first (cheap)
        if (Vector3.Angle(transform.forward, direction) > visionDegree)
            return;

        // Distance check
        if (_distanceToPlayer > _currentChaseRange)
        {
            enemyAnimator.SetBool("isChasingPlayer", false);
            _currentState = EnemyState.Patrol;
            return;
        }

        // Line of sight check (IMPORTANT)
        if (Physics.Raycast(origin, direction, out RaycastHit hit, _currentChaseRange, visionBlockingLayers))
        {
            if (hit.transform.CompareTag("Player"))
            {
                enemyAnimator.SetBool("isChasingPlayer", true);
                _currentState = EnemyState.Chase;
            }
            else
            {
                // Hit wall or obstacle
                enemyAnimator.SetBool("isChasingPlayer", false);
                _currentState = EnemyState.Patrol;
            }

            Debug.DrawRay(origin, direction * _currentChaseRange, Color.red);
        }
    }
    #endregion

    #region Patrolling

    private void Patrolling()
    {
        _hasScreamed = false;
        if (!_isAllowedToWalk) return;

        if (agent == null || !agent.isOnNavMesh || isDead || !_isAllowedToWalk) return;

        // Only pick a new point if the agent is idle or reached the target
        if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance)
        {
            float range = Random.Range(6f, 20f);
            if (RandomPoint(centerPoint.position, range, out _point))
            {
                agent.SetDestination(_point);
                _walkTimeCount++;

                if (_walkTimeCount >= _randomWalkCount)
                {
                    _isAllowedToWalk = false;
                    StartCoroutine(WaitBeforePatrol());
                    return; // Exit so we don't process movement this frame
                }
            }
        }

        // Calculate velocity and set animator
        Vector3 velocity = agent.desiredVelocity;
        float speed = velocity.magnitude;
        enemyAnimator.SetFloat("Speed", speed);

        // Smooth rotation toward agent desired velocity
        if (velocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }
    }

    private void OnAnimatorMove()
    {
        if (enemyAnimator == null || agent == null) return;

        // Calculate desired movement from root motion
        Vector3 rootMotion = enemyAnimator.deltaPosition;
        Vector3 nextPosition = transform.position + rootMotion;

        // Make sure the agent stays on the NavMesh
        if (NavMesh.SamplePosition(nextPosition, out NavMeshHit hit, 0.3f, NavMesh.AllAreas))
        {
            // Move toward the sampled position, staying constrained to the NavMesh
            agent.nextPosition = hit.position;
            transform.position = agent.nextPosition;
        }
        else
        {
            // If we drift off the NavMesh, snap back to a safe position
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit safeHit, 1.0f, NavMesh.AllAreas))
            {
                agent.Warp(safeHit.position);
                transform.position = safeHit.position;
            }
        }

        // Apply animator’s root rotation smoothly
        transform.rotation *= enemyAnimator.deltaRotation;

        // Ensure Y height always matches the NavMesh
        Vector3 pos = transform.position;
        pos.y = agent.nextPosition.y;
        transform.position = pos;

        // Keep the NavMeshAgent and transform perfectly synced
        agent.nextPosition = transform.position;
    }

    private bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range; // Random point in a shpere
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    private IEnumerator WaitBeforePatrol()
    {
        float waitTime = Random.Range(3f, 10f);

        // Debug.Log("Enemy will be waiting for " + waitTime + " seconds!");
        yield return new WaitForSeconds(waitTime);

        _randomWalkCount = Random.Range(3, 10);
        maxWalkCount = _randomWalkCount;

        // Debug.Log("Enemy will walk for " + _randomWalkCount + " times!");

        _walkTimeCount = 0f;
        if (_walkTimeCount <= 0)
        {
            _isAllowedToWalk = true;
            Patrolling();
        }

    }

    #endregion

    #region Chasing

    private void TryScream()
    {
        if (_isScreaming || isDead) return;
        if (Time.time < _lastScreamTime + screamCooldown) return;

        _lastScreamTime = Time.time;
        StartCoroutine(ScreamThenRunTowardsPlayer());
    }

    private void ChasingPlayer()
    {
        if (isDead) return;

        Vector3 targetPos = player.transform.position;
        targetPos.y = transform.position.y;

        transform.LookAt(targetPos);

        // Only scream ONCE in its lifetime
        if (!_isScreaming && !_hasScreamed && !isDead)
        {
            if (_distanceToPlayer <= _currentChaseRange && _distanceToPlayer > attackRange)
            {
                TryScream();
                return;
            }
        }

        // Only chase if not screaming
        if (!_isScreaming)
        {
            agent.isStopped = false;
            if (!isDead && agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.SetDestination(player.transform.position);
            }

            Vector3 velocity = agent.desiredVelocity;
            float speed = velocity.magnitude;
            enemyAnimator.SetFloat("chaseSpeed", speed);

            if (velocity.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(velocity.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
            }
        }
    }

    private IEnumerator ScreamThenRunTowardsPlayer()
    {
        if (isDead || agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
        {
            yield break;
        }

        _isScreaming = true;
        agent.isStopped = true;

        enemyAnimator.SetTrigger("isScreaming 0");

        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }

        _isScreaming = false;
        _hasScreamed = false;
    }

    #endregion

    #region Checking for gun noises

    private void OnTriggerEnter(Collider other)
    {
        //if (other.CompareTag("WeaponNoise"))
        //{
        //    BecomeAggresive();
        //    StartCoroutine(EnragedVision());
        //    if (!_hasScreamed)
        //    {
        //        MoveTowardsMarker();
        //        _hasScreamed = true;
        //    }
        //}
    }

    private void MoveTowardsMarker()
    {
        if (player == null || isDead || agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return;
        _currentState = EnemyState.Chase;
        agent.isStopped = false;
        agent.SetDestination(player.transform.position);
    }

    #endregion

    #region Attacking

    private void CheckForAttack()
    {
        _distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (_distanceToPlayer <= attackRange)
        {
            if (_currentState != EnemyState.Attack)
            {
                agent.isStopped = true;
                agent.ResetPath();
                _currentState = EnemyState.Attack;
                _isPlayerInAttackZone = true;
            }
        }
        else
        {
            if (_currentState == EnemyState.Attack)
            {
                StopAttacking();
                if (_attackCoroutine != null)
                {
                    StopCoroutine(_attackCoroutine);
                    _attackCoroutine = null;
                }
            }

            _isPlayerInAttackZone = false;

            if (_distanceToPlayer <= _currentChaseRange)
            {
                _currentState = EnemyState.Chase;
                agent.isStopped = false;
            }
            else
            {
                _currentState = EnemyState.Patrol;
                agent.isStopped = false;
            }
        }
    }

    private void PlayerInAttackZone()
    {
        if (!_isPlayerInAttackZone) return;

        Vector3 targetPos = player.transform.position;
        targetPos.y = transform.position.y;

        transform.LookAt(targetPos);

        // Face the player
        Vector3 direction = (player.transform.position - transform.position).normalized;
        direction.y = 0;
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
        }

        if (_attackCoroutine == null)
        {
            _attackCoroutine = StartCoroutine(AttackRoutine());
        }
    }


    private IEnumerator AttackRoutine()
    {
        while (_isPlayerInAttackZone)
        {
            // Trigger attack animation
            int atkIndex = Random.Range(0, 3); // 0 to 2

            if (enemyAnimator != null)
            {
                enemyAnimator.SetBool("isAttackingPlayer", true);
                enemyAnimator.SetInteger("attackIndex", atkIndex);
            }

            // Wait for the interval before next attack
            yield return new WaitForSeconds(_atkInterval);
            //PlayerHeath.Instance.TakeDamage(damageAmount);
        }

        _attackCoroutine = null;
    }

    public void AttackPlayer()
    {
        if (PlayerController.Instance != null && _distanceToPlayer <= attackRange)
        {
            PlayerController.Instance.playerHealth.TakeDamage(Random.Range(minDamageAmount, maxDamageAmount));
        }
    }

    private void StopAttacking()
    {
        if (enemyAnimator != null)
        {
            agent.isStopped = false;
            enemyAnimator.SetBool("isAttackingPlayer", false);
        }
    }


    #endregion

    #region Health and death system
    private void CheckForHealth()
    {
        if (currentHealth <= 0)
        {
            _currentState = EnemyState.Dead;
            bodyCollider.enabled = false;
            headCollider.enabled = false;
        }
    }

    public void TakeDamage(int damageAmount, Vector3 hitPoint, Vector3 hitForce)
    {
        currentHealth -= damageAmount;
        _isBeingAttacked = true;
        enemyAnimator.SetTrigger("GetHit");

        if (_isBeingAttacked && _currentState != EnemyState.Dead)
        {
            BecomeAggresive();
            StartCoroutine(EnragedVision());

            // If never screamed yet, scream ONCE when attacked
            if (!_hasScreamed)
            {
                ChasingPlayer();
                _hasScreamed = true;
            }
        }

        if (bloodHitParticle != null)
        {
            GameObject bloodParticle = Instantiate(bloodHitParticle, hitPoint, Quaternion.LookRotation(hitPoint));
            Destroy(bloodParticle, 1f);
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die(hitPoint, hitForce);
        }
    }

    public void TakeHeadShot(int criticalDamageAmount, Vector3 hitPoint, Vector3 hitForce)
    {
        currentHealth -= criticalDamageAmount;
        Die(hitPoint, hitForce);

        if (bloodHitParticle != null)
        {
            GameObject bloodParticle = Instantiate(bloodHitParticle, hitPoint, Quaternion.LookRotation(hitPoint));
            Destroy(bloodParticle, 1f);
        }
    }

    public void BecomeAggresive()
    {
        if (player == null || !isDead || agent != null || agent.isActiveAndEnabled || agent.isOnNavMesh) return;

        _currentState = EnemyState.Chase;
        agent.isStopped = false;
        agent.SetDestination(player.transform.position);
    }

    private IEnumerator EnragedVision()
    {
        if (isDead) yield break;

        _currentChaseRange = _enragedChaseRange;
        yield return new WaitForSeconds(30f);
        _currentChaseRange = normalChaseRange;
        _isBeingAttacked = false;

    }


    public void Die(Vector3 hitPoint, Vector3 hitForce)
    {
        isDead = true;
        // SpawnManager.Instance.UnregisterEnemies(gameObject);
        if (_currentState == EnemyState.Dead) return;
        _currentState = EnemyState.Dead;

        StopAllCoroutines();

        enemyRB.mass = 0.001f;
        enemyRagdollController.SetRagdoll(true);
        bodyCollider.enabled = false;
        agent.enabled = false;
        enemyAnimator.enabled = false;

        Rigidbody nearestPart = null;
        float closestDistance = Mathf.Infinity;

        foreach (Rigidbody rb in enemyRagdollController.ragdollRigidbodyParts)
        {
            float distance = Vector3.Distance(rb.worldCenterOfMass, hitPoint);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestPart = rb;
            }
        }

        if (nearestPart != null)
            nearestPart.AddForceAtPosition(hitForce, hitPoint, ForceMode.Impulse);

        //if (QuestSystemManager.Instance != null)
        //{
        //    QuestSystemManager.Instance.OnEnemyKilled(enemyID);
        //}

        CalculateDropLoot();

        StartCoroutine(HideBody());
    }

    private IEnumerator HideBody()
    {
        yield return new WaitForSeconds(2f);
        enemyMesh.enabled = false;
        ParticleSystem instance = Instantiate(disappearParticle, transform.position, transform.rotation);
        instance.Play();
        Destroy(instance.gameObject, 1.5f);
        Destroy(gameObject, 0.5f);
    }

    public void DieImmediately()
    {
        isDead = true;
        // SpawnManager.Instance.UnregisterEnemies(gameObject);
        if (_currentState == EnemyState.Dead) return;
        _currentState = EnemyState.Dead;

        StopAllCoroutines();

        enemyRB.mass = 0.1f;
        enemyRagdollController.SetRagdoll(true);
        bodyCollider.enabled = false;
        agent.enabled = false;
        enemyAnimator.enabled = false;

        CalculateDropLoot();
    }


    //private void OnDestroy()
    //{
    //    if (SpawnManager.Instance != null)
    //        SpawnManager.Instance.UnregisterEnemies(gameObject);
    //}

    #endregion

    #region Calculate random chance for looting
    
    private void CalculateDropLoot()
    {
        foreach(EnemiesDeadLoot loot in lootTable)
        {
            if(Random.Range(0f, 100f) <= loot.dropChance)
            {
                InstantiateLoot(loot.itemPrefab);
            }
        }
    }

    private void InstantiateLoot(GameObject lootPrefab)
    {
        Instantiate(lootPrefab, transform.position, Quaternion.identity);
    }

    #endregion
}
