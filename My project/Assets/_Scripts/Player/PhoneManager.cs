using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PhoneManager : MonoBehaviour
{
    public GameObject phone;
    public Transform pointA;
    public Transform pointB;
    public float moveSpeed = 10f;

    [HideInInspector] public bool isPhoneActive = false;
    private Coroutine _movementCoroutine;
    public AudioList phoneSFX;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(phone != null)
        {
            phone.transform.position = pointB.position;
            phone.SetActive(false);
        }
    }

    public void OnTogglePhone(InputAction.CallbackContext ctx)
    {
        // Only trigger on the initial press
        if (ctx.performed)
        {
            isPhoneActive = !isPhoneActive;

            phone.SetActive(true);

            Vector3 targetPosition = isPhoneActive ? pointA.position : pointB.position;
            AudioManager.Instance.PlaySounds(phoneSFX, transform.position);


            if (_movementCoroutine != null) StopCoroutine(_movementCoroutine);
            _movementCoroutine = StartCoroutine(MovePhone(targetPosition));
        }
    }

    public void OnQuitPhone(InputAction.CallbackContext ctx)
    {
        // Only trigger if the phone is actually active/visible
        if (ctx.performed && isPhoneActive)
        {
            GameManager.Instance.HideMouse();

            Vector3 targetPosition = pointB.position;
            AudioManager.Instance.PlaySounds(phoneSFX, transform.position);

            if (_movementCoroutine != null) StopCoroutine(_movementCoroutine);
            _movementCoroutine = StartCoroutine(MovePhone(targetPosition));
            isPhoneActive = false;
        }
    }

    private IEnumerator MovePhone(Vector3 target)
    {
        while (Vector3.Distance(phone.transform.position, target) > 0.01f)
        {
            phone.transform.position = Vector3.MoveTowards(phone.transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
        phone.transform.position = target;

        // Disable after moving to point B
        if (!isPhoneActive)
        {
            phone.SetActive(false);
        }
    }
}
