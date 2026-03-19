using UnityEngine;

public class TutorialInteract : MonoBehaviour, IPlayerInteract
{
    public DialougeSO dialougeToPlay;

    [Header("Settings")]
    public bool playOnce = true;
    private bool _hasPlayed = false;

    [Header("Gatekeeping")]
    public GameObject[] physicalBarriers; // Multiple if needed

    public void Interact()
    {
        if (playOnce && _hasPlayed) return;

        // Tell the Manager to play the SO
        // The Manager will then flip the switches (Lean, Move, Weapons) 
        // defined inside that specific SO.

        TutorialManager.Instance.PlayDialouge(dialougeToPlay);

        // Handle the physical barriers if there are any
        if (physicalBarriers != null)
        {
            foreach (GameObject barrier in physicalBarriers)
            {
                // Example: Open a door when the instructor starts talking
                barrier.SetActive(false);
            }
        }

        _hasPlayed = true;

        // Optional: Disable the trigger object entirely if playOnce is true
        if (playOnce) gameObject.SetActive(false);

        PlayerController pc = PlayerController.Instance;
    }
}
