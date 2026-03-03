using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public GameObject bloodHitParticle;
    public RagdollController ragdollController;
    public Rigidbody rb;

    public BoxCollider headCollider;
    public Transform headBone;

    private int _currentHealth;
    public bool isDead { get; private set; }


    private void Start()
    {
        _currentHealth = maxHealth;
        ragdollController = GetComponent<RagdollController>();
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;

        ragdollController.SetRagdoll(false);
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

        if (bloodHitParticle != null)
        {
            GameObject bloodParticle = Instantiate(bloodHitParticle, hitPoint, Quaternion.LookRotation(hitPoint));
            Destroy(bloodParticle, 1f);
        }

        ragdollController.SetRagdoll(true);
        Destroy(gameObject, 7f);
    }
}
