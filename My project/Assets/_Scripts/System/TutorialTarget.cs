using UnityEngine;

public class TutorialTarget : MonoBehaviour
{
    public bool isDestroyed = false;

    // This method is called by the WeaponBase via Raycast
    public void TakeDamage(int damage)
    {
        if (isDestroyed) return;

        // Logic for "Destroying" the target
        isDestroyed = true;

        // Notify the Manager that a target was hit
        TutorialManager.Instance.OnTargetHit();

        // Visuals (Optional: Play a sound or fall over)
        Destroy(gameObject);
    }
}
