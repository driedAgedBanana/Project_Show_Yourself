using UnityEngine;

public class MedKit : MonoBehaviour, IPlayerInteract
{
    public float minHealAmount = 15;
    public float maxHealAmount = 45;
    private float _healAmount;

    public void Interact()
    {
        _healAmount = Random.Range(minHealAmount, maxHealAmount + 1);

        if (PlayerController.Instance.playerHealth.currentHealth >= PlayerController.Instance.playerHealth.maxHealth)
        {
            return;
        }
        else
        {
            PlayerController.Instance.playerHealth.Heal(_healAmount);
            Destroy(gameObject);
        }
    }
}
