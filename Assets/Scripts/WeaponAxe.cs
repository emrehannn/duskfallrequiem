using UnityEngine;

// This script needs a few more adjustments. Currently it spins faster when you hold left click, and slower with camera movement. We need to make
// the camera movement based spinning very slow, make it rotate around itself as it orbits the player and make holding left click spinning faster.
// have a nice day future emre

public class WeaponAxe : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    
    [Header("Orbit Settings")]
    public float orbitDistance = 2f;
    public float yOffset = 1f;
    
    [Header("Physics Settings")]
    public float rotationSpeed = 300f;        // How fast the sword rotates with input
    public float momentumDecay = 0.95f;       // How quickly momentum slows down (0-1)
    public float maxAngularVelocity = 800f;   // Maximum rotation speed
    public float spinSpeedMultiplier = 2f;    // How much faster to spin when holding mouse button
    
    [Header("Input")]
    public string horizontalInputAxis = "Mouse X"; // Use "Horizontal" for keyboard/gamepad
    public bool useRawInput = true;           // Use GetAxisRaw instead of GetAxis
    
    // Internal variables
    private float currentAngle = 0f;
    private float angularVelocity = 0f;
    private bool isSpinning = false;
    
    void Start()
    {
        Debug.Log("SwordOrbitController started - INPUT BASED");
        // Initialize position
        transform.position = GetOrbitPosition();
    }
    
    void Update()
    {
        // Check if left mouse button is held down
        isSpinning = Input.GetMouseButton(0);
        
        // Get input from Unity's input system 
        float input = 0f;
        
        if (isSpinning)
        {
            // When spinning, apply constant rotation in one direction
            input = 1f * spinSpeedMultiplier;
        }
        else
        {
            // Normal control when not spinning
            input = useRawInput ? 
                Input.GetAxisRaw(horizontalInputAxis) : 
                Input.GetAxis(horizontalInputAxis);
        }
        
        // Apply input to angular velocity (if there is input)
        if (Mathf.Abs(input) > 0.01f)
        {
            // Add to angular velocity based on input
            angularVelocity += input * rotationSpeed * Time.deltaTime;
            
            // Clamp maximum velocity
            angularVelocity = Mathf.Clamp(angularVelocity, -maxAngularVelocity, maxAngularVelocity);
        }
        else
        {
            // Apply momentum decay when no input - using Time.deltaTime for framerate independence
            angularVelocity *= Mathf.Pow(momentumDecay, Time.deltaTime * 60f);
            
            // Stop very small velocities to prevent jitter
            if (Mathf.Abs(angularVelocity) < 0.1f)
                angularVelocity = 0f;
        }
        
        // Apply angular velocity to angle
        currentAngle += angularVelocity * Time.deltaTime;
        
        // Keep angle normalized (0-360)
        if (currentAngle > 360f)
            currentAngle -= 360f;
        else if (currentAngle < 0f)
            currentAngle += 360f;
        
        // Set sword position based on angle
        transform.position = GetOrbitPosition();
        
        // Calculate the sword's orientation to point outward
        Vector3 outwardDirection = transform.position - new Vector3(player.position.x, transform.position.y, player.position.z);
        
        if (outwardDirection != Vector3.zero)
        {
            // Set the forward direction of the sword to point outward horizontally
            Quaternion lookRotation = Quaternion.LookRotation(outwardDirection);
            
            // Adjust the up vector to make +Y point outward (this rotates the sword so it's not pointing up)
            Vector3 upDirection = outwardDirection.normalized;
            
            // Create a rotation that points the sword's forward (+Z) horizontally outward
            // and the sword's up (+Y) also outward but in the horizontal plane
            transform.rotation = Quaternion.LookRotation(Vector3.Cross(Vector3.up, upDirection), Vector3.up);
            
            // Apply a 90-degree rotation around the local X axis to make +Y point outward instead of +Z
            transform.Rotate(90f, 0f, 0f, Space.Self);
        }
    }
    
    Vector3 GetOrbitPosition()
    {
        // Calculate position on orbit circle
        float radians = currentAngle * Mathf.Deg2Rad;
        float x = player.position.x + Mathf.Cos(radians) * orbitDistance;
        float z = player.position.z + Mathf.Sin(radians) * orbitDistance;
        
        return new Vector3(x, player.position.y + yOffset, z);
    }
    
    void OnDrawGizmos()
    {
        if (player != null && Application.isPlaying)
        {
            // Draw orbit circle for debugging
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3(player.position.x, player.position.y + yOffset, player.position.z);
            Gizmos.DrawWireSphere(center, orbitDistance);
            
            // Draw line from player to sword
            Gizmos.color = Color.red;
            Gizmos.DrawLine(center, transform.position);
            
            // Draw sword orientation axes
            Gizmos.color = Color.blue; // Forward
            Gizmos.DrawRay(transform.position, transform.forward * 0.5f);
            Gizmos.color = Color.green; // Up
            Gizmos.DrawRay(transform.position, transform.up * 0.5f);
            Gizmos.color = Color.red; // Right
            Gizmos.DrawRay(transform.position, transform.right * 0.5f);
        }
    }
    
    void OnGUI()
    {
        GUI.Box(new Rect(5, 5, 200, 125), "Sword Control");
        GUI.Label(new Rect(10, 25, 300, 20), $"Angle: {currentAngle:F1}°");
        GUI.Label(new Rect(10, 45, 300, 20), $"Velocity: {angularVelocity:F1}");
        GUI.Label(new Rect(10, 65, 300, 20), $"Input Axis: {horizontalInputAxis}");
        GUI.Label(new Rect(10, 85, 300, 20), $"Last Input: {Input.GetAxisRaw(horizontalInputAxis):F2}");
        GUI.Label(new Rect(10, 105, 300, 20), $"Spinning: {isSpinning}");
    }
}