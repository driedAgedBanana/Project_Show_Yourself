using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public Transform player;
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    public int damage = 15;

    private float _lastAttackTime;
    private void Start()
    {
        player = FindAnyObjectByType<PlayerController>()?.transform;
    }

    public bool InAttackRange()
    {
        if (player == null) return false;
        float distance = Vector3.Distance(transform.position, player.position);
        return distance <= attackRange;
    }

    public void Attack()
    {
        if(Time.time < _lastAttackTime + attackCooldown) return;

        _lastAttackTime = Time.time;

        if(InAttackRange())
        {
            PlayerController.Instance.playerHealth.TakeDamage(damage);
        }
    }
}
