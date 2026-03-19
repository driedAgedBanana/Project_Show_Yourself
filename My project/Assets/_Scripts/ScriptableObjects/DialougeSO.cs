using UnityEngine;

[System.Serializable]
public class SubtitleTextLine
{
    [TextArea(2, 5)]
    public string text;
    public float startTime; // Time in seconds when this line should start displaying
    public float duration; // Duration in seconds for how long this line should be displayed
}

public enum TutorialRequirements
{
    None,
    Move,
    ShootTarget,
    InspectAmmo,
    Interact,
    SwitchWeapon,
    Lean,
    Sprint,
    OpenPhone
}

[CreateAssetMenu(fileName = "NewDialouge", menuName = "Dialouges/DialougeSO")]
public class DialougeSO : ScriptableObject
{
    public AudioClip voiceClip;
    // Add an array for multiple lines for one audio file
    public SubtitleTextLine[] subtitleLines;

    [Header("Tutorial Unlocks")]
    public bool unlockPrimary;
    public bool unlockSidearm;
    public bool lockEverything; // Useful for "Holster your weapons" segments

    [Header("Movement Unlocks")]
    public bool unlockBasicMovement;
    public bool unlockSprint;
    public bool unlockCrouch;
    public bool freezePlayer; // Use this for dramatic briefings

    [Header("Tactical Unlocks")]
    public bool unlockLeaning;

    [Header("Weapon Logic")]
    public bool unlockLiveFire; // If true, the player can finally shoot

    [Header("Phone Unlock")]
    public bool unlockPhone;

    [Header("Objectives")]
    public int requireTargetsHit; // Set this to 3 or 5
    public DialougeSO dialogueAfterGoalReached;

    [Header("Optional Task Pop-up")]
    public bool showTaskPanel;
    public string taskTitle;        
    [TextArea]
    public string taskInstruction;

    [Header("Completion Criteria")]
    public TutorialRequirements completionRequirement;
    public int targetsRequired = 0; // Only used if requirement is ShootTargets

    // Debug property to see how long the audio is in the inspector
    public float TotalAudioLength => voiceClip != null ? voiceClip.length : 0f;
}
