using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [SerializeField] private Dictionary<string, TutorialObjects> _roomObjects = new Dictionary<string, TutorialObjects>();
    public AudioSource dialougeVoiceSource;
    public TextMeshProUGUI subtitleText;

    [Header("Target Tracking")]
    public int targetsHit = 0;
    public int targetGoal = 0;
    public DialougeSO nextDialogueAfterTargets;

    [Header("Task UI References")]
    public GameObject taskPanel; // The TutorialPanel from your hierarchy
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI instructionText;

    private bool _requirementMet = false;

    private DialougeSO _currentDialogue; // To keep track of what's playing


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            SkipStep();
        }
    }

    private void SkipStep()
    {
        if (_currentDialogue == null) return;

        Debug.Log("<color=cyan>Dev Skip: Jumping to next step.</color>");

        // 1. Find the next dialogue in the chain
        DialougeSO next = null;
        if (_currentDialogue.dialogueAfterGoalReached != null)
            next = _currentDialogue.dialogueAfterGoalReached;
        else if (nextDialogueAfterTargets != null)
            next = nextDialogueAfterTargets;

        // 2. Clear UI instantly
        subtitleText.text = "";
        taskPanel.SetActive(false);
        dialougeVoiceSource.Stop();

        // 3. Play next or unlock player if at the end
        if (next != null)
        {
            PlayDialouge(next);
        }
        else
        {
            // Safety: If there is nothing next, at least unfreeze the player
            PlayerController.Instance.canMoveAtAll = true;
            Debug.Log("No more dialogues in the chain.");
        }
    }

    public void PlayDialouge(DialougeSO dialogue)
    {
        PlayerController pc = PlayerController.Instance;

        ApplyImmediateEnvironment(dialogue);

        StopAllCoroutines();

        if (dialogue == null) return;
        _currentDialogue = dialogue; // Store the reference here

        // Reset all tracking bools so the player has to do the action AGAIN
        WeaponSwapper swapper = pc.GetComponentInChildren<WeaponSwapper>();
        if (swapper != null)
        {
            swapper.hasSwappedWeapon = false;
            swapper.mainWeapon.GetComponent<WeaponBase>().hasCheckedMagazine = false;
            swapper.secondWeapon.GetComponent<WeaponBase>().hasCheckedMagazine = false;
        }

        // We freeze the player immediately so they have to listen
        if (dialogue.freezePlayer) pc.canMoveAtAll = false;

        // If we are locking things, do it now
        if (dialogue.lockEverything)
        {
            pc.primaryAuthorized = false;
            pc.sidearmAuthorized = false;
            pc.phoneManager.canOpenPhone = false;
        }

        // FORCE the panel off if it was stuck from a previous failed requirement
        if (taskPanel.activeSelf)
        {
            StartCoroutine(FadePanel(1f, 0.4f));
            taskPanel.SetActive(false);
            Debug.Log("Forcing old panel closed for new dialogue.");
        }
        pc.playerInteract.hasInteracted = false;

        // --- START SEQUENCE ---
        dialougeVoiceSource.clip = dialogue.voiceClip;
        dialougeVoiceSource.Play();

        // Pass the dialogue SO into the coroutine so it can unlock things at the end
        StartCoroutine(DisplaySubtitleSequence(dialogue));
    }

    private IEnumerator DisplaySubtitleSequence(DialougeSO dialogue)
    {
        // 1. Play subtitles
        foreach (SubtitleTextLine line in dialogue.subtitleLines)
        {
            yield return new WaitForSeconds(line.startTime);
            subtitleText.text = line.text;
            yield return new WaitForSeconds(line.duration);
            subtitleText.text = "";
        }

        // 2. Wait for audio only if a clip exists
        if (dialougeVoiceSource.clip != null)
        {
            while (dialougeVoiceSource.isPlaying) yield return null;
        }

        // 3. --- AUTHORIZE ACTIONS FIRST ---
        // We unlock the abilities NOW so the player CAN perform the requirement below.
        PlayerController pc = PlayerController.Instance;
        if (dialogue.unlockBasicMovement) pc.canMoveAtAll = true;
        if (dialogue.unlockSprint) pc.canSprintAuthorized = true;
        if (dialogue.unlockCrouch) pc.canCrouchAuthorized = true;
        if (dialogue.unlockLeaning) pc.canLeanAuthorized = true;
        if (dialogue.unlockPrimary) pc.primaryAuthorized = true;
        if (dialogue.unlockSidearm) pc.sidearmAuthorized = true;
        if (dialogue.unlockPhone) pc.phoneManager.canOpenPhone = true;

        // 4. --- SHOW TASK PANEL & WAIT FOR COMPLETION ---
        if (dialogue.showTaskPanel)
        {
            titleText.text = dialogue.taskTitle;
            instructionText.text = dialogue.taskInstruction;

            CanvasGroup cg = taskPanel.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 0;

            taskPanel.SetActive(true);
            yield return StartCoroutine(FadePanel(1f, 0.4f));

            _requirementMet = false;

            // This will now work for silent tasks because the player is "unfrozen" above
            yield return new WaitUntil(() => CheckRequirement(dialogue));

            yield return new WaitForSeconds(0.8f);

            yield return StartCoroutine(FadePanel(0f, 0.4f));
            taskPanel.SetActive(false);
        }

        // 5. TRIGGER TARGETS (For non-panel shooting tasks)
        if (dialogue.requireTargetsHit > 0)
        {
            SetTargetGoal(dialogue.requireTargetsHit, dialogue.dialogueAfterGoalReached);
        }

        if (dialogue.unlockLiveFire)
        {
            // Find the weapons and unlock their triggers
            WeaponBase[] weapons = pc.GetComponentsInChildren<WeaponBase>(true);
            foreach (WeaponBase w in weapons)
            {
                w.instructorTriggerAuth = true;
            }
        }

        if (dialogue.dialogueAfterGoalReached != null && dialogue.requireTargetsHit == 0)
        {
            Debug.Log("Task Complete. Moving to: " + dialogue.dialogueAfterGoalReached.name);
            PlayDialouge(dialogue.dialogueAfterGoalReached);
        }

        // 6. Action-Based Auto-Progression
        // This triggers after the WaitUntil requirement is met
        if (dialogue.dialogueAfterGoalReached != null && dialogue.completionRequirement != TutorialRequirements.None)
        {
            Debug.Log($"Action '{dialogue.completionRequirement}' Complete. Triggering: {dialogue.dialogueAfterGoalReached.name}");

            // A small delay makes the transition feel less "robotic"
            yield return new WaitForSeconds(0.5f);
            PlayDialouge(dialogue.dialogueAfterGoalReached);
        }
    }

    private bool CheckRequirement(DialougeSO dialogue)
    {
        PlayerController pc = PlayerController.Instance;
        WeaponSwapper swapper = pc.GetComponentInChildren<WeaponSwapper>();

        // Get the currently active weapon to check its magazine status
        WeaponBase currentWeapon = null;
        if (swapper != null)
        {
            currentWeapon = swapper.isMainWeaponActive ?
                swapper.mainWeapon.GetComponent<WeaponBase>() :
                swapper.secondWeapon.GetComponent<WeaponBase>();
        }

        switch (dialogue.completionRequirement)
        {
            case TutorialRequirements.Move:
                return pc.isMoving;

            case TutorialRequirements.ShootTarget:
                return targetsHit >= targetGoal;

            case TutorialRequirements.Sprint:
                // Check if the player is currently holding run and moving
                return pc.runHeld && pc.isMoving;

            case TutorialRequirements.Interact:
                bool interacted = pc.playerInteract.hasInteracted;
                if (interacted) Debug.Log("<color=green>TUTORIAL: Interaction detected by Manager!</color>");
                return interacted;

            case TutorialRequirements.Lean:
                // Checks if leanPivot is rotated away from center
                return Mathf.Abs(pc.leanPivot.localRotation.z) > 0.05f;

            case TutorialRequirements.LeanAndShoot:
                bool isLeaning = Mathf.Abs(pc.leanPivot.localRotation.z) > 0.05f;
                return isLeaning && targetsHit >= targetGoal;

            case TutorialRequirements.InspectAmmo:
                if (currentWeapon == null) return false;
                return currentWeapon.hasCheckedMagazine;

            case TutorialRequirements.SwitchWeapon:
                return swapper != null && swapper.hasSwappedWeapon;

            case TutorialRequirements.OpenPhone:
                return pc.phoneManager.isPhoneActive;

            case TutorialRequirements.None:
                return true;

            default:
                return false;
        }
    }

    public void RegisterObject(TutorialObjects obj)
    {
        if (obj == null || string.IsNullOrEmpty(obj.objectID)) return;

        if (!_roomObjects.ContainsKey(obj.objectID))
        {
            _roomObjects.Add(obj.objectID, obj);
        }
        else
        {
            // Update the reference if the ID already exists (useful for scene reloads)
            _roomObjects[obj.objectID] = obj;
        }
    }

    private void ApplyImmediateEnvironment(DialougeSO dialogue)
    {
        foreach (string id in dialogue.objectsToDisable)
        {
            if (_roomObjects.TryGetValue(id, out TutorialObjects obj))
                obj.SetState(false);
        }

        foreach (string id in dialogue.objectsToEnable)
        {
            if (_roomObjects.TryGetValue(id, out TutorialObjects obj))
                obj.SetState(true);
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

        if (targetsHit >= targetGoal && targetGoal > 0)
        {
            targetGoal = 0; // Reset the goal to prevent multiple triggers
            PlayDialouge(nextDialogueAfterTargets);
        }
    }

    private IEnumerator FadePanel(float targetAlpha, float duration)
    {
        CanvasGroup cg = taskPanel.GetComponent<CanvasGroup>();
        if (cg == null) yield break;

        float startAlpha = cg.alpha;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }
        cg.alpha = targetAlpha;
    }
}
