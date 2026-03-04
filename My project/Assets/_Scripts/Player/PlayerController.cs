using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [Header("Script references")]
    public WeaponBase[] weaponBase;
    public PlayerHealth playerHealth;
    public CameraShakeManager cameraShakeManager;
    public PlayerInteract playerInteract;

    [Header("References")]
    public Rigidbody rb;
    public Camera playerCam;
    public Transform leanPivot;
    public Transform camHolder;
    public CapsuleCollider playerCollider;

    [Header("Mouse Look")]
    public float sensitivity = 15f;
    [Range(0, 90)] public float xClamp = 50f;
    private float xRotation;
    [HideInInspector] public Vector2 lookInput;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float slowWalkSpeed = 2f;
    public float sprintSpeed = 10f;
    public float crouchSpeed = 2f;
    [Space]
    private Vector2 _currentMoveInput;
    private Vector2 _moveInputVelocity;
    public float inputSmoothTime = 0.1f;

    private Vector2 moveInput;
    [HideInInspector] public bool isMoving;

    [Header("Stamina")]
    public float maxStamina = 5f;
    public float staminaDrainRate = 1f;
    public float staminaRecoverRate = 0.5f;
    private float currentStamina;
    [HideInInspector] public bool canSprint;
    private bool runHeld;

    [Header("FOV")]
    public int normalFOV = 60;
    public int sprintFOV = 80;

    [Header("Head Bob")]
    public float walkBobSpeed = 14f;
    public float walkBobAmount = 0.05f;
    public float sprintBobSpeed = 18f;
    public float sprintBobAmount = 0.1f;
    private float bobTimer;

    [Header("Crouch")]
    public float standHeight = 2f;
    public float crouchHeight = 1f;
    public float standCamY = 1.6f;
    public float crouchCamY = 0.8f;
    private bool isCrouching;

    [Header("Lean")]
    public float leanAngle = 20f;
    public float leanSpeed = 10f;
    private float leanInput;

    [Header("Ground Check")]
    public Transform groundChecker;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;

    [Header("Effects")]
    public GameObject globalVolume;
    private GameObject spawnedVolume;
    [SerializeField] private Volume _playerVFX;
    private Vignette _vignette;
    private ChromaticAberration _chromaticAberration;
    private DepthOfField _depthOfField;

    [Header("SFX")]
    public AudioList footstepSounds;
    private bool _hasStepped; // Prevents the sound from playing every frame at the bottom of the curve

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        GameManager.Instance.HideMouse();

        playerHealth = GetComponent<PlayerHealth>();
        playerInteract = GetComponent<PlayerInteract>();

        if (globalVolume != null)
        {
            // 1. Create the object and store a reference to the instance
            spawnedVolume = Instantiate(globalVolume.gameObject, Vector3.zero, Quaternion.identity);

            // 2. Assign the specific component of that new instance back to your variable
            globalVolume = spawnedVolume.GetComponent<Volume>().gameObject;

            // 3. Now safely get the Volume component from the newly spawned object
            _playerVFX = globalVolume.GetComponent<Volume>();
        }
    }

    private void Start()
    {
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        currentStamina = maxStamina;
        cameraShakeManager = GetComponentInChildren<CameraShakeManager>();


        if (_playerVFX.profile.TryGet<Vignette>(out _vignette))
        {
            _vignette.intensity.value = 0.3f;
        }

        if (_playerVFX.profile.TryGet<ChromaticAberration>(out _chromaticAberration))
        {
            _chromaticAberration.intensity.value = 0f;
        }

        if (_playerVFX.profile.TryGet<DepthOfField>(out _depthOfField))
        {
            _depthOfField.focalLength.value = 0f;
        }
    }

    private void LateUpdate()
    {
        if (playerHealth.isAlive)
        {
            HandleLook();
        }
    }

    private void Update()
    {
        if (playerHealth.isAlive)
        {
            _currentMoveInput = Vector2.SmoothDamp(_currentMoveInput, moveInput, ref _moveInputVelocity, inputSmoothTime);

            HandleLean();
            HandleFOV();
            HandleHeadBob();
            HandleStamina();
        }
    }

    private void FixedUpdate()
    {
        HandleMovementPhysics();
    }

    private void HandleLook()
    {
        float mouseX = lookInput.x * sensitivity;
        float mouseY = lookInput.y * sensitivity;

        // Yaw
        transform.Rotate(Vector3.up * mouseX);

        // Pitch
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -xClamp, xClamp);

        Vector3 euler = camHolder.localEulerAngles;
        euler.x = xRotation;
        camHolder.localEulerAngles = euler;
    }

    private void HandleMovementPhysics()
    {
        float targetSpeed = moveSpeed;

        if (isCrouching)
        {
            targetSpeed = crouchSpeed;
        }
        else if (canSprint)
        {
            targetSpeed = sprintSpeed;
        }
        foreach (WeaponBase weapon in weaponBase)
        {
            if (weapon.isAiming)
            {
                targetSpeed = slowWalkSpeed;
                break;
            }
        }

        Vector3 moveDir = transform.right * _currentMoveInput.x + transform.forward * _currentMoveInput.y;
        Vector3 targetVelocity = moveDir * targetSpeed;

        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 velocityChange = (targetVelocity - currentVelocity);

        velocityChange.y = 0;

        rb.AddForce(velocityChange, ForceMode.VelocityChange);

        isMoving = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude > 0.1f;
    }

    private void HandleHeadBob()
    {
        float baseY = isCrouching ? crouchCamY : standCamY;

        foreach (WeaponBase weapon in weaponBase)
        {
            if (weapon.isAiming || !isMoving)
            {
                bobTimer = 0;
                Vector3 pos = camHolder.localPosition;
                pos.y = Mathf.Lerp(pos.y, baseY, Time.deltaTime * 8f);
                camHolder.localPosition = pos;
                return;
            }
        }

        bool sprinting = canSprint;
        float speed = sprinting ? sprintBobSpeed : walkBobSpeed;
        float amount = sprinting ? sprintBobAmount : walkBobAmount;

        bobTimer += Time.deltaTime * speed;
        float sinValue = Mathf.Sin(bobTimer); // Get the raw sine value (-1 to 1)
        float offset = sinValue * amount;

        // --- FOOTSTEP LOGIC START ---
        // If the sine wave is at the bottom (approaching -1)
        if (sinValue < -0.9f && !_hasStepped)
        {
            AudioManager.Instance.PlaySounds(footstepSounds, transform.position);
            _hasStepped = true; // Mark that we've played the sound for this "dip"
        }
        // Reset the flag when the head starts moving back up
        else if (sinValue > 0)
        {
            _hasStepped = false;
        }
        // --- FOOTSTEP LOGIC END ---

        Vector3 newPos = camHolder.localPosition;
        newPos.y = baseY + offset;
        camHolder.localPosition = newPos;
    }

    private void HandleStamina()
    {
        canSprint = runHeld && isMoving && !isCrouching && currentStamina > 0;

        if (canSprint)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
        }
        else
        {
            currentStamina += staminaRecoverRate * Time.deltaTime;
        }

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        // Calculate the "Tiredness" (0 when full stamina, 1 when empty)
        float tiredness = 1f - (currentStamina / maxStamina);

        // Apply the intensity. 
        // This will go from 0.1f (resting) to 0.5f (exhausted)
        if (_vignette != null)
        {
            float targetIntensity = Mathf.Lerp(0.1f, 0.5f, tiredness);
            _vignette.intensity.value = Mathf.MoveTowards(_vignette.intensity.value, targetIntensity, Time.deltaTime);
        }

        if (_chromaticAberration != null)
        {
            float targetIntensity = Mathf.Lerp(0f, 1f, tiredness);
            _chromaticAberration.intensity.value = Mathf.MoveTowards(_chromaticAberration.intensity.value, targetIntensity, Time.deltaTime);
        }

        if (_depthOfField != null)
        {
            float targetFocalLength = Mathf.Lerp(1f, 50f, tiredness);
            _depthOfField.focalLength.value = Mathf.MoveTowards(_depthOfField.focalLength.value, targetFocalLength, Time.deltaTime * 10f);
        }
    }

    private void HandleFOV()
    {
        foreach (WeaponBase weapon in weaponBase)
        {
            if (weapon.isAiming)
            {
                playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, normalFOV, Time.deltaTime * 6f);
                return;
            }
        }
    }

    private void HandleLean()
    {
        float targetZ = -leanInput * leanAngle;
        Quaternion targetRot = Quaternion.Euler(0, 0, targetZ);
        leanPivot.localRotation = Quaternion.Slerp(leanPivot.localRotation, targetRot, Time.deltaTime * leanSpeed);
    }


    private void ToggleCrouch()
    {
        isCrouching = !isCrouching;
        playerCollider.height = isCrouching ? crouchHeight : standHeight;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Door door))
        {
            door.PlayStaticAudio();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Door door))
        {
            door.doorAudioSource.Stop();
        }
    }


    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext ctx) => lookInput = ctx.ReadValue<Vector2>();
    public void OnRun(InputAction.CallbackContext ctx) => runHeld = ctx.ReadValueAsButton();
    public void OnLean(InputAction.CallbackContext ctx) => leanInput = ctx.ReadValue<float>();

    //public void OnCrouch(InputAction.CallbackContext ctx)
    //{
    //    if (ctx.performed) ToggleCrouch();
    //}
}
