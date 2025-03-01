using UnityEngine;

public class ragdollMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float acceleration = 150f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Height Settings")]
    [SerializeField] private float minHeightThreshold = 0.5f;
    [SerializeField] private float heightCorrectionForce = 20f;
    [SerializeField] private LayerMask groundLayer;

    [Header("References")]
    [SerializeField] private Rigidbody hipBone;
    [SerializeField] private Rigidbody backBone;
    [SerializeField] private Rigidbody headBone;
    [SerializeField] private Camera mainCamera;

    public Rigidbody HipBone => hipBone;

    private Vector3 moveDirection;
    private float currentGroundDistance;
    private bool isMoving;

    private void Start()
    {
        InitializeRagdoll();

            // Find your hip bone

    Rigidbody hipBone = GetComponentInChildren<Rigidbody>();  // If hip is the only/first rigidbody

    // OR if you need to find specific hip bone

    Rigidbody[] allBones = GetComponentsInChildren<Rigidbody>();

    foreach (Rigidbody rb in allBones)

    {

        if (rb.gameObject.name.ToLower().Contains("hip") || 

            rb.gameObject.name.ToLower().Contains("pelvis"))

        {

            PlayerTarget.SetTarget(rb.transform);

            break;

        }

    }

    }

    private void InitializeRagdoll()
    {
        if (hipBone != null)
        {
            hipBone.constraints = RigidbodyConstraints.FreezeRotation;
            hipBone.mass = 3f;
            hipBone.linearDamping = 0.01f;
            hipBone.interpolation = RigidbodyInterpolation.Interpolate;
        }

        if (backBone != null)
        {
            backBone.useGravity = true;
            backBone.mass = 1f;
        }

        if (headBone != null)
        {
            headBone.useGravity = true;
            headBone.mass = 1f;
        }
    }

    private void Update()
    {
        HandleMovementInput();
        HandleRotation();

        transform.position = new Vector3(transform.position.x,
            Mathf.Clamp(transform.position.y, -50f, 50f),
            transform.position.z);
    }

    private void HandleMovementInput()
    {
        // Get input axes
        float horizontal = Input.GetAxisRaw("Horizontal"); // A & D or Arrow keys
        float vertical = Input.GetAxisRaw("Vertical");     // W & S or Arrow keys

        // Create movement vector
        Vector3 input = new Vector3(horizontal, 0f, vertical).normalized;

        // Convert input to world space relative to camera
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;

        // Project camera's forward and right onto the horizontal plane (y = 0)
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Calculate movement direction in world space
        moveDirection = (cameraForward * input.z + cameraRight * input.x).normalized;
        isMoving = input.magnitude > 0.1f;
    }

    private void HandleRotation()
    {
        if (hipBone != null)
        {
            Plane characterPlane = new Plane(Vector3.up, hipBone.position);
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (characterPlane.Raycast(ray, out float distance))
            {
                Vector3 lookAtPoint = ray.GetPoint(distance);
                Vector3 directionToMouse = (lookAtPoint - hipBone.position).normalized;
                directionToMouse.y = 0;

                if (directionToMouse != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToMouse);
                    hipBone.MoveRotation(Quaternion.Lerp(hipBone.rotation, targetRotation,
                        Time.deltaTime * rotationSpeed));
                }
            }
        }
    }

    public void HandleDeath()
    {
        if (hipBone != null)
        {
            hipBone.constraints = RigidbodyConstraints.None;
        }

        // Get all rigidbodies for natural falling
        Rigidbody[] allBones = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in allBones)
        {
            rb.linearDamping = 0.5f;
            rb.angularDamping = 0.5f;
        }

        // Disable this script
        this.enabled = false;
    }

    private void FixedUpdate()
    {
        CheckGroundDistance();
        MoveCharacter();
    }

    private void CheckGroundDistance()
    {
        if (headBone != null)
        {
            RaycastHit hit;
            if (Physics.Raycast(headBone.position, Vector3.down, out hit, 10f, groundLayer))
            {
                currentGroundDistance = hit.distance;

                Debug.DrawRay(headBone.position, Vector3.down * hit.distance,
                    currentGroundDistance < minHeightThreshold ? Color.red : Color.green);

                if (currentGroundDistance < minHeightThreshold)
                {
                    float heightDifference = minHeightThreshold - currentGroundDistance;
                    float correctionMultiplier = heightDifference / minHeightThreshold;

                    Vector3 correctionForce = Vector3.up * heightCorrectionForce * correctionMultiplier;
                    hipBone.AddForce(correctionForce, ForceMode.Acceleration);
                    backBone.AddForce(correctionForce, ForceMode.Acceleration);
                    headBone.AddForce(correctionForce, ForceMode.Acceleration);
                }
            }
        }
    }

    private void MoveCharacter()
    {
        if (hipBone != null)
        {
            if (isMoving)
            {
                Vector3 targetVelocity = moveDirection * maxSpeed;
                targetVelocity.y = hipBone.linearVelocity.y;
                
                hipBone.linearVelocity = Vector3.MoveTowards(
                    hipBone.linearVelocity,
                    targetVelocity,
                    acceleration * Time.fixedDeltaTime *2
                );
            }
            else
            {
                Vector3 currentVel = hipBone.linearVelocity;
                currentVel.y = hipBone.linearVelocity.y;
                hipBone.linearVelocity = Vector3.MoveTowards(currentVel, Vector3.zero, acceleration * Time.fixedDeltaTime);
            }
        }
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 20), $"fanusun yarragi: {currentGroundDistance:F2} cm ");
        GUI.Label(new Rect(10, 50, 200, 20), $"Velocity: {hipBone.linearVelocity.magnitude:F2}");
    }
}