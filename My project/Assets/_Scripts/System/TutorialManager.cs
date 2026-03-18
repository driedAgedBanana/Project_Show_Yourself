using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    public AudioSource dialougeVoiceSource;
    public TextMeshProUGUI subtitleText;

    [Header("Target Tracking")]
    public int targetsHit = 0;
    public int targetGoal = 0;
    public DialougeSO nextDialogueAfterTargets;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayDialouge(DialougeSO dialogue)
    {
        StopAllCoroutines();

        // --- INSTANT ACTIONS (Safety/Freezing) ---
        PlayerController pc = PlayerController.Instance;

        // We freeze the player immediately so they have to listen
        if (dialogue.freezePlayer) pc.canMoveAtAll = false;

        // If we are locking things, do it now
        if (dialogue.lockEverything)
        {
            pc.primaryAuthorized = false;
            pc.sidearmAuthorized = false;
            pc.phoneManager.canOpenPhone = false;
        }

        // --- START SEQUENCE ---
        dialougeVoiceSource.clip = dialogue.voiceClip;
        dialougeVoiceSource.Play();

        // Pass the dialogue SO into the coroutine so it can unlock things at the end
        StartCoroutine(DisplaySubtitleSequence(dialogue));
    }

    private IEnumerator DisplaySubtitleSequence(DialougeSO dialogue)
    {
        // 1. Play all subtitles
        foreach (SubtitleTextLine line in dialogue.subtitleLines)
        {
            yield return new WaitForSeconds(line.startTime);
            subtitleText.text = line.text;
            yield return new WaitForSeconds(line.duration);
            subtitleText.text = "";
        }

        // 2. Wait for the actual audio clip to finish (in case subtitles are shorter than audio)
        while (dialougeVoiceSource.isPlaying)
        {
            yield return null;
        }

        // 3. --- DELAYED ACTIONS (Unlocks happen here) ---
        PlayerController pc = PlayerController.Instance;

        if (dialogue.unlockBasicMovement) pc.canMoveAtAll = true;
        if (dialogue.unlockSprint) pc.canSprintAuthorized = true;
        if (dialogue.unlockCrouch) pc.canCrouchAuthorized = true;
        if (dialogue.unlockLeaning) pc.canLeanAuthorized = true;

        if (dialogue.unlockPrimary) pc.primaryAuthorized = true;
        if (dialogue.unlockSidearm) pc.sidearmAuthorized = true;
        if (dialogue.unlockPhone) pc.phoneManager.canOpenPhone = true;

        // 4. Trigger the Target Practice goal AFTER the speech is done
        if (dialogue.requireTargetsHit > 0)
        {
            SetTargetGoal(dialogue.requireTargetsHit, dialogue.dialogueAfterGoalReached);
        }
    }

    public void SetTargetGoal(int goal, DialougeSO nextSpeech)
    {
        targetsHit = 0;
        targetGoal = goal;
        nextDialogueAfterTargets = nextSpeech;
    }

    public void OnTargetHit()
    {
        targetsHit++;

        if(targetsHit >= targetGoal && targetGoal > 0)
        {
            targetGoal = 0; // Reset the goal to prevent multiple triggers
            PlayDialouge(nextDialogueAfterTargets);
        }
    }
}
