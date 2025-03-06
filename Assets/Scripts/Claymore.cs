using UnityEngine;

public class SwordOrbitController : MonoBehaviour
{
    [Header("Orbit Settings")]
    public float orbitDistance = 2f;
    public float yOffset = 1f;
    
    [Header("Physics Settings")]
    public float rotationSpeed = 300f;        // How fast the sword rotates with input
    public float momentumDecay = 0.95f;       // How quickly momentum slows down (0-1)
    public float maxAngularVelocity = 800f;   // Maximum rotation speed
    
    [Header("Input")]
    public string horizontalInputAxis = "Mouse X"; // Use "Horizontal" for keyboard/gamepad
    public bool useRawInput = true;           // Use GetAxisRaw instead of GetAxis
    
    // Internal variables
    private float currentAngle = 0f;
    private float angularVelocity = 0f;
    private bool isSpinning = false;
    private Transform player;
    
    void Start()
    {
        Debug.Log("SwordOrbitController started - INPUT BASED");
        player = PlayerManager.Instance.GetPlayer();
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
        // Only accept input while mouse button is held
        input = -(useRawInput ? 
            Input.GetAxisRaw(horizontalInputAxis) : 
            Input.GetAxis(horizontalInputAxis));
    }
    // When not holding mouse button, input remains 0 and momentum decay takes over
    
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

    // Calculate direction from player to sword
    Vector3 outwardDirection = transform.position - new Vector3(player.position.x, transform.position.y, player.position.z);
    
    if (outwardDirection.magnitude > 0.001f) // Check if the direction is not too small
    {
        outwardDirection.Normalize();

        // Create a rotation that points the sword's up direction outward from the player
        // and keeps it vertical
        Vector3 swordForward = Vector3.Cross(outwardDirection, Vector3.up).normalized;
        if (swordForward.magnitude < 0.001f)
        {
            // If cross product is too small, use a default forward direction
            swordForward = Vector3.forward;
        }

        Vector3 swordRight = Vector3.Cross(Vector3.up, swordForward).normalized;
        
        // Create and apply the rotation
        transform.rotation = Quaternion.LookRotation(swordForward, outwardDirection);
        transform.Rotate(0, 90, 0, Space.Self);
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
    

    

}