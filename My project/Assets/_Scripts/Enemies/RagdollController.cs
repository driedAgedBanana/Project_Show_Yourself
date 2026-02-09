using UnityEngine;

public class RagdollController : MonoBehaviour
{
    public Rigidbody[] ragdollRigidbodyParts;
    private Animator _zombieAnimator;

    private void Awake()
    {
        ragdollRigidbodyParts = GetComponentsInChildren<Rigidbody>();
        _zombieAnimator = GetComponent<Animator>();

        SetRagdoll(false);
    }


    public void SetRagdoll(bool active)
    {
        foreach (Rigidbody rb in ragdollRigidbodyParts)
        {
            if(rb.gameObject != this.gameObject)
            {
                rb.isKinematic = !active;
                rb.detectCollisions = active;
            }
        }

        _zombieAnimator.enabled = !active;
    }
}
