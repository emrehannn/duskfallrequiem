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
    [SerializeField] private float magnetDistance = 3f;
    [SerializeField] private float magnetSpeed = 2f;

    private Vector3 startPosition;
    private bool isBeingCollected = false;
    private bool hasBeenCollected = false;

    private Rigidbody hipBoneTarget; // Use the hipBone Rigidbody from RagdollMovement

    private void Start()
    {
        startPosition = transform.position;
        Destroy(gameObject, lifetime);
        
        // Initial target search
        FindHipBoneTarget();
    }

    private void Update()
    {
        if (!isBeingCollected)
        {
            // Float animation remains the same
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
            float newY = startPosition.y + (Mathf.Sin(Time.time * floatSpeed) * floatHeight);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            // Only search for target periodically for efficiency
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
                // Smoothly move toward the hip bone using Lerp
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
    if (ragdollMovement != null)
    {
        hipBoneTarget = ragdollMovement.HipBone; // Access via property
    }
    else
    {
        hipBoneTarget = null;
    }
}

    public void SetXPValue(float value)
    {
        xpValue = value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenCollected) return;
        
        // Allow collection by the player's hip bone
        if (other.CompareTag("PlayerRagdoll") || other.CompareTag("Player"))
        {
            hasBeenCollected = true;
            Collider myCollider = GetComponent<Collider>();
            if (myCollider != null) myCollider.enabled = false;
            
            LevelSystem levelSystem = FindAnyObjectByType<LevelSystem>();
            if (levelSystem != null) levelSystem.GainXP(xpValue);
            
            Destroy(gameObject);
        }
    }
}