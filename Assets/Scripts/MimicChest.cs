using UnityEngine;
using System.Collections;

public class MimicChest : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public int numberOfEnemies = 10;
    public float spawnDelay = 0.2f;
    
    [Header("Movement Settings")]
    public float spawnDepth = -2f;  // How deep under the hole to spawn
    public float upwardForce = 15f;  // Strong upward burst
    public float spreadForce = 3f;   // Horizontal spread

    private bool isActivated = false;
    private float spawnTimer = 0f;
    private int spawnCount = 0;
    private Transform chestTransform;
    private MeshRenderer meshRenderer;
    private BoxCollider chestCollider;

    private void Start()
    {
        chestTransform = transform;
        meshRenderer = GetComponent<MeshRenderer>();
        chestCollider = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActivated && other.CompareTag("PlayerWeapon"))
        {
            isActivated = true;
            if (meshRenderer != null) meshRenderer.enabled = false;
        }
    }

    private void Update()
    {
        if (isActivated && spawnCount < numberOfEnemies)
        {
            spawnTimer += Time.deltaTime;
            
            if (spawnTimer >= spawnDelay)
            {
                SpawnEnemy();
                spawnTimer = 0f;
                spawnCount++;
            }
        }
    }

    private void SpawnEnemy()
{
    // Spawn position is directly under the hole
    Vector3 spawnPos = chestTransform.position + Vector3.up * spawnDepth;
    GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

    // Get the hip bone (main rigidbody)
    EnemyRagdollAI enemyAI = enemy.GetComponent<EnemyRagdollAI>();
    Rigidbody hipBone = enemyAI.GetComponent<Rigidbody>();

    // Set higher health!
    enemyAI.SetHealth(200f);  // Or whatever value you want

    // Temporarily disable AI until physics settles
    enemyAI.enabled = false;
    StartCoroutine(EnableAIAfterDelay(enemyAI));

    // Calculate random spread direction
    Vector3 randomDir = new Vector3(
        Random.Range(-1f, 1f),
        0,
        Random.Range(-1f, 1f)
    ).normalized;

    // Apply forces to ALL rigidbodies for consistent rise
    Rigidbody[] allBones = enemy.GetComponentsInChildren<Rigidbody>();
    foreach (Rigidbody rb in allBones)
    {
        // Strong upward force + spread
        rb.linearVelocity = (Vector3.up * upwardForce) + (randomDir * spreadForce);
    }
}
    private IEnumerator EnableAIAfterDelay(EnemyRagdollAI enemyAI)
    {
        yield return new WaitForSeconds(0.1f);
        if (enemyAI != null)
        {
            enemyAI.enabled = true;
        }
    }
}