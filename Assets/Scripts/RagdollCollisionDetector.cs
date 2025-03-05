using UnityEngine;

public class RagdollCollisionDetector : MonoBehaviour
{
    [SerializeField] private static float enemyDamageAmount = 15;
    [SerializeField] private static float playerDamageAmount = 300;
    private PlayerHealth playerHealth;

    private void Start()
    {
        playerHealth = FindAnyObjectByType<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealth script not found in scene!");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Enemy hitting player
        if (gameObject.CompareTag("EnemyRagdoll") && collision.gameObject.CompareTag("PlayerRagdoll"))
        {
            if (playerHealth != null && playerHealth.CanTakeDamage())
            {
                Debug.Log($"Enemy dealing {enemyDamageAmount} damage to player");
                playerHealth.TakeDamage(enemyDamageAmount);
            }
        }

        // Player weapon hitting enemy
        if (gameObject.CompareTag("PlayerWeapon") && collision.gameObject.CompareTag("EnemyRagdoll"))
        {
            EnemyRagdollAI EnemyRagdollAI = collision.gameObject.GetComponentInParent<EnemyRagdollAI>();
            if (EnemyRagdollAI != null)
            {
                Debug.Log($"Player weapon dealing {playerDamageAmount} damage to enemy");
                EnemyRagdollAI.TakeDamage(playerDamageAmount);
            }
        }
    }
}