using UnityEngine;

[System.Serializable]
public class SubtitleTextLine
{
    [TextArea(2, 5)]
    public string text;
    public float startTime; // Time in seconds when this line should start displaying
    public float duration; // Duration in seconds for how long this line should be displayed
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

    [Header("Phone Unlock")]
    public bool unlockPhone;

    [Header("Objectives")]
    public int requireTargetsHit; // Set this to 3 or 5
    public DialougeSO dialogueAfterGoalReached;

    // Debug property to see how long the audio is in the inspector
    public float TotalAudioLength => voiceClip != null ? voiceClip.length : 0f;
}
