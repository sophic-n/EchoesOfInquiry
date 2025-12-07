using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Manages the deduction phase with environmental altars
/// Handles ritual selection and transition to ritual performance
/// </summary>
public class EnvironmentalRitualManager : MonoBehaviour
{
    public static EnvironmentalRitualManager Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private Canvas deductionCanvas;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_Text answerReviewText;
    [SerializeField] private CanvasGroup deductionCanvasGroup;
    
    [Header("Configuration")]
    [SerializeField] private bool requireConfirmation = true;
    [SerializeField] private float autoProgressDelay = 3f;
    
    private RitualAltar[] allAltars;
    private RitualAltar selectedAltar;
    private SpiritData currentSpirit;
    private Dictionary<string, string> currentAnswers;
    private bool isDeductionActive = false;
    
    public bool IsDeductionPhaseActive => isDeductionActive;
    
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
        allAltars = FindObjectsOfType<RitualAltar>();
        
        if (deductionCanvasGroup == null && deductionCanvas != null)
        {
            deductionCanvasGroup = deductionCanvas.GetComponent<CanvasGroup>();
        }
        
        if (deductionCanvas != null)
            deductionCanvas.enabled = false;
            
        Debug.Log($"EnvironmentalRitualManager initialized with {allAltars.Length} altars");
    }
    
    void Update()
{
    if (Input.GetKeyDown(KeyCode.E))
    {
        Debug.Log($"<color=magenta>E KEY DETECTED</color>");
        Debug.Log($"<color=magenta>IsDeductionActive: {isDeductionActive}</color>");
        Debug.Log($"<color=magenta>SelectedAltar: {(selectedAltar != null ? selectedAltar.ritualDisplayName : "None")}</color>");
        
        // IMPORTANT: If altar is selected, E confirms it
        if (isDeductionActive && selectedAltar != null)
        {
            Debug.Log($"<color=green>=== CONFIRMING RITUAL ===</color>");
            ConfirmRitualSelection();
            return; // Don't let altars also process this E press
        }
    }
    
    // Allow Space or Return as alternative confirm keys
    if (isDeductionActive && selectedAltar != null)
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log($"<color=green>=== CONFIRMING via Space/Return ===</color>");
            ConfirmRitualSelection();
        }
    }
}
    
    /// <summary>
    /// Called by SpiritEncounterManager when questions are complete
    /// </summary>
    public void BeginDeductionPhase(SpiritData spirit, Dictionary<string, string> answersGiven)
    {
        currentSpirit = spirit;
        currentAnswers = answersGiven;
        isDeductionActive = true;
        
        Debug.Log($"=== DEDUCTION PHASE STARTED ===");
        Debug.Log($"Spirit: {spirit.spiritName}");
        Debug.Log($"Correct Ritual: {spirit.ritualType}");
        Debug.Log($"Altars found: {allAltars.Length}");
        
        if (deductionCanvas != null)
        {
            deductionCanvas.enabled = true;
            if (deductionCanvasGroup != null)
                deductionCanvasGroup.alpha = 1f;
        }
        
        // DON'T pause game - player needs to move!
        // Time.timeScale = 0f; // COMMENTED OUT
        
        // Unlock cursor so player can see UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Setup UI
        if (instructionText != null)
        {
            instructionText.text = "Review the spirit's answers and approach a ritual altar.\nPress [E] to select, then confirm to begin.";
        }
        
        if (answerReviewText != null)
        {
            answerReviewText.text = FormatAnswersForDisplay(answersGiven);
        }
        
        // Activate all altars
        foreach (var altar in allAltars)
        {
            Debug.Log($"Altar: {altar.ritualDisplayName} at position {altar.transform.position}");
        }
        
        Debug.Log($"Player can now move and explore altars. Time.timeScale = {Time.timeScale}");
    }
    
    public void SelectAltar(RitualAltar altar)
{
    Debug.Log($"=== ALTAR SELECTED: {altar.ritualDisplayName} ({altar.ritualType}) ===");
    
    if (selectedAltar != null && selectedAltar != altar)
    {
        selectedAltar.DeselectAltar();
    }
    
    selectedAltar = altar;
    altar.SelectAltar();
    
    // Update instruction with clear confirm prompt
    if (instructionText != null)
    {
        instructionText.text = $"<size=24><b>Selected: {altar.ritualDisplayName}</b></size>\n\n" +
                              $"{altar.ritualDescription}\n\n" +
                              $"<color=yellow><size=20>Press [E] again to CONFIRM and begin ritual</size></color>\n" +
                              $"<color=grey>(or approach another altar to change selection)</color>";
    }
    
    Debug.Log($"<color=cyan>Player must press E again to confirm</color>");
}
    
    /// <summary>
    /// Called when player confirms ritual selection
    /// </summary>
    public void ConfirmRitualSelection()
    {
        if (selectedAltar == null)
        {
            Debug.LogWarning("No altar selected!");
            return;
        }
        
        Debug.Log($"Confirming ritual: {selectedAltar.ritualDisplayName}");
        
        // Resume normal time (was already running, but ensure it)
        Time.timeScale = 1f;
        
        // Lock cursor back for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (deductionCanvas != null)
            deductionCanvas.enabled = false;
        
        // Start the ritual with chosen type
        if (RitualManager.Instance != null)
        {
            RitualManager.Instance.BeginRitual(currentSpirit, selectedAltar.ritualType);
        }
        
        isDeductionActive = false;
    }
    
    private string FormatAnswersForDisplay(Dictionary<string, string> answers)
    {
        string formatted = "";
        foreach (var kvp in answers)
        {
            formatted += $"<b>Q:</b> {kvp.Key}\n";
            formatted += $"<b>A:</b> {kvp.Value}\n\n";
        }
        return formatted;
    }
}

/// <summary>
/// Provides thematic hints when player chooses wrong ritual
/// </summary>
public class RitualHintSystem : MonoBehaviour
{
    private static readonly Dictionary<RitualType, string[]> ElementalHints = new()
    {
        { RitualType.CleansingFlame, new[] {
            "The spirit recoils from the flames.",
            "Fire brings only pain, not peace.",
            "The heat intensifies their suffering.",
            "They were not bound by fire or regret.",
            "Flame is not the answer they seek."
        }},
        
        { RitualType.BindingFlames, new[] {
            "The twin flames spark confusion, not clarity.",
            "This spirit was not bound by paired fires.",
            "The burning brings no resolution.",
            "Fire cannot break this particular binding."
        }},
        
        { RitualType.BindingWaters, new[] {
            "The water does not soothe them.",
            "They pull away from the flowing ritual.",
            "Water holds no meaning for this spirit.",
            "The currents disturb rather than calm.",
            "This spirit was not claimed by water."
        }},
        
        { RitualType.WeepingWillowRite, new[] {
            "Nature's embrace feels foreign to them.",
            "The roots do not reach their heart.",
            "Green growth means nothing to this soul.",
            "They were not bound beneath tree or vine.",
            "The earth rejects this offering."
        }},
        
        { RitualType.BlossomRite, new[] {
            "The petals fall on deaf spiritual ears.",
            "Flowers cannot bind what is truly broken.",
            "Nature's beauty means nothing to them.",
            "They were not bound by gardens or bloom."
        }},
        
        { RitualType.RenewalRite, new[] {
            "Rebirth calls to a spirit that seeks no renewal.",
            "The cycle means nothing to them.",
            "Growth cannot reach this particular soul.",
            "They do not wish to be reborn."
        }},
        
        { RitualType.EchoOfBlades, new[] {
            "The clash of steel brings no resolution.",
            "War's echo is not what binds them.",
            "They did not fall to blade or battle.",
            "Martial rites stir confusion, not clarity.",
            "This spirit knew no warrior's end."
        }},
        
        { RitualType.WarChant, new[] {
            "The war drums beat for another's sorrow.",
            "This spirit fought no war.",
            "Martial rhythm means nothing here.",
            "The chant of battle falls silent before them."
        }},
        
        { RitualType.ReleaseArmy, new[] {
            "No army stands behind this spirit.",
            "The banner's call reaches no one.",
            "They commanded no troops.",
            "This was no general's binding."
        }},
        
        { RitualType.SongOfUnmasking, new[] {
            "The truth revealed is not theirs to claim.",
            "Unmasking brings no peace to this one.",
            "They hide nothing that your ritual can reveal.",
            "Deception was not their chain."
        }},
        
        { RitualType.MirrorRitual, new[] {
            "The mirror shows a face that is not their burden.",
            "Reflection cannot reach their core.",
            "They do not fear what they see.",
            "The image reflected means nothing to them."
        }},
        
        { RitualType.FeastOfSilence, new[] {
            "Loneliness was not what held them.",
            "Silence deepens, but does not lift.",
            "They do not hunger for this meal.",
            "Famine did not claim this soul."
        }},
        
        { RitualType.LanternLight, new[] {
            "The light guides no one here.",
            "This spirit seeks no illumination.",
            "The vigil burns for the wrong sorrow.",
            "They have no need of guidance."
        }},
        
        { RitualType.StoneToAshes, new[] {
            "Stone crumbles, but this spirit remains bound.",
            "Rubble cannot break their chains.",
            "They were not buried beneath stone.",
            "Collapse was not their fate."
        }},
        
        { RitualType.RebuildAltar, new[] {
            "Sacred offerings ring hollow to them.",
            "The gods they knew are silent still.",
            "Sanctity holds no power over their binding.",
            "This spirit was not bound by divine oath."
        }},
        
        { RitualType.PlaceFlower, new[] {
            "The offering falls on unhearing ground.",
            "Flowers cannot replace what they lost.",
            "A gentle gesture will not free them.",
            "This spirit requires more than remembrance."
        }}
    };
    
    /// <summary>
    /// Get hint for wrong ritual choice based on elemental theme
    /// </summary>
    public static string GetElementalHint(RitualType chosenRitual)
    {
        if (ElementalHints.ContainsKey(chosenRitual))
        {
            var hints = ElementalHints[chosenRitual];
            return hints[Random.Range(0, hints.Length)];
        }
        
        return "The ritual does not resonate with this spirit.";
    }
    
    /// <summary>
    /// Get personality-based hint for wrong choice
    /// </summary>
    public static string GetPersonalityHint(Personality personality)
    {
        return personality switch
        {
            Personality.Mournful => "Your melody stirs only deeper sorrow. The ritual cannot touch what truly binds them.",
            Personality.Hostile => "Your ritual enrages them further! The spirit thrashes against your bindings.",
            Personality.Agitated => "The spirit thrashes in anguish—this was not the way.",
            Personality.Meek => "The spirit shrinks from your offering. Your ritual frightens more than frees.",
            Personality.Trickster => "The spirit laughs bitterly at your choice. Clever... but not clever enough.",
            Personality.Evasive => "Your offering slips past them like water through fingers.",
            Personality.Neutral => "The ritual completes, yet nothing changes. Your offering falls on deaf spiritual ears.",
            _ => "The binding remains unbroken."
        };
    }
    
    /// <summary>
    /// Get comprehensive wrong ritual message
    /// </summary>
    public static string GetWrongRitualMessage(RitualType chosenRitual, Personality spiritPersonality)
    {
        // 60% chance for elemental hint, 40% for personality hint
        if (Random.value < 0.6f)
        {
            return GetElementalHint(chosenRitual);
        }
        else
        {
            return GetPersonalityHint(spiritPersonality);
        }
    }
}