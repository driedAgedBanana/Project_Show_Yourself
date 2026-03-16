using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class WeaponSwapper : MonoBehaviour
{
    [Header("References")]
    public Transform weaponSlot;
    public GameObject mainWeapon;
    public GameObject secondWeapon;

    [Header("Settings")]
    public float smoothTime = 0.15f;
    public Vector3 hiddenOffset = new Vector3(0, -3f, 0);

    private Vector3 _targetPos;
    private Vector3 _velocity = Vector3.zero;
    [HideInInspector] public bool isMainWeaponActive = true;
    [HideInInspector] public bool isSwapping = false;

    [Header("SFX")]
    public AudioList swapSound;

    // Update is called once per frame
    void Update()
    {
        // Smoothly lerp the slot to the targetPos (either zero or hiddenOffset)
        weaponSlot.localPosition = Vector3.SmoothDamp(weaponSlot.localPosition, _targetPos, ref _velocity, smoothTime);
    }

    private IEnumerator SwapWeaponSequence()
    {
        isSwapping = true;

        // Drop the weapon
        _targetPos = hiddenOffset;

        AudioManager.Instance.PlaySounds(swapSound, transform.position);
        // Wait until the weapon is hidden
        yield return new WaitUntil(() => Vector3.Distance(weaponSlot.localPosition, _targetPos) < 0.1f);

        // Toggle logic
        isMainWeaponActive = !isMainWeaponActive;
        mainWeapon.SetActive(isMainWeaponActive);
        secondWeapon.SetActive(!isMainWeaponActive);

        // Bring the new weapon up
        _targetPos = Vector3.zero;

        // Wait until the new weapon is in position
        yield return new WaitUntil(() => Vector3.Distance(weaponSlot.localPosition, _targetPos) < 0.01f);
        isSwapping = false;
    }

    public void OnScroll(InputAction.CallbackContext ctx)
    {
        if (PlayerController.Instance.phoneManager.isPhoneActive) return;

        // Prevent swapping if we're already in the middle of a swap
        if (isSwapping || !ctx.performed) return;

        Vector2 scrollInput = ctx.ReadValue<Vector2>();

        if (Mathf.Abs(scrollInput.y) > 0.1f && !isSwapping)
        {
            StartCoroutine(SwapWeaponSequence());
        }
    }
}
