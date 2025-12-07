using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

/// <summary>
/// Manages the player's spiritual energy (health/resource system)
/// Losing all energy causes game over
/// </summary>
public class SpiritualEnergyManager : MonoBehaviour
{
    public static SpiritualEnergyManager Instance { get; private set; }
    
    [Header("Spiritual Energy Configuration")]
    [SerializeField] private float maxSpiritualEnergy = 100f;
    [SerializeField] private float currentSpiritualEnergy = 100f;
    [SerializeField] private float energyRegenRate = 2f; // per second
    [SerializeField] private bool allowRegen = true;
    
    [Header("Backlash Configuration")]
    [SerializeField] private float wrongRitualPenalty = 25f;
    [SerializeField] private float ritualFailurePenalty = 15f;
    [SerializeField] private float totalFailurePenalty = 35f;
    [SerializeField] private float perfectSuccessBonus = 25f;
    [SerializeField] private float forcedTruthCost = 10f;
    [SerializeField] private float hostileSpiritDrain = 5f;
    
    [Header("UI References")]
    [SerializeField] private Slider energySlider;
    [SerializeField] private TMP_Text energyText;
    [SerializeField] private Image energyFillImage;
    [SerializeField] private Color highEnergyColor = Color.cyan;
    [SerializeField] private Color lowEnergyColor = Color.red;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject lowEnergyVFX;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip regenSound;
    
    public float CurrentEnergy => currentSpiritualEnergy;
    public float MaxEnergy => maxSpiritualEnergy;
    public float EnergyPercentage => currentSpiritualEnergy / maxSpiritualEnergy;
    public bool IsAlive => currentSpiritualEnergy > 0;
    
    public event Action<float> OnEnergyChanged;
    public event Action OnEnergyDepleted;
    public event Action<float> OnEnergyDamaged;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    void Start()
    {
        currentSpiritualEnergy = maxSpiritualEnergy;
        UpdateUI();
    }
    
    void Update()
    {
        if (allowRegen && currentSpiritualEnergy < maxSpiritualEnergy)
        {
            RegenerateEnergy(energyRegenRate * Time.deltaTime);
        }
        
        UpdateLowEnergyEffects();
    }
    
    public void DamageEnergy(float amount, string reason = "")
    {
        if (!IsAlive) return;
        
        float previousEnergy = currentSpiritualEnergy;
        currentSpiritualEnergy = Mathf.Max(0, currentSpiritualEnergy - amount);
        
        Debug.Log($"Spiritual Energy Lost: {amount} - Reason: {reason}");
        
        OnEnergyDamaged?.Invoke(amount);
        OnEnergyChanged?.Invoke(currentSpiritualEnergy);
        
        PlayDamageSound();
        UpdateUI();
        
        if (currentSpiritualEnergy <= 0 && previousEnergy > 0)
        {
            HandleEnergyDepletion();
        }
    }
    
    public void RestoreEnergy(float amount)
    {
        if (!IsAlive) return;
        
        currentSpiritualEnergy = Mathf.Min(maxSpiritualEnergy, currentSpiritualEnergy + amount);
        
        OnEnergyChanged?.Invoke(currentSpiritualEnergy);
        
        if (regenSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(regenSound);
        }
        
        UpdateUI();
    }
    
    private void RegenerateEnergy(float amount)
    {
        currentSpiritualEnergy = Mathf.Min(maxSpiritualEnergy, currentSpiritualEnergy + amount);
        OnEnergyChanged?.Invoke(currentSpiritualEnergy);
        UpdateUI();
    }

    /// <summary>
    /// Add energy (for perfect success bonus and other rewards)
    /// </summary>
    public void AddEnergy(float amount)
    {
        RestoreEnergy(amount);
    }
    
    /// <summary>
    /// Remove energy (for ritual penalties)
    /// </summary>
    public void RemoveEnergy(float amount)
    {
        DamageEnergy(amount, "Ritual Penalty");
    }
    
    public void ApplyWrongRitualPenalty()
    {
        DamageEnergy(wrongRitualPenalty, "Wrong Ritual Performed");
    }
    
    public void ApplyRitualFailurePenalty()
    {
        DamageEnergy(ritualFailurePenalty, "Ritual Failed");
    }
    
     public void ApplyTotalFailurePenalty()
    {
        DamageEnergy(totalFailurePenalty, "Total Failure");
    }
    
    public void ApplyPerfectSuccessBonus()
    {
        RestoreEnergy(perfectSuccessBonus);
    }

    public void ApplyForcedTruthCost()
    {
        DamageEnergy(forcedTruthCost, "Forced Truth Question");
    }
    
    public void ApplyHostileSpiritDrain()
    {
        DamageEnergy(hostileSpiritDrain, "Hostile Spirit Backlash");
    }
    
    private void HandleEnergyDepletion()
    {
        Debug.Log("Spiritual Energy Depleted - Game Over");
        
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        OnEnergyDepleted?.Invoke();
        
        if (GameController.Instance != null)
        {
            GameController.Instance.TriggerGameOver();
        }
    }
    
    private void UpdateUI()
    {
        if (energySlider != null)
        {
            energySlider.value = EnergyPercentage;
        }
        
        if (energyText != null)
        {
            energyText.text = $"{Mathf.CeilToInt(currentSpiritualEnergy)} / {Mathf.CeilToInt(maxSpiritualEnergy)}";
        }
        
        if (energyFillImage != null)
        {
            energyFillImage.color = Color.Lerp(lowEnergyColor, highEnergyColor, EnergyPercentage);
        }
    }
    
    private void UpdateLowEnergyEffects()
    {
        if (lowEnergyVFX != null)
        {
            bool shouldShowLowEnergyVFX = EnergyPercentage <= 0.25f;
            if (lowEnergyVFX.activeSelf != shouldShowLowEnergyVFX)
            {
                lowEnergyVFX.SetActive(shouldShowLowEnergyVFX);
            }
        }
    }
    
    private void PlayDamageSound()
    {
        if (damageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSound);
        }
    }
    
    public void ResetEnergy()
    {
        currentSpiritualEnergy = maxSpiritualEnergy;
        OnEnergyChanged?.Invoke(currentSpiritualEnergy);
        UpdateUI();
    }
    
    public void SetMaxEnergy(float newMax)
    {
        maxSpiritualEnergy = newMax;
        currentSpiritualEnergy = Mathf.Min(currentSpiritualEnergy, maxSpiritualEnergy);
        UpdateUI();
    }
    
    public void SetRegenEnabled(bool enabled)
    {
        allowRegen = enabled;
    }
}
