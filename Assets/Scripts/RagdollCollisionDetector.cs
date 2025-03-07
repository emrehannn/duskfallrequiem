using UnityEngine;

public class RagdollCollisionDetector : MonoBehaviour
{
    [SerializeField] private float enemyDamageAmount = 15f;
    [SerializeField] private float playerDamageAmount = 10f;
    private PlayerHealth playerHealth;

    private void Start()
    {
        playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealth script not found in scene!");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"I am {gameObject.name} colliding with {collision.gameObject.name}");

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
            EnemyRagdollAI enemyRagdollAI = collision.gameObject.GetComponentInParent<EnemyRagdollAI>();
            if (enemyRagdollAI != null)
            {
                Debug.Log($"Player weapon dealing {playerDamageAmount} damage to enemy");
                enemyRagdollAI.TakeDamage(playerDamageAmount);
            }
        }
    }
}