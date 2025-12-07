using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using TMPro;

/// <summary>
/// Manages the deduction phase where player chooses the correct ritual
/// Based on answers received from the spirit
/// </summary>
public class DeductionManager : MonoBehaviour
{
    public static DeductionManager Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private GameObject deductionPanel;
    [SerializeField] private TMP_Text deductionPromptText;
    [SerializeField] private Transform ritualButtonContainer;
    [SerializeField] private GameObject ritualButtonPrefab;
    
    [Header("Review Panel")]
    [SerializeField] private GameObject answerReviewPanel;
    [SerializeField] private TMP_Text answerReviewText;
    [SerializeField] private ScrollRect answerScrollRect;
    
    [Header("Configuration")]
    [SerializeField] private bool allowWrongRitualChoice = true;
    [SerializeField] private float wrongRitualPenaltyMultiplier = 1.5f;
    
    private SpiritData currentSpirit;
    private Dictionary<string, string> currentAnswers;
    private RitualType selectedRitualType;
    private RitualType deducedRitualType;
    private List<GameObject> ritualButtons = new List<GameObject>();
    private RitualManager ritualManager;
    
    public event Action<RitualType> OnRitualChosen;
    
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
        HideDeductionPanel();
        ritualManager = RitualManager.Instance;
    }
    
    public void BeginDeductionPhase(SpiritData spirit, Dictionary<string, string> answers)
    {
        currentSpirit = spirit;
        currentAnswers = new Dictionary<string, string>(answers);
        
        ShowDeductionPanel();
        DisplayAnswerReview();
        CreateRitualChoices();
    }
    
    private void ShowDeductionPanel()
    {
        if (deductionPanel != null)
        {
            deductionPanel.SetActive(true);
        }
        
        if (deductionPromptText != null)
        {
            deductionPromptText.text = "Review the spirit's answers and choose the appropriate ritual to free them.";
        }
    }
    
    private void HideDeductionPanel()
    {
        if (deductionPanel != null)
        {
            deductionPanel.SetActive(false);
        }
    }
    
    private void DisplayAnswerReview()
    {
        if (answerReviewPanel == null || answerReviewText == null) return;
        
        answerReviewPanel.SetActive(true);
        
        string reviewText = "=== SPIRIT'S ANSWERS ===\n\n";
        
        foreach (var qa in currentAnswers)
        {
            reviewText += $"Q: {qa.Key}\n";
            reviewText += $"A: {qa.Value}\n\n";
        }
        
        answerReviewText.text = reviewText;
        
        // Reset scroll to top
        if (answerScrollRect != null)
        {
            answerScrollRect.verticalNormalizedPosition = 1f;
        }
    }
    
    private void CreateRitualChoices()
    {
        // Clear existing buttons
        foreach (var button in ritualButtons)
        {
            Destroy(button);
        }
        ritualButtons.Clear();
        
        if (ritualButtonContainer == null || ritualButtonPrefab == null) return;
        
        // Get all ritual types
        RitualType[] allRituals = (RitualType[])Enum.GetValues(typeof(RitualType));
        
        foreach (var ritualType in allRituals)
        {
            GameObject buttonObj = Instantiate(ritualButtonPrefab, ritualButtonContainer);
            ritualButtons.Add(buttonObj);
            
            Button button = buttonObj.GetComponent<Button>();
            Text buttonText = buttonObj.GetComponentInChildren<Text>();
            
            if (buttonText != null)
            {
                buttonText.text = GetRitualDisplayName(ritualType);
            }
            
            if (button != null)
            {
                RitualType capturedRitualType = ritualType;
                button.onClick.AddListener(() => OnRitualButtonClicked(capturedRitualType));
            }
            
            // Optionally highlight correct ritual (for testing/easy mode)
            // if (ritualType == currentSpirit.ritualType)
            // {
            //     buttonText.color = Color.green;
            // }
        }
    }
    
    private string GetRitualDisplayName(RitualType ritualType)
    {
        switch (ritualType)
        {
            case RitualType.CleansingFlame:
                return "Cleansing Flame - For those consumed by fire or regret";
            case RitualType.BindingWaters:
                return "Binding Waters - For those taken by river or sea";
            case RitualType.SongOfUnmasking:
                return "Song of Unmasking - For those hidden by deceit or lies";
            case RitualType.WeepingWillowRite:
                return "Weeping Willow Rite - For betrayal beneath sacred trees";
            case RitualType.EchoOfBlades:
                return "Echo of Blades - For warriors and soldiers";
            case RitualType.FeastOfSilence:
                return "Feast of Silence - For those who starved or died alone";
            case RitualType.StoneToAshes:
                return "Stone to Ashes - For those buried or crushed";
            case RitualType.LanternLight:
                return "Lantern Light - For those who wait or guide";
            case RitualType.RenewalRite:
                return "Renewal Rite - For unfulfilled duty or care";
            case RitualType.BindingFlames:
                return "Binding Flames - For lovers parted by death";
            case RitualType.WarChant:
                return "War Chant - For dishonored or forgotten soldiers";
            case RitualType.BlossomRite:
                return "Blossom Rite - For those tied to earth and growth";
            case RitualType.MirrorRitual:
                return "Mirror Ritual - For masks, jealousy, and false faces";
            case RitualType.ReleaseArmy:
                return "Release Army - For commanders haunted by their fallen";
            case RitualType.RebuildAltar:
                return "Rebuild Altar - For monks and sacred martyrs";
            case RitualType.PlaceFlower:
                return "Place Flower - For final offerings and last rites";
            default:
                return ritualType.ToString();
        }
    }
    
    private void OnRitualButtonClicked(RitualType chosenRitual)
    {
        selectedRitualType = chosenRitual;
        
        Debug.Log($"Player chose: {chosenRitual} | Correct: {currentSpirit.ritualType}");
        
        bool isCorrect = chosenRitual == currentSpirit.ritualType;
        
        if (isCorrect)
        {
            HandleCorrectRitual();
        }
        else if (allowWrongRitualChoice)
        {
            HandleWrongRitual();
        }
        else
        {
            // Force player to choose correct one
            if (deductionPromptText != null)
            {
                deductionPromptText.text = "That doesn't feel right... Try again.";
            }
        }
    }
    
    private void HandleCorrectRitual()
    {
        if (deductionPromptText != null)
        {
            deductionPromptText.text = "You sense this is the correct ritual. Preparing...";
        }
        
        OnRitualChosen?.Invoke(selectedRitualType);
        
        HideDeductionPanel();
        
        // Proceed to ritual
        if (RitualManager.Instance != null)
        {
            ritualManager.BeginRitual(currentSpirit, deducedRitualType);
        }
    }
    
    private void HandleWrongRitual()
    {
        if (deductionPromptText != null)
        {
            deductionPromptText.text = "The ritual backfires! Spiritual energy drains...";
        }
        
        // Apply penalty
        if (SpiritualEnergyManager.Instance != null)
        {
            float penalty = SpiritualEnergyManager.Instance.MaxEnergy * 0.2f * wrongRitualPenaltyMultiplier;
            SpiritualEnergyManager.Instance.DamageEnergy(penalty, "Wrong Ritual Chosen");
        }
        
        // Damage instrument
        if (InstrumentManager.Instance != null)
        {
            InstrumentManager.Instance.ApplyRitualFailureDamage();
        }
        
        // Give player chance to try again or skip
        ShowRetryOptions();
    }
    
    private void ShowRetryOptions()
    {
        // Could show UI buttons for "Try Again" or "Skip Spirit"
        Debug.Log("Wrong ritual chosen - show retry options");
    }
    
    public void SkipSpirit()
    {
        Debug.Log("Player chose to skip this spirit");
        
        HideDeductionPanel();
        
        // Move to next spirit
        if (GameController.Instance != null)
        {
            GameController.Instance.ProgressToNextSpirit();
        }
    }
    
    public void EndDeductionPhase()
    {
        HideDeductionPanel();
        currentSpirit = null;
        currentAnswers = null;
    }
}