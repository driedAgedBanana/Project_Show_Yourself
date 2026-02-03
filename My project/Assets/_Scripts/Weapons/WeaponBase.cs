using UnityEngine;
using UnityEngine.InputSystem;

public enum weaponType
{
    Pistol,
    Rifle
}

public class WeaponBase : MonoBehaviour, IWeapon
{
    [HideInInspector] public bool isAiming;
    public bool IsAiming => isAiming;
    public Transform WeaponTransform => transform;
    public GameObject weaponItSelf;

    [Header("Aiming")]
    public Camera mainCam;
    public GameObject crossHair;
    public GameObject scopeCorssHair;
    public Transform weaponTransform;
    public Transform defaultPosition;
    public Transform aimingPosition;
    public float aimingSpeed = 5f;
    public float aimTime;

    [Space]
    // FOV when ADS
    public int zoomInFOV;
    public int defaultFOV;

    public float fovSmoothTime = 0.1f; // How long the transition takes
    private float _fovVelocity = 0f;    // This MUST be private and only used by SmoothDamp



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        crossHair.SetActive(true);
        scopeCorssHair.SetActive(false);

        mainCam = PlayerController.Instance.playerCam;

    }

    // Update is called once per frame
    void Update()
    {
        Aiming();
    }

    public void Aiming()
    {
        if(true) // Replace 'true' with WeaponManager in the future
        {
            Transform targetPosition = isAiming ? aimingPosition : defaultPosition;
            float targetFOV = isAiming ? zoomInFOV : defaultFOV;

            crossHair.SetActive(!isAiming);
            scopeCorssHair.SetActive(isAiming);

            // Smoothly transition aiming time between 0 and 1
            aimTime = Mathf.Clamp01(aimTime + Time.deltaTime * aimingSpeed * (isAiming ? 1 : -1));

            // Lerp the weapon's position and rotation smoothly between default and aiming positions
            weaponTransform.position = Vector3.Lerp(defaultPosition.position, aimingPosition.position, aimTime);
            weaponTransform.rotation = Quaternion.Slerp(defaultPosition.rotation, aimingPosition.rotation, aimTime);

            // Camera POV transistion between aiming or not
            float currentFOV = mainCam.fieldOfView;
            mainCam.fieldOfView = Mathf.SmoothDamp(currentFOV, targetFOV, ref _fovVelocity, fovSmoothTime);
        }
        else
        {
            isAiming = false;
            float currentFOV = mainCam.fieldOfView;
            mainCam.fieldOfView = Mathf.SmoothDamp(currentFOV, defaultFOV, ref _fovVelocity, fovSmoothTime);
        }
    }

    #region Inputs

    public void OnAim(InputAction.CallbackContext ctx)
    {
        isAiming = ctx.ReadValue<float>() > 0;
    }

    #endregion
}

