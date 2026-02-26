using JetBrains.Annotations;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public enum WeaponType
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
    [HideInInspector] public int defaultFOV;
    public float fovSmoothTime = 0.1f; // How long the transition takes
    private float _fovVelocity = 0f;    // This MUST be private and only used by SmoothDamp

    [Header("Shooting and Damages")]
    public int damage;
    [Space]
    public WeaponType currentWeaponType = WeaponType.Rifle;
    public GameObject shootingPoint;
    public LineRenderer bulletTrail;
    public GameObject bulletHitImpact;
    public ParticleSystem muzzleFlash;
    public ParticleSystem shellEjectParticle;
    public Transform shellEjectPoint;
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

    [Header("Ammunition and animations")]
    public WeaponsAmmoData ammoData;
    private int _currentAmmo;
    private int _maxAmmo;
    [HideInInspector] public int totalAmountOfCarryAmmo;
    private bool _isReloading;
    [Space]
    public Animator weaponsAnimator;
    public float reloadTime = 2f;
    private Vector3 _defaultLocalPos;
    private Quaternion _defaultLocalRot;

    [Header("Checking for ammunition")]
    [HideInInspector] public bool isCheckingForAmmo;

    [Header("UI")]
    public GameObject uiHolder;
    public TextMeshProUGUI currentAmountAmmo;
    public TextMeshProUGUI totalAmountAmmo;

    [Header("Audio")]
    public AudioList shootAudio;
    public AudioList reloadAudio;
    [Space]
    public AudioList openMagInspectionAudio;
    public AudioList closeMagInspectionAudio;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        crossHair.SetActive(true);
        scopeCorssHair.SetActive(false);

        _mainCam = PlayerController.Instance.playerCam;

        defaultFOV = PlayerController.Instance.normalFOV;

        _defaultPos = transform.localPosition;

        if (shellEjectParticle != null)
        {
            shellEjectParticle.Stop();
        }

        _defaultLocalPos = transform.localPosition;
        _defaultLocalRot = transform.localRotation;

        if (weaponsAnimator != null)
        {
            DisableAnimator();
        }

        _maxAmmo = ammoData.maxAmmo;
        _currentAmmo = _maxAmmo;
        totalAmountOfCarryAmmo = ammoData.totalAmountOfCarryAmmo;

        if(uiHolder != null)
        {
            uiHolder.SetActive(false);
        }

        UpdateAmmoUI();
    }

    // Update is called once per frame
    void Update()
    {
        if(PlayerController.Instance.playerHealth.isAlive)
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
    }

    private void OnDisable()
    {
        // Reset states so the weapon isn't "broken" when you switch back to it
        _isReloading = false;
        _isShootingAuto = false;
        isCheckingForAmmo = false;
        if (_shootAutoCoroutine != null) StopCoroutine(_shootAutoCoroutine);

        // Ensure the animator doesn't stay on
        DisableAnimator();
    }

    #region Weapon moving

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

    private void ApplyFinalTransform()
    {
        // Position (aiming handled by weaponRoot, bob is additive)
        transform.localPosition = _defaultPos + _bobOffset;

        // Rotation (sway additive on top of current local rotation)
        transform.localRotation = _swayRotation;
    }

    #endregion

    #region Aiming
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
    #endregion

    #region Shooting
    public void Shooting()
    {
        RaycastHit hit;
        Vector3 force = _mainCam.transform.forward * impactForce;

        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        // Applying recoil 
        PlayerController.Instance.cameraShakeManager.ApplyingRecoil();

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

        if (Physics.Raycast(_mainCam.transform.position, direction, out hit, range, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            // Spawn bullet trail
            StartCoroutine(SpawnBulletLine(shootingPoint.transform.position, hit.point));

            // print("Hit: " + hit.collider.name);

            // Spawn hit impact effect
            GameObject bulletImpact = Instantiate(bulletHitImpact, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(bulletImpact, 0.5f);

            // Deal damage on the enemy
            EnemyController enemy = hit.transform.GetComponentInParent<EnemyController>();
            if (enemy != null)
            {
                if (hit.collider.CompareTag("Head"))
                {
                    int criticalDamage = Random.Range(25, 250);
                    enemy.TakeHeadShot(criticalDamage, hit.point, force);
                }
                else
                {
                    enemy.TakeDamage(damage, hit.point, force);
                }

                Destroy(bulletImpact);
            }
        }
        else
        {
            // No hit draw tracer into distance
            Vector3 endPoint = shootingPoint.transform.position + direction * range;
            StartCoroutine(SpawnBulletLine(shootingPoint.transform.position, endPoint));
        }

        UpdateAmmoUI();
    }

    private IEnumerator ShootAuto()
    {
        while (_isShootingAuto)
        {
            // Add a check for isCheckingForAmmo here too
            if (_isReloading || _currentAmmo <= 0 || isCheckingForAmmo)
            {
                SetEjectionState(false);
                yield break;
            }

            _currentAmmo--;
            Shooting();
            AudioManager.Instance.PlaySounds(shootAudio, transform.position);

            yield return new WaitForSeconds(fireRate);
        }
        SetEjectionState(false);
    }

    private IEnumerator SpawnBulletLine(Vector3 startPoint, Vector3 hitTarget)
    {
        bulletTrail.positionCount = 2;
        bulletTrail.SetPosition(0, startPoint);
        bulletTrail.SetPosition(1, hitTarget);
        yield return new WaitForSeconds(0.01f);
        bulletTrail.positionCount = 0;
    }

    public void SetEjectionState(bool shouldBeOn)
    {
        // Shell ejection
        if (shouldBeOn)
        {
            shellEjectParticle.Play();
        }
        else
        {
            shellEjectParticle.Stop();
        }
    }
    #endregion

    #region Reloading
    public void Reloading()
    {
        if (_isReloading) return;
        SetEjectionState(false);
        _isShootingAuto = false;
        StartCoroutine(ReloadingAnimation());
        AudioManager.Instance.PlaySounds(reloadAudio, transform.position);
    }

    private IEnumerator ReloadingAnimation()
    {
        _isReloading = true;
        EnableAnimator();
        weaponsAnimator.SetBool("isReloading", true);

        yield return new WaitForSeconds(reloadTime);
    }

    public void FinishReloading()
    {
        weaponsAnimator.SetBool("isReloading", false);
        DisableAnimator();

        int missing = ammoData.maxAmmo - _currentAmmo;
        int toLoad = Mathf.Min(missing, totalAmountOfCarryAmmo);
        _currentAmmo += toLoad;
        totalAmountOfCarryAmmo -= toLoad;

        UpdateAmmoUI();

        _isReloading = false;
    }

    public void GainingAmmunition(int refillAmount)
    {
        totalAmountOfCarryAmmo = Mathf.Clamp(totalAmountOfCarryAmmo + refillAmount, 0, ammoData.totalAmountOfCarryAmmo);
    }

    public void EnableAnimator()
    {
        weaponsAnimator.enabled = true;
        isAiming = false;
    }

    public void DisableAnimator()
    {
        weaponsAnimator.enabled = false;
    }

    public void UpdateAmmoUI()
    {
        currentAmountAmmo.text = _currentAmmo.ToString();
        totalAmountAmmo.text = totalAmountOfCarryAmmo.ToString();
    }

    #endregion

    #region Check For Ammo

    private void PlayCheckMagAnimation()
    {
        isCheckingForAmmo = true;
        EnableAnimator();
        AudioManager.Instance.PlaySounds(openMagInspectionAudio, transform.position);
        weaponsAnimator.SetBool("isCheckingForAmmo", true);
        uiHolder.SetActive(true);
    }

    private void PlayCloseMagAnimation()
    {
        uiHolder.SetActive(false);
        weaponsAnimator.SetBool("isCheckingForAmmo", false);
        AudioManager.Instance.PlaySounds(closeMagInspectionAudio, transform.position);
    }

    public void FinishCheckingAmmo()
    {
        isCheckingForAmmo = false;
        DisableAnimator();
    }

    #endregion

    #region Inputs

    public void OnAim(InputAction.CallbackContext ctx)
    {
        if (!PlayerController.Instance.playerHealth.isAlive) return;
        if(isCheckingForAmmo) return;

        isAiming = ctx.ReadValue<float>() > 0;
    }

    public void OnShoot(InputAction.CallbackContext ctx)
    {
        if (!PlayerController.Instance.playerHealth.isAlive) return;
        if (isCheckingForAmmo) return;

        if (currentWeaponType != WeaponType.Pistol || !gameObject.activeSelf || _isReloading) return;

        if (ctx.started && _currentAmmo > 0)
        {
            _currentAmmo--;
            Shooting();
            SetEjectionState(true);
            AudioManager.Instance.PlaySounds(shootAudio, transform.position);
        }

        if (_currentAmmo == 0 && totalAmountOfCarryAmmo > 0)
        {
            Reloading();
        }
    }

    public void OnShootAuto(InputAction.CallbackContext ctx)
    {
        if (!PlayerController.Instance.playerHealth.isAlive || isCheckingForAmmo || _isReloading) return;

        if (ctx.started)
        {
            _isShootingAuto = true;
            if (_shootAutoCoroutine != null) StopCoroutine(_shootAutoCoroutine); // Safety
            _shootAutoCoroutine = StartCoroutine(ShootAuto());
            SetEjectionState(true);
        }
        else if (ctx.canceled)
        {
            _isShootingAuto = false;
            // Don't stop it immediately if you want the last shot to finish, 
            // but for responsiveness, stopping it is usually fine.
        }
    }

    public void OnReload(InputAction.CallbackContext ctx)
    {
        if (!PlayerController.Instance.playerHealth.isAlive) return;

        if(_currentAmmo >= ammoData.maxAmmo || totalAmountOfCarryAmmo <= 0) return;

        if (isCheckingForAmmo) return;

        if (!this.gameObject.activeSelf) return; // exit early if weapon is inactive

        if (ctx.started)
        {
            Reloading();
        }
    }

    public void OnCheckMagazine(InputAction.CallbackContext ctx)
    {
        if (!PlayerController.Instance.playerHealth.isAlive) return;
        if (!this.gameObject.activeSelf) return;

        if (ctx.started)
        {
            PlayCheckMagAnimation();
        }

        if(ctx.canceled)
        {
            PlayCloseMagAnimation();
        }
    }

    #endregion
}

