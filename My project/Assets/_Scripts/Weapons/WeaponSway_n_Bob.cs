using UnityEngine;

public class WeaponSway_n_Bob : MonoBehaviour
{
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

    private Vector3 _defaultPos;
    private float _timer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _origin = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        SwayWeapon();
        WeaponBobbing();
    }

    private void SwayWeapon()
    {
        //Vector2 swayInput = new Vector2(PlayerController.Instance.mouseX, PlayerController.Instance.mouseY);

        //swayInput.x = Mathf.Clamp(swayInput.x, -swayClamp, swayClamp);
        //swayInput.y = Mathf.Clamp(swayInput.y, -swayClamp, swayClamp);

        //Vector3 target = new Vector3(-swayInput.x, -swayInput.y, 0);

        //transform.localPosition = Vector3.Lerp(transform.localPosition, target + _origin, smoothing * Time.deltaTime);

        float mouseX = Input.GetAxisRaw("Mouse X") * swayMultiplier;
        float mouseY = Input.GetAxisRaw("Mouse Y") * swayMultiplier;

        // Calculate target rotation
        Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);

        Quaternion targetRotation = rotationX * rotationY;

        // Rotate
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, smoothing * Time.deltaTime);
    }

    public void WeaponBobbing()
    {
        if (PlayerController.Instance.isMoving)
        {
            bool isSprinting = PlayerController.Instance.canSprint;
            float bobSpeed = isSprinting ? sprintBobSpeed : walkBobSpeed;
            float bobAmount = isSprinting ? sprintBobAmount : walkBobAmount;

            _timer += Time.deltaTime * bobSpeed;

            // We modify the Y (middle), keep X and Z at default
            transform.localPosition = new Vector3(_defaultPos.x, _defaultPos.y + Mathf.Sin(_timer) * bobAmount, _defaultPos.z);
        }
        else
        {
            _timer = 0f;
            // Smoothly return the Y back to defaultPos.y
            float newY = Mathf.Lerp(transform.localPosition.y, _defaultPos.y, Time.deltaTime * 5f);
            transform.localPosition = new Vector3(_defaultPos.x, newY, _defaultPos.z);
        }
    }
}
