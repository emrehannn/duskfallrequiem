using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private float damageInvulnerabilityTime = 1f;
    [SerializeField] private Rigidbody hipBone;
    [SerializeField] private Rigidbody backBone;
    [SerializeField] private Rigidbody headBone;

    [Header("UI")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Image healthBarLip;
    private float smoothUpdateSpeed = 5f;
    private float lipOffset = 0.3f;  
    private float nextDamageTime;
    private float targetHealthBarFill;
    private float targetLipFill;
    public Rigidbody HipBone => hipBone;

    private void Start()
    {
        currentHealth = maxHealth;
        Debug.Log($"Player health initialized. Current health: {currentHealth}");
        targetHealthBarFill = 1f;
        targetLipFill = targetHealthBarFill + lipOffset;

    // Set player target using hipBone
    if (hipBone != null)
    {
        PlayerManager.Instance.SetPlayer(hipBone.transform);
        Debug.Log("Set player in PlayerManager using hipBone");
    }
    else
    {
        PlayerManager.Instance.SetPlayer(transform);
        Debug.Log("Set player in PlayerManager using transform");
    }

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
        Debug.Log($"Took {damage} damage. Current health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public static bool isPlayerDead = false;

    private void Die()
    {
        Debug.Log("Player died!");
        isPlayerDead = true;

        RagdollController ragdollController = GetComponentInParent<RagdollController>();
        if (ragdollController != null)
        {
            ragdollController.HandleDeath();
        }
        else
        {
            Debug.LogError("RagdollController script not found in parent hierarchy!");
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