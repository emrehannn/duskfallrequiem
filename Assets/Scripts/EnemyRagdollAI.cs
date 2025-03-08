using UnityEngine;

public class EnemyRagdollAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxSpeed = 3f; // Change from private to public
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float rotationSpeed = 6f;
    [SerializeField] private float retreatDistance = 20f;

    public int enemyIndex;

    [Header("Height Settings")]
    [SerializeField] private float minHeightThreshold = 0.5f;
    [SerializeField] private float heightCorrectionForce = 20f;
    [SerializeField] private LayerMask groundLayer;

    [Header("References")]
    [SerializeField] private Rigidbody hipBone;
    [SerializeField] private Rigidbody backBone;
    [SerializeField] private Rigidbody headBone;
    [Header("Spring Settings")]
    [SerializeField] private float enemySpecificStiffness = 400f;    
    [SerializeField] private float enemySpecificDamping = 100f;



private float DeadrotationSpeed = 2f;  // Add this at the top with your other variables

private float retreatSpeed = 2f;  // Add this too - lower value = slower retreat

    public  Vector3 moveDirection;
    private float currentGroundDistance;
    private bool isMoving;
    private Vector3 playerDeathPosition;
    private float health;

        private void Start()
    {
        // Add these two crucial initialization calls
   
        InitializeRagdoll();
    }



    public void SetHealth(float newHealth)
    {
        health = newHealth;
    }

    [Header("Combat Settings")]
    [SerializeField] private float damageInvulnerabilityTime = 0.5f; // Adjust this value to control damage frequency
    private float nextDamageTime;

    // Modify the TakeDamage method
    public void TakeDamage(float damage)
    {
        // Check if can take damage
        if (Time.time < nextDamageTime) return;

        // Set next damage time
        nextDamageTime = Time.time + damageInvulnerabilityTime;

        // Apply damage
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

private void Die()
{
    // Get the layer number for "DeadEnemy"
    int deadEnemyLayer = LayerMask.NameToLayer("DeadEnemy");

    // Change the layer of the main GameObject and all its children
    gameObject.layer = deadEnemyLayer;
    foreach (Transform child in GetComponentsInChildren<Transform>())
    {
        child.gameObject.layer = deadEnemyLayer;
    }

    // Release all constraints on the hip bone
    if (hipBone != null)
    {
        hipBone.constraints = RigidbodyConstraints.None;
    }

    // Get all rigidbodies and colliders in the ragdoll
    Rigidbody[] allBones = GetComponentsInChildren<Rigidbody>();
    Collider[] allColliders = GetComponentsInChildren<Collider>();

    // Make them all just ragdoll pieces
    foreach (Rigidbody rb in allBones)
    {

        rb.useGravity = true;
        rb.isKinematic = false;
    }

    // Change all colliders to ignore the PlayerRagdoll layer to stop dealing damage
    foreach (Collider col in allColliders)
    {
        col.tag = "Untagged";  // Remove the EnemyRagdoll tag
    }

    // Disable this script
    this.enabled = false;

    // Optional: Add force to make the death more dramatic
    if (hipBone != null)
    {
        hipBone.AddForce(Vector3.up * 2f, ForceMode.Impulse);
    }

    LootManager lootManager = FindObjectOfType<LootManager>();

    if (lootManager != null)

    {

        Vector3 dropPosition = (hipBone != null) ? hipBone.position : transform.position;
        lootManager.OnEnemyDeath(dropPosition, enemyIndex);

    }
    

    // Optional: Destroy the gameObject after a delay to show the ragdoll effect
    Destroy(gameObject, 5f);  
}

    


    private void InitializeRagdoll()
    {
        if (hipBone != null)
        {
            hipBone.constraints = RigidbodyConstraints.FreezeRotation;
            hipBone.mass = 3f;
            hipBone.linearDamping = 0.01f;
            hipBone.interpolation = RigidbodyInterpolation.Interpolate;
        }

        if (backBone != null)
        {
            backBone.useGravity = true;
            backBone.mass = 1f;
        }

        if (headBone != null)
        {
            headBone.useGravity = true;
            headBone.mass = 1f;
        }
    }

    private void Update()

{

    if (!PlayerHealth.isPlayerDead)

    {
            
            ChasePlayer();

            HandleRotation();


    }
    else
    {
        if (!isMoving && PlayerManager.Instance.GetPlayer() != null)
        {
            playerDeathPosition = PlayerManager.Instance.GetPlayer().position;
            isMoving = true;
        }

        RetreatFromDeath();
        HandleRetreatRotation();
    }

}

    
private void ChasePlayer()
{
    Transform playerTransform = PlayerManager.Instance.GetPlayer();
    if (playerTransform == null) return;

    // Always chase, no distance checks
    isMoving = true;
    Vector3 directionToPlayer = (playerTransform.position - hipBone.position).normalized;
    directionToPlayer.y = 0;
    moveDirection = directionToPlayer;


}

    private void RetreatFromDeath()

{

    float distanceToDeathPoint = Vector3.Distance(hipBone.position, playerDeathPosition);


    if (distanceToDeathPoint < retreatDistance)

    {

        isMoving = true;

        Vector3 directionFromDeath = (hipBone.position - playerDeathPosition).normalized;

        directionFromDeath.y = 0;

        

        // Smoothly interpolate directly to moveDirection

        moveDirection = Vector3.Lerp(moveDirection, directionFromDeath * retreatSpeed, Time.deltaTime * rotationSpeed);

    }

    else

    {

        isMoving = false;

        moveDirection = Vector3.zero;

    }

}

    private void HandleRotation()
{
    Transform playerTransform = PlayerManager.Instance.GetPlayer();
    if (playerTransform == null) return;

    Vector3 directionToPlayer = (playerTransform.position - hipBone.position).normalized;
    directionToPlayer.y = 0;

    if (directionToPlayer != Vector3.zero)
    {
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
        hipBone.MoveRotation(Quaternion.Lerp(hipBone.rotation, targetRotation,
            Time.deltaTime * rotationSpeed));
    }
}

    private void HandleRetreatRotation()
    {
        Vector3 directionFromDeath = (hipBone.position - playerDeathPosition).normalized;
        directionFromDeath.y = 0;

        if (directionFromDeath != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionFromDeath);
            hipBone.MoveRotation(Quaternion.Lerp(hipBone.rotation, targetRotation,
                Time.deltaTime * DeadrotationSpeed/2));
        }
    }

    private void FixedUpdate()
    {

        CheckGroundDistance();
        MoveCharacter();
    }

    private void CheckGroundDistance()
{
    if (hipBone == null) return;

    RaycastHit hit;
    bool isGrounded = Physics.Raycast(hipBone.position, Vector3.down, out hit, 10f, groundLayer);

    if (isGrounded)
    {
        currentGroundDistance = hit.distance;
        float heightError = minHeightThreshold - currentGroundDistance;
        float verticalVelocity = hipBone.linearVelocity.y;

        // Fetch the force using this enemy's specific values
        float totalCorrectionForce = SpringLookupTable.Instance.GetSpringForce(heightError, verticalVelocity, enemySpecificStiffness, enemySpecificDamping);

        // Apply force
        hipBone.AddForce(Vector3.up * totalCorrectionForce, ForceMode.Acceleration);
    }
}




private void MoveCharacter()
{
    if (hipBone != null)
    {
        if (isMoving)
        {
            Vector3 targetVelocity = moveDirection * maxSpeed;
            targetVelocity.y = hipBone.linearVelocity.y;
            
            hipBone.linearVelocity = Vector3.MoveTowards(
                hipBone.linearVelocity,
                targetVelocity,
                acceleration * Time.fixedDeltaTime
            );
        }
        else
        {
            Vector3 currentVel = hipBone.linearVelocity;
            currentVel.y = hipBone.linearVelocity.y;
            hipBone.linearVelocity = Vector3.MoveTowards(currentVel, Vector3.zero, acceleration * Time.fixedDeltaTime);
        }
    }
}
}