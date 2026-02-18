using UnityEngine;

public class TestInteraction : MonoBehaviour, IPlayerInteract
{
    public void Interact()
    {
        Debug.Log("Interacted with " + gameObject.name);
        Destroy(gameObject, 1f);
    }
}
