using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    [SerializeField] protected Camera mainCamera;
    [SerializeField] protected Rigidbody rb;

    [Header("Movement Type")]
    [SerializeField] protected bool useMouseTracking = true;
    [SerializeField] protected bool useConstantOrbit = false;
    [SerializeField] protected bool useSelfRotation = false;

    [Header("Orbit Settings")]
    public float orbitDistance = 2f;
    public float hiltOffset = 2f;
    public float rotationSpeed = 5f;      // For mouse tracking
    public float orbitSpeed = 100f;       // For constant orbit
    public float spinSpeed = 360f;        // For self rotation
    [SerializeField] protected float heightOffset = 1f;

    [Header("Weapon Orientation")]
    [SerializeField] protected TipAxis tipAxis = TipAxis.Up;
    [SerializeField] protected bool invertTip = false;
    [SerializeField] [Range(0f, 360f)] protected float additionalRotation = 0f;

    public enum TipAxis 
    { 
        Up, 
        Forward, 
        Right 
    }

    protected float currentAngle = 0f;
    protected float targetAngle = 0f;
    protected float orbitAngle = 0f;
    protected Quaternion tipRotationOffset;
    protected bool isDead = false;

    protected virtual void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
            
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
            
        rb.isKinematic = true;
        rb.useGravity = false;
            
        UpdateTipRotation();
    }

    protected virtual void Update()
    {
        if (isDead || player == null) return;

        if (PlayerHealth.isPlayerDead && !isDead)
        {
            HandleDeath();
            return;
        }

        UpdateTipRotation();

        Vector3 orbitPosition;
        Quaternion finalRotation;

        if (useMouseTracking)
        {
            HandleMouseTracking(out orbitPosition, out finalRotation);
        }
        else if (useConstantOrbit)
        {
            HandleConstantOrbit(out orbitPosition, out finalRotation);
        }
        else
        {
            // Default to fixed position (for wand-like weapons)
            orbitPosition = player.position + (player.right * hiltOffset) + (Vector3.up * heightOffset);
            finalRotation = HandleMouseAiming(orbitPosition);
        }

        // Apply position and rotation
        transform.position = orbitPosition;
        
        if (useSelfRotation)
        {
            // Add self-rotation on top of the base rotation
            Quaternion spinRotation = Quaternion.Euler(0, 0, spinSpeed * Time.time);
            finalRotation *= spinRotation;
        }

        transform.rotation = finalRotation;
    }

    protected virtual void HandleMouseTracking(out Vector3 position, out Quaternion rotation)
    {
        Plane characterPlane = new Plane(Vector3.up, player.position);
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (characterPlane.Raycast(ray, out float distance))
        {
            Vector3 lookAtPoint = ray.GetPoint(distance);
            Vector3 directionToMouse = (lookAtPoint - player.position).normalized;
            targetAngle = Mathf.Atan2(directionToMouse.z, directionToMouse.x) * Mathf.Rad2Deg;
        }

        currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * rotationSpeed);

        float radians = currentAngle * Mathf.Deg2Rad;
        position = player.position + new Vector3(
            Mathf.Cos(radians),
            heightOffset,
            Mathf.Sin(radians)
        ) * orbitDistance;

        Quaternion baseRotation = Quaternion.LookRotation(position - player.position);
        rotation = baseRotation * tipRotationOffset * Quaternion.Euler(0, additionalRotation, 0);
    }

    protected virtual void HandleConstantOrbit(out Vector3 position, out Quaternion rotation)
    {
        orbitAngle += orbitSpeed * Time.deltaTime;
        orbitAngle %= 360f;

        float radians = orbitAngle * Mathf.Deg2Rad;
        position = player.position + new Vector3(
            Mathf.Cos(radians),
            heightOffset,
            Mathf.Sin(radians)
        ) * orbitDistance;

        // For constant orbit, we can use a horizontal base rotation
        rotation = Quaternion.Euler(90, 0, 0);
    }

    protected virtual void UpdateTipRotation()
{
    switch (tipAxis)
    {
        case TipAxis.Up:
            tipRotationOffset = Quaternion.Euler(invertTip ? 90 : -90, 0, 0);
            break;
        case TipAxis.Forward:
            tipRotationOffset = Quaternion.identity;
            break;
        case TipAxis.Right:
            tipRotationOffset = Quaternion.Euler(0, 0, invertTip ? -90 : 90);
            break;
    }
}


    protected virtual Quaternion HandleMouseAiming(Vector3 position)
    {
        Plane characterPlane = new Plane(Vector3.up, player.position);
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (characterPlane.Raycast(ray, out float distance))
        {
            Vector3 lookAtPoint = ray.GetPoint(distance);
            Vector3 direction = (lookAtPoint - position).normalized;
            return Quaternion.LookRotation(direction) * tipRotationOffset * 
                   Quaternion.Euler(0, additionalRotation, 0);
        }

        return transform.rotation;
    }

    protected virtual void HandleDeath()
    {
        isDead = true;
        
        rb.isKinematic = false;
        rb.useGravity = true;
        
        rb.AddTorque(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f), ForceMode.Impulse);
        rb.AddForce((Vector3.up + transform.forward) * 5f, ForceMode.Impulse);
    }
}