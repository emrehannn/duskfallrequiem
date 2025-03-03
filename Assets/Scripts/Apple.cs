using UnityEngine;

public class Apple : MonoBehaviour
{
    [Header("Healing Settings")]
    [SerializeField] private float healAmount = 20f;
    [SerializeField] private AudioClip healSound;

    [Header("Movement Settings")]
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float floatHeight = 0.5f;
    
    [Header("Collection Settings")]
    [SerializeField] private float lifetime = 30f;
    [SerializeField] private float magnetDistance = 5f;
    [SerializeField] private float magnetSpeed = 10f;

    private Vector3 startPosition;
    private bool isBeingCollected = false;
    private bool hasBeenCollected = false;
    private Rigidbody hipBoneTarget;

    private void Start()
    {
        startPosition = transform.position;
        Destroy(gameObject, lifetime);
        FindHipBoneTarget();
    }

    private void Update()
    {
        if (!isBeingCollected)
        {
            // Float animation
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
            float newY = startPosition.y + (Mathf.Sin(Time.time * floatSpeed) * floatHeight);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            // Periodic target check (every 30 frames)
            if (Time.frameCount % 30 == 0) FindHipBoneTarget();

            if (hipBoneTarget != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, hipBoneTarget.position);
                if (distanceToPlayer < magnetDistance)
                {
                    isBeingCollected = true;
                }
            }
        }
        else
        {
            // Update target continuously during collection
            FindHipBoneTarget();
            
            if (hipBoneTarget != null)
            {
                // Smooth magnet movement using Lerp
                transform.position = Vector3.Lerp(
                    transform.position,
                    hipBoneTarget.position,
                    magnetSpeed * Time.deltaTime
                );
            }
        }
    }

    private void FindHipBoneTarget()
    {
        ragdollMovement ragdollMovement = FindAnyObjectByType<ragdollMovement>();
        hipBoneTarget = ragdollMovement != null ? ragdollMovement.HipBone : null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenCollected) return;

        if (other.CompareTag("PlayerRagdoll") || other.CompareTag("Player"))
        {
            hasBeenCollected = true;
            Collider myCollider = GetComponent<Collider>();
            if (myCollider != null) myCollider.enabled = false;

            PlayerHealth playerHealth = FindAnyObjectByType<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Heal(healAmount);
                
                if (healSound != null)
                {
                    AudioSource.PlayClipAtPoint(healSound, transform.position);
                }
            }

            Destroy(gameObject);
        }
    }
}