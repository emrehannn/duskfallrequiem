using UnityEngine;
using System.Collections.Generic;

public class VirtualEnemyManager : MonoBehaviour 
{
    [Header("Enemy Stats")]
    [SerializeField] private EnemyStats[] enemyStats;

    [Header("Spawn Settings")]
    [SerializeField] private float waveInterval = 80f;
    [SerializeField] private float baseSpawnInterval = 2f;
    [SerializeField] private Vector2 mapSize = new Vector2(100f, 100f);
    [SerializeField] private float minDistanceFromPlayer = 20f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private int maxEnemies = 1000;
    [SerializeField] private ChaoticWaveManager chaosManager;

    private int currentWave = 0;
    private int currentEnemyIndex = 0;
    private float currentSpawnInterval;
    private List<VirtualEnemy> activeEnemies = new List<VirtualEnemy>();
    private Camera mainCamera;
    private float nextWaveTime;
    private float nextSpawnTime;
    private bool regularSpawningEnabled = true;

    private void OnDrawGizmos()
    {

        // Draw virtual enemies
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            foreach (var enemy in activeEnemies)
            {
                if (!enemy.isReal)
                {
                    Gizmos.DrawWireSphere(enemy.position, 0.5f);
                }
            }
        }
    }

    public float GetWaveInterval() { return waveInterval; }
    public int GetMaxEnemies() { return maxEnemies; }
    public int GetCurrentEnemyIndex() { return currentEnemyIndex; }
    public Vector3 GetSpawnPoint() { return GetValidSpawnPoint(); }
    public GameObject[] GetEnemyPrefabs()
{
    if (enemyPrefabs == null || enemyPrefabs.Length == 0)
    {
        Debug.LogError("Attempting to access null or empty enemy prefabs array");
        return new GameObject[0]; // Return empty array instead of null
    }
    return enemyPrefabs;
}
    public List<VirtualEnemy> GetActiveEnemies() { return activeEnemies; }


    private void Start()
    {
        mainCamera = Camera.main;
        nextWaveTime = Time.time + waveInterval;
        currentSpawnInterval = baseSpawnInterval;

        if (enemyStats == null || enemyStats.Length != enemyPrefabs.Length)
        {
            Debug.LogError("Enemy stats array must match enemy prefabs array!");
            enemyStats = new EnemyStats[enemyPrefabs.Length];
            for (int i = 0; i < enemyPrefabs.Length; i++)
            {
                enemyStats[i] = new EnemyStats();
            }
        }

        if (enemyPrefabs == null || enemyPrefabs.Length == 0)

    {

        Debug.LogError("Enemy prefabs array is null or empty!");

        enabled = false;

        return;

    }
    
    Debug.Log($"Initialized with {enemyPrefabs.Length} enemy prefabs:");

    for (int i = 0; i < enemyPrefabs.Length; i++)

    {

        if (enemyPrefabs[i] == null)

        {

            Debug.LogError($"Null prefab at index {i}");

        }

        else

        {

            Debug.Log($"Prefab {i}: {enemyPrefabs[i].name}");

        }

    }




    }

    public void DisableRegularSpawning()
    {
        regularSpawningEnabled = false;
    }

private void Update()
{
    Transform playerTransform = PlayerManager.Instance.GetPlayer();
    if (playerTransform == null || PlayerHealth.isPlayerDead) return;

    UpdateVirtualEnemies();
    CheckStuckEnemies();

    if (regularSpawningEnabled)
    {
        if (Time.time >= nextWaveTime)
        {
            StartNewWave();
        }

        if (Time.time >= nextSpawnTime && activeEnemies.Count < maxEnemies)
        {
            SpawnEnemy();
        }
    }
    CleanDeadEnemies();
}

        private void UpdateVirtualEnemies()
{
    Transform playerTransform = PlayerManager.Instance.GetPlayer();
    if (playerTransform == null) return;

    foreach(var enemy in activeEnemies)
    {
        if(!enemy.isReal)
        {
            enemy.UpdatePosition(playerTransform.position);
        }
    }
}

private Vector3 GetValidSpawnPoint()
{
    Transform playerTransform = PlayerManager.Instance.GetPlayer();
    if (playerTransform == null) return Vector3.zero;

    for (int i = 0; i < 30; i++)
    {
        float randomX = transform.position.x + Random.Range(-mapSize.x/2, mapSize.x/2);
        float randomZ = transform.position.z + Random.Range(-mapSize.y/2, mapSize.y/2);
        
        Vector3 randomPoint = new Vector3(randomX, 0f, randomZ);
        
        float distToPlayer = Vector3.Distance(
            new Vector3(randomPoint.x, 0f, randomPoint.z), 
            new Vector3(playerTransform.position.x, 0f, playerTransform.position.z)
        );
        
        if (distToPlayer < minDistanceFromPlayer)
            continue;
                
        Vector3 rayStart = randomPoint + Vector3.up * 100f;
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 200f, groundLayer))
        {
            if (!IsVisibleToCamera(hit.point))
            {
                return hit.point;
            }
        }
    }
    
    return Vector3.zero;
}


public Transform GetPlayer()
{
    return PlayerManager.Instance.GetPlayer();
}

    private void StartNewWave()
{
    currentWave++;
    Debug.Log($"Starting Wave {currentWave}. Current Enemy Index: {currentEnemyIndex}. Total Prefabs: {enemyPrefabs.Length}");

    // Fix the wave increment logic
    if (currentWave % 3 == 0 && currentEnemyIndex < enemyPrefabs.Length - 1) // Changed from % 3 == 1
    {
        currentEnemyIndex++;
        Debug.Log($"Increasing enemy index to: {currentEnemyIndex}");
    }

    // Only check chaos if chaosManager is assigned
    if (chaosManager != null)
    {
        chaosManager.CheckForChaos(currentWave);
    }
    
    nextWaveTime = Time.time + waveInterval;
    currentSpawnInterval *= 0.7f;
    currentSpawnInterval = Mathf.Max(currentSpawnInterval, 0.05f);
}

private void SpawnEnemy()
{
    if (activeEnemies.Count >= maxEnemies)
    {
        Debug.Log($"Spawn failed: activeEnemies={activeEnemies.Count}, maxEnemies={maxEnemies}");
        return;
    }

    nextSpawnTime = Time.time + currentSpawnInterval;

    Vector3 spawnPoint = GetValidSpawnPoint();
    if (spawnPoint != Vector3.zero)
    {

        VirtualEnemy enemy = new VirtualEnemy(spawnPoint, currentEnemyIndex);
        activeEnemies.Add(enemy);
    }
    else
    {
        Debug.LogWarning("Failed to find valid spawn point");
    }
}
    public void SpawnSpecificEnemy(int enemyIndex, Vector3 position)
    {
        if (enemyIndex < 0 || enemyIndex >= enemyPrefabs.Length) return;
        
        VirtualEnemy enemy = new VirtualEnemy(position, enemyIndex);
        activeEnemies.Add(enemy);
    }

    private float CalculateEnemyHealth(int enemyIndex)
    {
        EnemyStats stats = enemyStats[enemyIndex];
        return stats.baseHealth + (stats.healthIncreasePerWave * (currentWave - 1));
    }

    public float GetEnemyHealth(int enemyIndex)
    {
        return CalculateEnemyHealth(enemyIndex);
    }

    private bool IsVisibleToCamera(Vector3 position)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
        Bounds bounds = new Bounds(position, Vector3.one);
        return GeometryUtility.TestPlanesAABB(planes, bounds);
    }

    private void CheckStuckEnemies()
    {
        List<VirtualEnemy> enemiesToRemove = new List<VirtualEnemy>();

        float minX = transform.position.x - mapSize.x/2;
        float maxX = transform.position.x + mapSize.x/2;
        float minZ = transform.position.z - mapSize.y/2;
        float maxZ = transform.position.z + mapSize.y/2;

        foreach (var enemy in activeEnemies)
        {
            if(!enemy.isReal && (
                enemy.position.x < minX || enemy.position.x > maxX || 
                enemy.position.z < minZ || enemy.position.z > maxZ))
            {
                enemiesToRemove.Add(enemy);
            }
        }

        foreach (var enemy in enemiesToRemove)
        {
            activeEnemies.Remove(enemy);
        }
    }

    public int GetActiveEnemyCount()
    {
        return activeEnemies.Count;
    }

    public int GetCurrentWave()
    {
        return currentWave;
    }

private void OnGUI()

{

    // Define style for better visibility

    GUIStyle style = new GUIStyle();

    style.normal.textColor = Color.white;

    style.fontSize = 20;


    // Draw stats in top left corner

    GUI.Label(new Rect(20, 20, 300, 20), $"Active Enemies: {GetActiveEnemyCount()}", style);

    GUI.Label(new Rect(20, 35, 300, 20), $"Wave: {currentWave}", style);

    GUI.Label(new Rect(20, 60, 300, 20), $"Enemy Type: {currentEnemyIndex + 1}", style);

}

private void CleanDeadEnemies()

{

    int beforeCount = activeEnemies.Count;

    

    activeEnemies.RemoveAll(enemy => 

        enemy == null || 

        (enemy.gameObject != null && enemy.gameObject.layer == LayerMask.NameToLayer("DeadEnemy"))

    );




}





}

[System.Serializable]
public class EnemyStats
{
    public float baseHealth = 100f;
    public float healthIncreasePerWave = 10f;
}

