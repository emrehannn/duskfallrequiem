using UnityEngine;

public class GTACamera : MonoBehaviour
{
    [SerializeField] private float sensitivityX = 2f;
    [SerializeField] private float sensitivityY = 2f;
    public float rotationSpeed = 2f;
    public float zoomSpeed = 2f;
    public float minZoomDistance = 2f;
    public float maxZoomDistance = 10f;

    private float mouseX;
    private float mouseY;
    private float currentZoomDistance;
    private bool cursorLocked = true;
    private Transform pivot;
    private LayerMask collisionMask;
    private Transform target;

    // Store default sensitivities
    private float defaultSensitivityX;
    private float defaultSensitivityY;

    // Define layer indices here (modify these as needed)
    private int[] collidableLayers = { 3, 6 }; // Example: terrain = 3, object = 6

    private float cameraCollisionOffset = 0.2f;  // A default offset to keep distance from the surface
    private float minimumCollisionDistance = 3f; // Distance from terrain where we should register a collision

    void Start()
    {
        // Store default sensitivities
        defaultSensitivityX = sensitivityX;
        defaultSensitivityY = sensitivityY;

        // Get target from PlayerManager
        target = PlayerManager.Instance.GetPlayer();
        
        if (target == null)
        {
            Debug.LogError("Target is not assigned!");
            return;
        }

        // Create a pivot to fix rotation issues
        pivot = new GameObject("Camera Pivot").transform;
        pivot.position = target.position;
        pivot.parent = target;

        // Convert layer indices to a LayerMask
        collisionMask = LayerMaskFromIndices(collidableLayers);

        currentZoomDistance = (transform.position - target.position).magnitude;
        LockCursor(true);
    }

    void Update()
    {
        HandleCursorLock();
        
        // Adjust sensitivity when Mouse1 is held (for sword control)
        float sensitivityMultiplier = Input.GetMouseButton(0) ? 0.1f : 1f;
        sensitivityX = defaultSensitivityX * sensitivityMultiplier;
        sensitivityY = defaultSensitivityY * sensitivityMultiplier;
    }

    void LateUpdate()
    {
        if (cursorLocked)
        {
            mouseX += Input.GetAxis("Mouse X") * sensitivityX;
            mouseY -= Input.GetAxis("Mouse Y") * sensitivityY;

            mouseY = Mathf.Clamp(mouseY, -70f, 70f); // Prevent flipping
        }

        // Rotate the pivot
        pivot.rotation = Quaternion.Euler(mouseY, mouseX, 0);

        // Handle zoom input
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        currentZoomDistance = Mathf.Clamp(currentZoomDistance - scrollInput * zoomSpeed, minZoomDistance, maxZoomDistance);

        // Perform collision check with offset consideration
        Vector3 desiredCameraPosition = pivot.position - pivot.forward * currentZoomDistance;
        RaycastHit hit;

        // Raycast to detect collisions between the pivot and the terrain (or other layers)
        if (Physics.Raycast(pivot.position, -pivot.forward, out hit, currentZoomDistance + minimumCollisionDistance, collisionMask))
        {
            // Adjust the camera position to make sure it stays 1 unit away from the terrain
            float distanceToGround = hit.distance;

            // Ensure we register a collision even if we are 1 unit away from the terrain
            float adjustedCameraDistance = Mathf.Max(distanceToGround - minimumCollisionDistance, cameraCollisionOffset); 

            transform.position = pivot.position - pivot.forward * adjustedCameraDistance;
        }
        else
        {
            // If no collision, just set the camera at the desired position with the zoom
            transform.position = desiredCameraPosition;
        }

        // Make camera look at target
        transform.LookAt(target);
    }

    private void HandleCursorLock()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LockCursor(false);
        }
        else if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
        {
            LockCursor(true);
        }
    }

    private void LockCursor(bool lockCursor)
    {
        Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !lockCursor;
        cursorLocked = lockCursor;
    }

    private LayerMask LayerMaskFromIndices(int[] layerIndices)
    {
        int mask = 0;
        foreach (int layer in layerIndices)
        {
            mask |= 1 << layer;
        }
        return mask;
    }

    // Public method to manually set sensitivity multiplier (if needed)
    public void SetSensitivityMultiplier(float multiplier)
    {
        sensitivityX = defaultSensitivityX * multiplier;
        sensitivityY = defaultSensitivityY * multiplier;
    }
}