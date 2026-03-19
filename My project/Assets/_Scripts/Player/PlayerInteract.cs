using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    public float interactRange = 3f;
    public GameObject interactIcon;

    private Camera _mainCamera;
    private bool _canInteract = true; // Default to true

    // Store the interface we found
    private IPlayerInteract _currentInteractable;

    [HideInInspector] public bool hasInteracted;

    private void Start()
    {
        if (interactIcon != null)
            interactIcon.SetActive(false);

        _mainCamera = PlayerController.Instance.playerCam;
    }

    private void FixedUpdate()
    {
        RaycastCheck();
    }

    public void RaycastCheck()
    {
        Ray ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange))
        {
            // Try to get the interface from what we hit
            if (hit.collider.TryGetComponent(out IPlayerInteract interactable))
            {
                _currentInteractable = interactable; // STORE IT
                interactIcon.SetActive(true);
                return;
            }
        }

        // If we hit nothing or something non-interactable
        _currentInteractable = null;
        interactIcon.SetActive(false);
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.started && _canInteract && _currentInteractable != null)
        {
            // 1. SET THE FLAG FIRST
            hasInteracted = true;
            Debug.Log("Tutorial: Interaction Flag Set to True");

            // 2. DO THE INTERACTION
            _currentInteractable.Interact();

            StartCoroutine(InteractionCoolDown(0.2f));
        }
    }

    private IEnumerator InteractionCoolDown(float time)
    {
        _canInteract = false;
        yield return new WaitForSeconds(time);
        _canInteract = true;
    }
}