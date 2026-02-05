using System.Collections;
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

    [Header("Sway Settings")]
    public float swayClamp = 0.09f;
    public float smoothing = 3f;
    private Vector3 _origin;
    [Space]
    public float swayMultiplier;

    [Header("Bobbing Setting")]
    public float walkBobSpeed = 14f;
    public float walkBobAmount = 0.05f;
    public float sprintBobSpeed = 18f;
    public float sprintBobAmount = 0.1f;
    private Vector3 _bobOffset;
    private Quaternion _swayRotation = Quaternion.identity;


    private Vector3 _defaultPos;
    private float _timer;

    [Header("Aiming")]
    private Camera _mainCam;
    public GameObject crossHair;
    public GameObject scopeCorssHair;
    public Transform weaponRoot;
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

    [Header("Shooting and Damages")]
    public int damage;
    [Space]
    public weaponType currentWeaponType = weaponType.Rifle;
    public GameObject shootingPoint;
    public LineRenderer bulletTrail;
    public GameObject bulletHitImpact;
    public ParticleSystem muzzleFlash;
    [Space]
    public int range;
    public float impactForce;
    public bool isShooting = false;
    public float bulletSpread = 0.07f;
    public float aimingBulletSpread = 0.02f;
    [Space]
    public float fireRate = 0.1f;
    private bool _isShootingAuto;
    private Coroutine _shootAutoCoroutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        crossHair.SetActive(true);
        scopeCorssHair.SetActive(false);

        _mainCam = PlayerController.Instance.playerCam;

        _defaultPos = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        Aiming();

        if (isAiming)
        {
            _swayRotation = Quaternion.identity;
            return;
        }
        else
        {
            SwayWeapon();
            ApplyFinalTransform();
            WeaponBobbing();
        }

    }

    public void Aiming()
    {
        if (true) // Replace 'true' with WeaponManager in the future
        {
            Transform targetPosition = isAiming ? aimingPosition : defaultPosition;
            float targetFOV = isAiming ? zoomInFOV : defaultFOV;

            crossHair.SetActive(!isAiming);
            scopeCorssHair.SetActive(isAiming);

            // Smoothly transition aiming time between 0 and 1
            aimTime = Mathf.Clamp01(aimTime + Time.deltaTime * aimingSpeed * (isAiming ? 1 : -1));

            // Lerp the weapon's position and rotation smoothly between default and aiming positions
            weaponRoot.position = Vector3.Lerp(defaultPosition.position, aimingPosition.position, aimTime);
            weaponRoot.rotation = Quaternion.Slerp(defaultPosition.rotation, aimingPosition.rotation, aimTime);

            // Camera POV transistion between aiming or not
            float currentFOV = _mainCam.fieldOfView;
            _mainCam.fieldOfView = Mathf.SmoothDamp(currentFOV, targetFOV, ref _fovVelocity, fovSmoothTime);
        }
        else
        {
            isAiming = false;
            float currentFOV = _mainCam.fieldOfView;
            _mainCam.fieldOfView = Mathf.SmoothDamp(currentFOV, defaultFOV, ref _fovVelocity, fovSmoothTime);
        }
    }

    public void Shooting()
    {
        RaycastHit hit;
        Vector3 force = _mainCam.transform.forward * impactForce;

        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        // Applying recoil here later on

        // Random bullet spread
        Vector3 direction = _mainCam.transform.forward;

        if (!isAiming)
        {
            direction += _mainCam.transform.right * Random.Range(-bulletSpread, bulletSpread);
            direction += _mainCam.transform.up * Random.Range(-bulletSpread, bulletSpread);
        }
        else
        {
            direction += _mainCam.transform.right * Random.Range(-aimingBulletSpread, aimingBulletSpread);
            direction += _mainCam.transform.up * Random.Range(-aimingBulletSpread, aimingBulletSpread);
        }

        // Sounds later on

        if (Physics.Raycast(_mainCam.transform.position, direction, out hit, range))
        {
            Debug.Log("Hit: " + hit.collider.name);

            // Spawn bullet trail
            StartCoroutine(SpawnBulletLine(shootingPoint.transform.position, hit.point));

            // Spawn hit impact effect
            GameObject bulletImpact = Instantiate(bulletHitImpact, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(bulletImpact, 0.5f);
        }
        else
        {
            // No hit draw tracer into distance
            Vector3 endPoint = shootingPoint.transform.position + direction * range;
            StartCoroutine(SpawnBulletLine(shootingPoint.transform.position, endPoint));
        }
    }

    private IEnumerator ShootAuto()
    {
        while (_isShootingAuto)
        {
            Shooting();
            yield return new WaitForSeconds(fireRate);
        }
    }

    private IEnumerator SpawnBulletLine(Vector3 startPoint, Vector3 hitTarget)
    {
        bulletTrail.positionCount = 2;
        bulletTrail.SetPosition(0, startPoint);
        bulletTrail.SetPosition(1, hitTarget);
        yield return new WaitForSeconds(0.01f);
        bulletTrail.positionCount = 0;
    }

    private void ApplyFinalTransform()
    {
        // Position (aiming handled by weaponRoot, bob is additive)
        transform.localPosition = _defaultPos + _bobOffset;

        // Rotation (sway additive on top of current local rotation)
        transform.localRotation = _swayRotation;
    }


    private void SwayWeapon()
    {
        float mouseX = PlayerController.Instance.lookInput.x * swayMultiplier;
        float mouseY = PlayerController.Instance.lookInput.y * swayMultiplier;

        mouseX = Mathf.Clamp(mouseX, -swayClamp, swayClamp);
        mouseY = Mathf.Clamp(mouseY, -swayClamp, swayClamp);

        Quaternion rotX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotY = Quaternion.AngleAxis(mouseX, Vector3.up);

        Quaternion targetRot = rotX * rotY;
        _swayRotation = Quaternion.Slerp(_swayRotation, targetRot, smoothing * Time.deltaTime);
    }


    public void WeaponBobbing()
    {
        if (PlayerController.Instance.isMoving)
        {
            bool isSprinting = PlayerController.Instance.canSprint;
            float bobSpeed = isSprinting ? sprintBobSpeed : walkBobSpeed;
            float bobAmount = isSprinting ? sprintBobAmount : walkBobAmount;

            _timer += Time.deltaTime * bobSpeed;
            _bobOffset = new Vector3(0, Mathf.Sin(_timer) * bobAmount, 0);
        }
        else
        {
            _timer = 0f;
            _bobOffset = Vector3.Lerp(_bobOffset, Vector3.zero, Time.deltaTime * 8f);
        }
    }

    #region Inputs

    public void OnAim(InputAction.CallbackContext ctx)
    {
        isAiming = ctx.ReadValue<float>() > 0;
    }

    public void OnShoot(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            Shooting();
        }
    }

    public void OnShootAuto(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            _isShootingAuto = true;
            _shootAutoCoroutine = StartCoroutine(ShootAuto());
        }
        else if (ctx.canceled)
        {
            _isShootingAuto = false;
            if (_shootAutoCoroutine != null)
                StopCoroutine(_shootAutoCoroutine);
        }
    }

    #endregion
}

