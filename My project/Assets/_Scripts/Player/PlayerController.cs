using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

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
    public float normalFOV = 60f;
    public float sprintFOV = 80f;

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

    private void Awake()
    {
        Instance = this;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        currentStamina = maxStamina;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        HandleLook();

    }

    private void Update()
    {
        HandleLean();
        HandleFOV();
        HandleHeadBob();
        HandleStamina();
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
        float speed = isCrouching ? crouchSpeed : canSprint ? sprintSpeed : moveSpeed;

        Vector3 moveDir = transform.right * moveInput.x + transform.forward * moveInput.y;
        Vector3 velocity = moveDir * speed;
        velocity.y = rb.linearVelocity.y;

        rb.linearVelocity = velocity;

        isMoving = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude > 0.1f;
    }

    private void HandleHeadBob()
    {
        float baseY = isCrouching ? crouchCamY : standCamY;

        if (!isMoving)
        {
            bobTimer = 0;
            Vector3 pos = camHolder.localPosition;
            pos.y = Mathf.Lerp(pos.y, baseY, Time.deltaTime * 8f);
            camHolder.localPosition = pos;
            return;
        }

        bool sprinting = canSprint;
        float speed = sprinting ? sprintBobSpeed : walkBobSpeed;
        float amount = sprinting ? sprintBobAmount : walkBobAmount;

        bobTimer += Time.deltaTime * speed;
        float offset = Mathf.Sin(bobTimer) * amount;

        Vector3 newPos = camHolder.localPosition;
        newPos.y = baseY + offset;
        camHolder.localPosition = newPos;
    }

    private void HandleStamina()
    {
        canSprint = runHeld && isMoving && !isCrouching && currentStamina > 0;

        if (canSprint)
            currentStamina -= staminaDrainRate * Time.deltaTime;
        else
            currentStamina += staminaRecoverRate * Time.deltaTime;

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }

    private void HandleFOV()
    {
        float target = canSprint ? sprintFOV : normalFOV;
        playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, target, Time.deltaTime * 6f);
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


    public void OnMove(InputAction.CallbackContext ctx) => moveInput = ctx.ReadValue<Vector2>();
    public void OnLook(InputAction.CallbackContext ctx) => lookInput = ctx.ReadValue<Vector2>();
    public void OnRun(InputAction.CallbackContext ctx) => runHeld = ctx.ReadValueAsButton();
    public void OnLean(InputAction.CallbackContext ctx) => leanInput = ctx.ReadValue<float>();

    public void OnCrouch(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) ToggleCrouch();
    }
}
