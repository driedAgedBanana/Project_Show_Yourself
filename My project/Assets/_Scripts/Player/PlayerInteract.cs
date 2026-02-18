using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    public float interactRange = 3f;
    public GameObject interactIcon;

    private Camera _mainCamera;
    private bool _isInteracting = false;
    private bool _canInteract = false;

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
        RaycastHit hit;
        Ray ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.red);

        if (Physics.Raycast(ray, out hit, interactRange))
        {
            GameObject hitObject = hit.collider.gameObject;

            if (hitObject.TryGetComponent(out IPlayerInteract interactable))
            {
                interactIcon.SetActive(true);

                if (_isInteracting)
                {
                    interactable.Interact();
                    _isInteracting = false;
                }
                return;
            }
        }

        interactIcon.SetActive(false);
        _isInteracting = false;
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            _isInteracting = true;
        }
        StartCoroutine(InteractionCoolDown(0.3f));
    }

    private IEnumerator InteractionCoolDown(float time)
    {
        _canInteract = false;
        yield return new WaitForSeconds(time);
        _canInteract = true;
    }
}
