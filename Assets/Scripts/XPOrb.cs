using UnityEngine;

public class XPOrb : MonoBehaviour
{
    [Header("XP Settings")]
    public float xpValue = 0f; // This will be set by LootManager

    [Header("Movement Settings")]
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float floatHeight = 0.3f;
    
    [Header("Collection Settings")]
    [SerializeField] private float lifetime = 30f;
    [SerializeField] private float magnetDistance = 5f;
    [SerializeField] private float magnetSpeed = 15f;

    private Vector3 startPosition;
    private bool isBeingCollected = false;
    private bool hasBeenCollected = false;  // Add this flag for trigger detection

    private void Start()
    {
        startPosition = transform.position;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!isBeingCollected)
        {
            // Rotate and float
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
            float newY = startPosition.y + (Mathf.Sin(Time.time * floatSpeed) * floatHeight);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            // Check for player proximity
            PlayerHealth player = FindObjectOfType<PlayerHealth>();
            if (player != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
                if (distanceToPlayer < magnetDistance)
                {
                    isBeingCollected = true;
                }
            }
        }
        else
        {
            // Move towards player
            PlayerHealth player = FindObjectOfType<PlayerHealth>();
            if (player != null)
            {
                Vector3 direction = (player.transform.position - transform.position).normalized;
                transform.position += direction * magnetSpeed * Time.deltaTime;
            }
        }
    }

    public void SetXPValue(float value)
    {
        xpValue = value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenCollected) return;  // Skip if already collected
        
        if (other.CompareTag("PlayerRagdoll"))
        {
            hasBeenCollected = true;  // Mark as collected
            
            // Disable collider immediately
            Collider myCollider = GetComponent<Collider>();
            if (myCollider != null) myCollider.enabled = false;
            
            LevelSystem levelSystem = FindObjectOfType<LevelSystem>();
            if (levelSystem != null)
            {
                levelSystem.GainXP(xpValue);
                Debug.Log($"Added {xpValue} XP to player");
            }
            
            Destroy(gameObject);  // Only destroy once
        }
    }
}