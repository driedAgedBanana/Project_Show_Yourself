using UnityEngine;

public class TutorialObjects : MonoBehaviour
{
    public string objectID; // Set this in the Inspector (e.g., "RangeGate")

    private void OnEnable()
    {
        // Register this object with the Manager as soon as it exists
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.RegisterObject(this);
    }

    public void SetState(bool active)
    {
        gameObject.SetActive(active);
    }
}
