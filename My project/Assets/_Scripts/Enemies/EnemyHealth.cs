using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public GameObject bloodHitParticle;
    public ParticleSystem disappearParticle;
    public SkinnedMeshRenderer enemyRenderer;
    public RagdollController ragdollController;
    public Rigidbody rb;

    public BoxCollider headCollider;
    public BoxCollider bodyCollider;
    public Transform headBone;

    public static event Action OnEnemyKilled;


    public List<EnemiesDeadLoot> lootTable = new List<EnemiesDeadLoot>();

    private int _currentHealth;
    public bool isDead { get; private set; }


    private void Start()
    {
        _currentHealth = maxHealth;
        ragdollController = GetComponent<RagdollController>();
        rb = GetComponent<Rigidbody>();

        if (bodyCollider != null)
            bodyCollider.enabled = true;

        rb.isKinematic = true;

        ragdollController.SetRagdoll(false);

        if(disappearParticle != null)
        {
            disappearParticle.Stop();
        }

        if(enemyRenderer != null)
        {
            enemyRenderer.enabled = true;
        }
    }

    private void FixedUpdate()
    {
        headCollider.transform.position = headBone.position;
        headCollider.transform.rotation = headBone.rotation;
    }

    public void TakeDamage(int damage, Vector3 hitPoint, Vector3 hitForce)
    {
        if (isDead) return;
        _currentHealth -= damage;

        GetComponent<EnemyStateBehaviour>().TriggerGetHit();

        if (bloodHitParticle != null)
        {
            GameObject bloodParticle = Instantiate(bloodHitParticle, hitPoint, Quaternion.LookRotation(hitPoint));
            Destroy(bloodParticle, 1f);
        }

        if (_currentHealth <= 0)
        {
            Die(hitPoint, hitForce);
        }
    }

    private void Die(Vector3 hitPoint, Vector3 hitForce)
    {
        isDead = true;
        rb.isKinematic = false;
        bodyCollider.enabled = false;

        EnemyStateBehaviour behaviour = GetComponent<EnemyStateBehaviour>();

        AudioManager.Instance.PlaySounds(behaviour.dead, transform.position);

        if (bloodHitParticle != null)
        {
            GameObject bloodParticle = Instantiate(bloodHitParticle, hitPoint, Quaternion.LookRotation(hitPoint));
            Destroy(bloodParticle, 1f);
        }

        OnEnemyKilled?.Invoke();

        ragdollController.SetRagdoll(true);
        StartCoroutine(HideBody());

        CalculateDropLoot();
    }

    private IEnumerator HideBody()
    {
        // 1. Capture the exact position and rotation where the mesh is RIGHT NOW
        Vector3 finalPos = enemyRenderer.transform.position;
        Quaternion finalRot = enemyRenderer.transform.rotation;

        // 2. Wait for the ragdoll to settle or the animation to play
        yield return new WaitForSeconds(2f);

        // 3. Hide the body
        if (enemyRenderer != null)
            enemyRenderer.enabled = false;

        // 4. Spawn the disappear effect at the captured location
        if (disappearParticle != null)
        {
            ParticleSystem instance = Instantiate(disappearParticle, finalPos, finalRot);
            instance.Play();
            Destroy(instance.gameObject, 1.5f);
        }

        // 5. Cleanup the enemy root object
        Destroy(gameObject, 0.5f);
    }

    #region Loot Drop

    private void CalculateDropLoot()
    {
        foreach(EnemiesDeadLoot loot in lootTable)
        {
            float roll = UnityEngine.Random.Range(0f, 100f);
            if (roll <= loot.dropChance)
            {
                InstantiateLoot(loot.itemPrefab);
            }
        }
    }

    private void InstantiateLoot(GameObject lootPrefab)
    {
        Vector3 spawnPosition = transform.position + Vector3.up * 0.7f; // Spawn loot slightly above the enemy's position
        Instantiate(lootPrefab, spawnPosition, Quaternion.identity);
    }

    #endregion
}
