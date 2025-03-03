using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private float damageInvulnerabilityTime = 1f;

    [Header("UI")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Image healthBarLip;
    private float smoothUpdateSpeed = 5f;
    private float lipOffset = 0.3f;  
    private float nextDamageTime;
    private float targetHealthBarFill;
    private float targetLipFill;

    private void Start()
    {
        currentHealth = maxHealth;
        targetHealthBarFill = 1f;
        targetLipFill = targetHealthBarFill + lipOffset;

        // Tag only this bone as PlayerRagdoll
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.tag = "PlayerRagdoll";
        }
        else
        {
            Debug.LogWarning("No collider found on the root bone. Add a collider to enable damage detection.");
        }
    }

    private void Update()
    {
        healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, targetHealthBarFill, Time.deltaTime * smoothUpdateSpeed);
        healthBarLip.fillAmount = healthBarFill.fillAmount + 0.01f;
    }

    public void TakeDamage(float damage)
    {
        if (Time.time < nextDamageTime)
        {
            Debug.Log("Damage blocked by cooldown");
            return;
        }

        nextDamageTime = Time.time + damageInvulnerabilityTime;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        
        targetHealthBarFill = currentHealth / maxHealth;
        targetLipFill = targetHealthBarFill + lipOffset;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public static bool isPlayerDead = false;

    private void Die()
    {
        Debug.Log("Player died!");
        isPlayerDead = true;  // Set the static flag

        // Handle ragdoll death
        ragdollMovement movement = GetComponentInParent<ragdollMovement>();
        if (movement != null)
        {
            movement.HandleDeath();
        }
        else
        {
            Debug.LogError("ragdollMovement script not found in parent hierarchy!");
        }
    }

    public void Heal(float amount)
    {
        Debug.Log($"Heal called. Current Health before healing: {currentHealth}");
        
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        targetHealthBarFill = currentHealth / maxHealth;
        Debug.Log($"Healed for {amount}. New health: {currentHealth}");
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    public bool CanTakeDamage()
    {
        return Time.time >= nextDamageTime;
    }
}