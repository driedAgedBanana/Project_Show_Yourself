using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    private Vector2 _moveInput;

    public Camera playerCam;
    public Rigidbody rb;

    [Header("Camera Settings")]
    public Transform camHolder;
    public float sensitivity = 15f;
    private Vector2 _lookDirection;
    [Range(0f, 90f)] private float _xClamp = 50f;
    private float _xRotation;
    private Vector3 _originalCamPosition;
    [SerializeField] private Transform _camHolder;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float slowWalkSpeed = 2f;
    public float sprintSpeed = 10f;

    public LayerMask groundLayer;
    public GameObject groundChecker;
    public float groundCheckDistance = 0.2f;

    [Space]
    // Stamina for sprinting
    public float maxStamina;
    private float _currentStamina;
    public float staminaDrainRate;
    public float staminaNormalGainRate;
    public float staminaFastGainRate;
    public float cooldownTime;

    private bool _canSprint;
    private float _coolDownTimer;

    private bool _isSlowWalk;
    private bool _isRunning;
    private bool _runButtonPressed;
    private bool _isMoving;

    [Header("Head Bobbing")]
    public float walkBobSpeed = 14f;
    public float walkBobAmount = 0.05f;
    public float sprintBobSpeed = 18f;
    public float sprintBobAmount = 0.1f;

    private float _defaultPosY = 0f;
    private float _timer;

    [Header("Crouching Mechanic")]
    public float crouchSpeed;
    public float crouchHeight = 1f;
    public float standHeight = 2f;
    public float crouchCamY = 0.8f;
    public float standCamY = 1.6f;

    private bool _isCrouching;

    [SerializeField] private CapsuleCollider _playerCollider;

    public LayerMask obstacleLayer;

    [Header("Sliding Mechanic")]
    public float slideForce = 12f;
    public float slideDuration = 0.8f;
    public float minRunTimeBeforeSlide = 1.5f;
    public float slidingStaminaDrainRate = 30f;
    private bool _sprintBlockedAfterSlide = false;

    [SerializeField] private float _runTimer = 0f;
    [SerializeField] private bool _isSliding = false;
    [SerializeField] private float _slideTimer = 0f;

    [Header("Leaning Mechanic")]
    public float leaningAmount = 20f;
    public float leaningSpeed = 15f;

    private bool _leaningAllowed;
    private Quaternion _targetLeanRotation;
    private float _leaningDirection;

    public Vector2 GetMovementInput() => _moveInput;
    public Vector2 GetLookInput() => _lookDirection;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance);
        }
        else
        {
            Instance = this;
        }

        rb = GetComponent<Rigidbody>();

        _originalCamPosition = camHolder.localPosition;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _currentStamina = maxStamina;
        _leaningAllowed = true;
        _defaultPosY = transform.localPosition.y;
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    // Update is called once per frame
    void Update()
    {
        HandleLook();
        HandleLeaning();
        IsGrounded();

        // Tracking running time
        if (_isRunning)
        {
            _runTimer += Time.deltaTime;
        }
        else
        {
            _runTimer = 0f;
        }

        if (_isSliding)
        {
            _slideTimer += Time.deltaTime;
            if (_slideTimer >= slideDuration)
            {
                StopSliding();
            }
        }
    }

    private void HandleLook()
    {
        float mouseX = _lookDirection.x * sensitivity * Time.deltaTime;
        float mouseY = _lookDirection.y * sensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);
        _xRotation = Mathf.Clamp(_xRotation - mouseY, -_xClamp, _xClamp);
        _camHolder.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
    }

    public bool IsGrounded()
    {
        return Physics.Raycast(groundChecker.transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }

    private void HandleLeaning()
    {
        if (!_leaningAllowed)
        {
            return;            
        }
        else
        {
            if(_leaningDirection > 0) // Lean left
            {
                _targetLeanRotation = Quaternion.Euler(0, transform.localEulerAngles.y, -leaningAmount);
            }
            else if (_leaningDirection < 0) // Lean right
            {
                _targetLeanRotation = Quaternion.Euler(0, transform.localEulerAngles.y, leaningAmount);
            }
            else
            {
                _targetLeanRotation = Quaternion.Euler(0, transform.localEulerAngles.y, 0);
            }
        }

        // Smoothly interpolate to the target rotation
        transform.localRotation = Quaternion.Slerp(transform.localRotation, _targetLeanRotation, Time.deltaTime * leaningSpeed);
    }

    private void ResetLeaning()
    {
        transform.localRotation = Quaternion.Slerp(transform.localRotation, _targetLeanRotation, Time.deltaTime * leaningSpeed);
        transform.localRotation = Quaternion.Euler(0, transform.localEulerAngles.y, 0);
    }

    private void HandleMovement()
    {
        if (_isSliding) return;

        _canSprint = _runButtonPressed && _isMoving && !_isCrouching && _currentStamina > 0 && !_isSliding && !_leaningAllowed;
        float speed = _isCrouching ? crouchSpeed : (_canSprint ? sprintSpeed : (_isSlowWalk ? slowWalkSpeed : moveSpeed));

        Vector3 movement = transform.right * _moveInput.x + transform.forward * _moveInput.y;
        rb.linearVelocity = new Vector3(movement.x * speed, rb.linearVelocity.y, movement.z * speed);

        _isMoving = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude > 0.1f;
        float baseY = _isCrouching ? crouchCamY : standCamY;

        // Head bobbing

        if (_isMoving)
        {
            bool isSprinting = _canSprint;
            float bobSpeed = isSprinting ? sprintBobSpeed : walkBobSpeed;
            float bobAmount = isSprinting ? sprintBobAmount : walkBobAmount;

            // Calculate the head bobbing
            _timer += Time.deltaTime * bobSpeed;
            camHolder.localPosition = new Vector3(camHolder.localPosition.x, baseY + Mathf.Sin(_timer) * bobAmount, camHolder.localPosition.z);
        }
        else
        {
            // Reset timer and smoothly return to original position
            _timer = 0f;
            camHolder.localPosition = new Vector3(camHolder.localPosition.x, Mathf.Lerp(camHolder.localPosition.y, baseY, Time.deltaTime * 5f), camHolder.localPosition.z);
        }

        if (_canSprint)
        {
            _currentStamina -= staminaDrainRate * Time.deltaTime;
            _coolDownTimer = 0f;
            _isRunning = true;
        }
        else
        {
            _isRunning = false;
        }

        float targetFOV = _canSprint ? 80f : 60f;
        playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, targetFOV, Time.deltaTime * 5f);

        _currentStamina = Mathf.Clamp(_currentStamina, 0, maxStamina);
    }

    private void CrouchDown()
    {
        _isCrouching = true;
        _playerCollider.height = crouchHeight;
        //_playerCollider.center = new Vector3(0, crouchHeight / 2f, 0);

        // Lower the camera
        camHolder.localPosition = new Vector3(camHolder.localPosition.x, crouchCamY, camHolder.localPosition.z);
        rb.AddForce(Vector3.down * 3f, ForceMode.Impulse);
    }

    private bool CanStandUp()
    {
        float radius = _playerCollider.radius * 0.95f;
        float startY = transform.position.y + crouchHeight / 2f;
        float endY = transform.position.y + standHeight / 2f;

        Vector3 point1 = new Vector3(transform.position.x, startY, transform.position.z);
        Vector3 point2 = new Vector3(transform.position.x, endY, transform.position.z);

        Debug.DrawLine(point1, point2, Color.red, 1f);

        return !Physics.CheckCapsule(point1, point2, radius, obstacleLayer);
    }

    private void StandUp()
    {
        if (!CanStandUp()) return;

        _isCrouching = false;
        _playerCollider.height = standHeight;
        //_playerCollider.center = new Vector3(0, standHeight / 2f, 0);

        // Ensure standCamY is a small value (like 0.8), not the full height of the player
        camHolder.localPosition = new Vector3(camHolder.localPosition.x, standCamY, camHolder.localPosition.z);
    }

    private void StartSliding()
    {
        _isSliding = true;
        _isCrouching = true;

        CrouchDown();

        Vector3 slideDirection = rb.linearVelocity.normalized;
        rb.AddForce(slideDirection * slideForce, ForceMode.VelocityChange);

        if (_isSliding)
        {
            _currentStamina -= slidingStaminaDrainRate * Time.deltaTime;
            _slideTimer = 0f;
        }

        print("Sliding started");
    }

    private void StopSliding()
    {
        _isSliding = false;
        _sprintBlockedAfterSlide = true;
        _canSprint = false;

        print("Sliding stopped");
    }

    #region Inputs

    public void OnLook(InputAction.CallbackContext context)
    {
        _lookDirection = context.ReadValue<Vector2>();
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnLean(InputAction.CallbackContext ctx)
    {
        if (_sprintBlockedAfterSlide) return;

        float leaningInput = ctx.ReadValue<float>();
        _leaningDirection = leaningInput;
    }

    public void OnRun(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && _isMoving)
        {
            ResetLeaning();

            if (_isCrouching && CanStandUp())
            {
                StandUp();
            }
            _isRunning = true;
            _runButtonPressed = true;
            _leaningAllowed = false;
        }

        if (ctx.canceled)
        {
            _leaningAllowed = true;
            _runButtonPressed = false;
            _canSprint = true;
            _sprintBlockedAfterSlide = false;
        }
    }

    public void OnCrouch(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (_isRunning && _runTimer >= minRunTimeBeforeSlide && !_isSliding && _canSprint)
            {
                StartSliding();
            }
            else
            {
                if (_isCrouching)
                {
                    StandUp();
                }
                else
                {
                    CrouchDown();
                }
            }
        }
    }

    #endregion
}
