using UnityEngine;

public class GTACamera : MonoBehaviour
{
    public Transform target;
    public float rotationSpeed = 2f;
    public Vector3 offset = new Vector3(1f, 2f, -4f);
    
    [Header("Zoom Settings")]
    public float zoomSpeed = 2f;
    public float minZoomDistance = 2f;
    public float maxZoomDistance = 10f;

    private float mouseX;
    private float mouseY;
    private float currentZoomDistance;
    
    void Start()
    {
        // Initialize zoom distance to the magnitude of the offset
        currentZoomDistance = offset.magnitude;
    }
    
    void LateUpdate()
    {
        // Handle rotation
        mouseX += Input.GetAxis("Mouse X") * rotationSpeed;
        mouseY -= Input.GetAxis("Mouse Y") * rotationSpeed;
        mouseY = Mathf.Clamp(mouseY, -90f, 90f);

        // Handle zoom
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        currentZoomDistance = Mathf.Clamp(currentZoomDistance - scrollInput * zoomSpeed, 
                                        minZoomDistance, maxZoomDistance);

        // Calculate new position with zoom
        Vector3 zoomedOffset = offset.normalized * currentZoomDistance;
        transform.position = target.position + Quaternion.Euler(mouseY, mouseX, 0) * zoomedOffset;
        transform.LookAt(target);
    }
}