using UnityEngine;

public class ChaoticWaveManager : MonoBehaviour
{
    [SerializeField] private VirtualEnemyManager spawnSystem;
    [SerializeField] private float spawnInterval = 0.1f;
    private const int MAX_ENEMIES = 600;

    private GameObject[] enemyPrefabs;
    private int dominantEnemyIndex;
    private float nextSpawnTime;
    private float nextWaveTime;
    private bool isActive = false;

    private void Start()
    {
        enemyPrefabs = spawnSystem.GetEnemyPrefabs();
        this.enabled = false;

        // Calculate when chaos should start
        int chaosStartWave = enemyPrefabs.Length * 3;
        Debug.Log($"Chaos mode will activate on wave {chaosStartWave}");
    }

    public void CheckForChaos(int currentWave)
    {
        if (currentWave >= enemyPrefabs.Length * 3 && !isActive)
        {
            StartChaos();
        }
    }

    private void StartChaos()
{
    spawnSystem.DisableRegularSpawning();  // Instead of disabling the whole script
    this.enabled = true;
    isActive = true;
    nextWaveTime = Time.time + 60f;
    dominantEnemyIndex = Random.Range(0, enemyPrefabs.Length);
    Debug.Log($"CHAOS MODE ACTIVATED! First dominant enemy: {enemyPrefabs[dominantEnemyIndex].name}");
}

    private void Update()
    {
        if (!isActive) return;

        // Check for new wave every 60 seconds
        if (Time.time >= nextWaveTime)
        {
            dominantEnemyIndex = Random.Range(0, enemyPrefabs.Length);
            nextWaveTime = Time.time + 60f;
            Debug.Log($"New chaos wave! Dominant enemy: {enemyPrefabs[dominantEnemyIndex].name}");
        }

        // Regular spawning
        if (Time.time >= nextSpawnTime && spawnSystem.GetActiveEnemyCount() < MAX_ENEMIES)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    private void SpawnEnemy()
    {
        Vector3 spawnPoint = spawnSystem.GetSpawnPoint();
        if (spawnPoint == Vector3.zero) return;

        // 80% dominant, 20% random
        int enemyIndex;
        if (Random.value < 0.8f)
        {
            enemyIndex = dominantEnemyIndex;
        }
        else
        {
            // Make sure random enemy isn't the same as dominant
            do
            {
                enemyIndex = Random.Range(0, enemyPrefabs.Length);
            } while (enemyIndex == dominantEnemyIndex && enemyPrefabs.Length > 1);
        }

        spawnSystem.SpawnSpecificEnemy(enemyIndex, spawnPoint);
    }
}