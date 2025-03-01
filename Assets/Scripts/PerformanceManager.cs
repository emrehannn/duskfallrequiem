using UnityEngine;

public class PerformanceManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerPelvis;
    [SerializeField] private VirtualEnemyManager spawnSystem;
    
    [Header("Spawn Settings")]
    [SerializeField] private float realEnemyRange = 38f;  // When to convert virtual to real
    [SerializeField] private LayerMask groundLayer;

    private void Start()
    {
        if (playerPelvis == null || spawnSystem == null)
        {
            Debug.LogError("PerformanceManager: Missing references!");
            enabled = false;
            return;
        }
    }

    private void FixedUpdate()
    {
        CheckVirtualEnemies();
    }

    private void CheckVirtualEnemies()
    {
        var activeEnemies = spawnSystem.GetActiveEnemies();

        foreach(var virtualEnemy in activeEnemies)
        {
            if(!virtualEnemy.isReal)
            {
                float distanceToPlayer = Vector3.Distance(virtualEnemy.position, playerPelvis.position);
                
                if(distanceToPlayer < realEnemyRange)
                {
                    SpawnRealEnemy(virtualEnemy);
                }
            }
        }
    }

    private void SpawnRealEnemy(VirtualEnemy virtualEnemy)
    {
        // Make sure we spawn on ground
        RaycastHit hit;
        Vector3 rayStart = virtualEnemy.position + Vector3.up * 100f;
        if (Physics.Raycast(rayStart, Vector3.down, out hit, 200f, groundLayer))
        {
            // Get prefab and spawn
            GameObject[] prefabs = spawnSystem.GetEnemyPrefabs();
            GameObject enemy = Instantiate(
                prefabs[virtualEnemy.enemyTypeIndex], 
                hit.point + Vector3.up * 0.5f, 
                Quaternion.identity
            );

            // Set health based on current wave
            float health = spawnSystem.GetEnemyHealth(virtualEnemy.enemyTypeIndex);
            virtualEnemy.gameObject = enemy;
            var enemyAI = enemy.GetComponent<EnemyRagdollAI>();
            if(enemyAI != null)
            {
                enemyAI.SetHealth(health);
            }

            // Mark as real
            virtualEnemy.isReal = true;
            
        }
    }

}