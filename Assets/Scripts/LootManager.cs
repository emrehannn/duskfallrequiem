using UnityEngine;

public class LootManager : MonoBehaviour
{
    [System.Serializable]
    public class EnemyLootData
    {
        public int enemyIndex;
        public int baseXP;
        public float healthItemDropChance = 0.05f; // 5% chance
    }

    [Header("Prefabs")]
    [SerializeField] private GameObject applePrefab;
    [SerializeField] private GameObject xpOrbPrefab;
    
    private const float HEALTH_DROP_CHANCE = 0.05f; // Moved this to a constant
    private VirtualEnemyManager enemyManager;

    private void Start()
    {
        enemyManager = FindFirstObjectByType<VirtualEnemyManager>(); // Updated to new method

        if (enemyManager != null)
        {
            int currentEnemyIndex = enemyManager.GetCurrentEnemyIndex();
        }
        else
        {
            Debug.LogWarning("Enemy Manager not found!");
        }
    }

    private int CalculateXPForIndex(int index)
    {
        switch (index)
        {
            case 0:
                return 20;  // First enemy type
            case 1:
                return 90;  // Second enemy type
            default:
                if (enemyManager != null)
                {
                    return enemyManager.GetCurrentWave() * 10;
                }
                return 100; // Default fallback value
        }
    }

    public void OnEnemyDeath(Vector3 deathPosition, int enemyIndex)
    {
        // Calculate the XP amount using your wave-based logic.
        float xpAmount = CalculateXPForIndex(enemyIndex);

        // Spawn an XP orb that holds this XP value.
        SpawnXPOrb(deathPosition, xpAmount);

        // Drop a health item based on the drop chance.
        if (Random.value < HEALTH_DROP_CHANCE)
        {
            SpawnHealthItem(deathPosition);
        }
    }

    private void SpawnXPOrb(Vector3 position, float xpAmount)
    {
        if (xpOrbPrefab != null)
        {
            GameObject xpOrb = Instantiate(xpOrbPrefab, position + (Vector3.up * -0.2f), Quaternion.identity);
            XPOrb xpOrbScript = xpOrb.GetComponent<XPOrb>();
            if (xpOrbScript != null)
            {
                xpOrbScript.SetXPValue(xpAmount);
            }
            Debug.Log("XP orb spawned!");
        }
        else
        {
            Debug.LogError("XP Orb Prefab is not assigned!");
        }
    }

    private void SpawnHealthItem(Vector3 position)
    {
        if (applePrefab != null)
        {
            Vector3 spawnPosition = position + Vector3.up * 0.5f;
            GameObject apple = Instantiate(applePrefab, spawnPosition, Quaternion.identity);
            Debug.Log("Apple dropped!");
        }
    }
}