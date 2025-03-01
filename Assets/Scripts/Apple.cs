using UnityEngine;

public class Apple : MonoBehaviour
{
    [SerializeField] private float healAmount = 20f;
    [SerializeField] private AudioClip healSound;

    [Header("Movement Settings")]
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float floatHeight = 0.5f;
    
    [Header("Collection Settings")]
    [SerializeField] private float lifetime = 30f; // Apple disappears after 30 seconds
    [SerializeField] private float magnetDistance = 5f; // Distance at which apple starts moving towards player
    [SerializeField] private float magnetSpeed = 10f; // Speed at which apple moves towards player

    private Vector3 startPosition;
    private bool isBeingCollected = false;

    private void Start()
    {
        startPosition = transform.position;
        // Destroy the apple after lifetime seconds if not collected
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!isBeingCollected)
        {
            // Normal floating behavior
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
            float newY = startPosition.y + (Mathf.Sin(Time.time * floatSpeed) * floatHeight);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            // Check for player proximity (optional magnet effect)
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
            // Move towards player when in range
            PlayerHealth player = FindObjectOfType<PlayerHealth>();
            if (player != null)
            {
                Vector3 direction = (player.transform.position - transform.position).normalized;
                transform.position += direction * magnetSpeed * Time.deltaTime;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerRagdoll"))
        {
            PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Heal(healAmount);
                
                if (healSound != null)
                {
                    AudioSource.PlayClipAtPoint(healSound, transform.position);
                }
                
                Destroy(gameObject);
            }
        }
    }
}