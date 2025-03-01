using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSystem : MonoBehaviour
{
    [Header("XP Settings")]
    [SerializeField] private float currentXP = 0f;
    [SerializeField] private int currentLevel = 1;
    
    [Header("UI")]
    [SerializeField] private Image xpBarFill;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private float smoothUpdateSpeed = 5f;

    private float targetXPBarFill;
    private float requiredXP;

    private void Start()
    {
        // Find the XP bar if not assigned
        if (xpBarFill == null)
        {
            xpBarFill = transform.Find("Canvas/XpMask/XPBar").GetComponent<Image>();
        }

        // Initialize XP bar to empty
        if (xpBarFill != null)
        {
            xpBarFill.fillAmount = 0f;
            targetXPBarFill = 0f;
        }
        
        UpdateRequiredXP();
        UpdateUI();
    }

    private void Update()
    {
        if (xpBarFill != null)
        {
            // Smooth XP bar fill update
            xpBarFill.fillAmount = Mathf.Lerp(xpBarFill.fillAmount, targetXPBarFill, Time.deltaTime * smoothUpdateSpeed);
        }
    }

    private void UpdateRequiredXP()
    {
        if (currentLevel <= 20)
        {
            requiredXP = 50 + (currentLevel - 1) * 100;
        }
        else if (currentLevel <= 40)
        {
            requiredXP = 1950 + (currentLevel - 20) * 130;
        }
        else
        {
            requiredXP = 4550 + (currentLevel - 40) * 160;
        }
    }

    public void GainXP(float amount)
    {
        currentXP += amount;
        
        while (currentXP >= requiredXP && currentLevel < 99)
        {
            currentXP -= requiredXP;
            currentLevel++;
            UpdateRequiredXP();
            OnLevelUp();
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        // Calculate the fill amount based on current XP progress
        targetXPBarFill = Mathf.Clamp01(currentXP / requiredXP);
        
        if (levelText != null)
        {
            levelText.text = $"Level {currentLevel}";
        }

        Debug.Log($"Level: {currentLevel}, XP: {currentXP}/{requiredXP}, Fill: {targetXPBarFill}");
    }

    private void OnLevelUp()
    {
        Debug.Log($"Level Up! Now level {currentLevel}");
        // Add any level up effects here (particles, sounds, etc.)
    }

    // Getter methods
    public int GetCurrentLevel() { return currentLevel; }
    public float GetCurrentXP() { return currentXP; }
    public float GetRequiredXP() { return requiredXP; }
    public float GetXPProgress() { return currentXP / requiredXP; }
}