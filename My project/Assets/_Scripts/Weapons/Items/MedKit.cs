using UnityEngine;

public class MedKit : MonoBehaviour, IPlayerInteract
{
    public float minHealAmount = 15;
    public float maxHealAmount = 45;
    private float _healAmount;

    public AudioList healingSFX;
    public AudioList denySFX;

    public void Interact()
    {
        _healAmount = Random.Range(minHealAmount, maxHealAmount + 1);

        if (PlayerController.Instance.playerHealth.currentHealth >= PlayerController.Instance.playerHealth.maxHealth)
        {
            AudioManager.Instance.PlaySounds(denySFX, transform.position);
            return;
        }
        else
        {
            PlayerController.Instance.playerHealth.Heal(_healAmount);
            AudioManager.Instance.PlaySounds(healingSFX, transform.position);
            Destroy(gameObject);
        }
    }
}
