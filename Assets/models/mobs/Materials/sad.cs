using UnityEngine;

public class FreeLookCamera : MonoBehaviour
{
    [Header("Movement Settings")]
    public float movementSpeed = 10f;
    public float boostMultiplier = 2f;
    public float verticalSpeed = 5f;

    [Header("Rotation Settings")]
    public float mouseSensitivity = 3f;
    public float rotationSmoothness = 10f;
    
    private float m_yaw;
    private float m_pitch;
    private bool m_cursorLocked = true;

    void Start()
    {
        LockCursor(true);
        m_yaw = transform.eulerAngles.y;
        m_pitch = transform.eulerAngles.x;
    }

    void Update()
    {
        HandleCursorInput();
        HandleMovement();
        HandleRotation();
    }

    void HandleCursorInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LockCursor(!m_cursorLocked);
        }
    }

    void LockCursor(bool lockState)
    {
        m_cursorLocked = lockState;
        Cursor.lockState = lockState ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !lockState;
    }

    void HandleMovement()
    {
        // Calculate boost speed
        float currentSpeed = movementSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed *= boostMultiplier;
        }

        // Get WASD input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Calculate movement direction
        Vector3 movement = (transform.forward * vertical + transform.right * horizontal) * currentSpeed;

        // Add vertical movement (Space/Shift)
        if (Input.GetKey(KeyCode.Space))
        {
            movement += Vector3.up * verticalSpeed;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            movement += Vector3.down * verticalSpeed;
        }

        // Apply movement
        transform.Translate(movement * Time.deltaTime, Space.World);
    }

    void HandleRotation()
    {
        if (!m_cursorLocked) return;

        // Get mouse input
        m_yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        m_pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        m_pitch = Mathf.Clamp(m_pitch, -90f, 90f);

        // Smoothly rotate towards target rotation
        Quaternion targetRotation = Quaternion.Euler(m_pitch, m_yaw, 0f);
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            rotationSmoothness * Time.deltaTime
        );
    }
}