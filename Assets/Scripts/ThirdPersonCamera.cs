using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public float distance = 10.0f;
    public float height = 5.0f;
    public Vector3 offset = new Vector3(0, 0, -1);

    [Header("Movement Settings")]
    public float smoothSpeed = 5f;
    public float rotationSmoothSpeed = 5f;
    public float heightSmoothSpeed = 3f;

    [Header("Collision Settings")]
    public float minDistance = 3f;
    public float maxDistance = 12f;
    public float collisionOffset = 0.3f;
    public LayerMask collisionLayers;

    private Vector3 currentVelocity;
    private float currentHeight;
    private float targetHeight;

    private void Start()
    {
        currentHeight = height;
        targetHeight = height;
    }

    private void LateUpdate()
    {
        if (!target) return;

        // Calculate desired position
        Vector3 targetPosition = target.position;
        targetHeight = Mathf.Lerp(targetHeight, height + target.position.y, Time.deltaTime * heightSmoothSpeed);
        
        // Calculate camera position
        Vector3 desiredPosition = targetPosition + (offset.normalized * distance);
        desiredPosition.y = targetHeight;

        // Check for collision
        RaycastHit hit;
        Vector3 directionToTarget = (target.position - desiredPosition).normalized;
        if (Physics.SphereCast(target.position, 0.5f, -directionToTarget, out hit, distance, collisionLayers))
        {
            float distanceToObstacle = hit.distance;
            desiredPosition = target.position - directionToTarget * (distanceToObstacle - collisionOffset);
        }

        // Smooth movement
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / smoothSpeed);

        // Ground check
        RaycastHit groundHit;
        if (Physics.Raycast(transform.position, Vector3.down, out groundHit, height, collisionLayers))
        {
            float minHeightAboveGround = 1f;
            if (groundHit.distance < minHeightAboveGround)
            {
                Vector3 correctedPosition = transform.position;
                correctedPosition.y = groundHit.point.y + minHeightAboveGround;
                transform.position = correctedPosition;
            }
        }

        // Smooth look at target
        Vector3 directionToLook = target.position - transform.position;
        if (directionToLook != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToLook);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothSpeed);
        }
    }

    // Optional: Visualize camera settings in Scene view
    private void OnDrawGizmosSelected()
    {
        if (target)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(target.position, 0.5f);
            Gizmos.DrawLine(target.position, transform.position);
        }
    }
}