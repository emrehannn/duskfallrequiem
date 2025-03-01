using UnityEngine;
using System.Collections;

public class WaveSurprise : MonoBehaviour
{
    [SerializeField] private VirtualEnemyManager spawnSystem;
    
    [Header("Enemy Costs")]
    [SerializeField] private int[] enemyCosts = new int[] { 1, 10, 100, 200 };
    [SerializeField] private float spawnDelay = 0.5f;

    private float nextCheckTime;
    private float checkOffset = 5f;
    private bool isSpawning = false;

    private void Start()
{
    if (spawnSystem == null)
    {
        Debug.LogError("Spawn System not assigned to WaveSurprise!");
        enabled = false;
        return;
    }

    if (enemyCosts == null || enemyCosts.Length == 0)
    {
        Debug.LogError("Enemy costs array is empty!");
        enabled = false;
        return;
    }

    // Validate enemy costs
    for (int i = 0; i < enemyCosts.Length; i++)
    {
        if (enemyCosts[i] <= 0)
        {
            Debug.LogError($"Invalid enemy cost at index {i}: {enemyCosts[i]}");
            enabled = false;
            return;
        }
    }

    float waveInterval = spawnSystem.GetWaveInterval();
    nextCheckTime = Time.time + (waveInterval - checkOffset);
}

    private void Update()
    {
        if (Time.time >= nextCheckTime && !isSpawning)
        {
            StartCoroutine(SpawnSurpriseEnemies());
            nextCheckTime = Time.time + (spawnSystem.GetWaveInterval() - checkOffset);
        }
    }

    private IEnumerator SpawnSurpriseEnemies()
{
    isSpawning = true;
    
    // Add safety checks for the budget calculation
    int maxEnemies = spawnSystem.GetMaxEnemies();
    int activeEnemies = spawnSystem.GetActiveEnemyCount();
    
    // Validate the values
    if (maxEnemies <= 0)
    {
        Debug.LogError($"Invalid maxEnemies value: {maxEnemies}");
        isSpawning = false;
        yield break;
    }
    
    if (activeEnemies < 0)
    {
        Debug.LogError($"Invalid activeEnemies count: {activeEnemies}");
        isSpawning = false;
        yield break;
    }
    
    // Calculate budget with safety check
    int remainingBudget = Mathf.Max(0, maxEnemies - activeEnemies);
    
    Debug.Log($"Starting surprise spawns - Max: {maxEnemies}, Active: {activeEnemies}, Budget: {remainingBudget}");

    // Only proceed if we have a positive budget
    if (remainingBudget <= 0)
    {
        Debug.Log("No budget available for surprise spawns");
        isSpawning = false;
        yield break;
    }

    while (remainingBudget > 0)
    {
        bool spawnedSomething = false;

        // Try to spawn highest tier possible with remaining budget
        for (int i = enemyCosts.Length - 1; i >= 0; i--)
        {
            int cost = enemyCosts[i];
            if (cost <= 0)
            {
                Debug.LogWarning($"Invalid enemy cost at index {i}: {cost}");
                continue;
            }

            if (remainingBudget >= cost)
            {
                Vector3 spawnPoint = spawnSystem.GetSpawnPoint();
                if (spawnPoint != Vector3.zero)
                {
                    spawnSystem.SpawnSpecificEnemy(i, spawnPoint);
                    Debug.Log($"Spawned tier {i} enemy (cost: {cost}), remaining budget: {remainingBudget - cost}");
                    remainingBudget -= cost;
                    spawnedSomething = true;
                    yield return new WaitForSeconds(spawnDelay);
                    break;
                }
            }
        }

        if (!spawnedSomething) break;
    }

    isSpawning = false;
}
}