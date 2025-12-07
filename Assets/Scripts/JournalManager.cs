using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Manages the player's journal/codex that displays:
/// - Available questions with their sequences
/// - Known rituals and their purposes
/// - Collected story fragments
/// Press TAB to open/close
/// </summary>
public class JournalManager : MonoBehaviour
{
    public static JournalManager Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private GameObject journalPanel;
    [SerializeField] private CanvasGroup journalCanvasGroup;
    
    [Header("Tab Buttons")]
    [SerializeField] private Button questionsTabButton;
    [SerializeField] private Button ritualsTabButton;
    [SerializeField] private Button fragmentsTabButton;
    
    [Header("Tab Panels")]
    [SerializeField] private GameObject questionsPanel;
    [SerializeField] private GameObject ritualsPanel;
    [SerializeField] private GameObject fragmentsPanel;
    
    [Header("Questions Panel")]
    [SerializeField] private Transform questionsContainer;
    [SerializeField] private GameObject questionEntryPrefab;
    [SerializeField] private TMP_Text questionsHeaderText;
    
    [Header("Rituals Panel")]
    [SerializeField] private Transform ritualsContainer;
    [SerializeField] private GameObject ritualEntryPrefab;
    [SerializeField] private TMP_Text ritualsHeaderText;
    
    [Header("Fragments Panel")]
    [SerializeField] private Transform fragmentsContainer;
    [SerializeField] private GameObject fragmentEntryPrefab;
    [SerializeField] private TMP_Text fragmentsHeaderText;
    [SerializeField] private ScrollRect fragmentsScrollRect;
    
    [Header("Configuration")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
    [SerializeField] private bool showLockedQuestions = true;
    [SerializeField] private Color unlockedColor = Color.white;
    [SerializeField] private Color lockedColor = Color.gray;
    
    [Header("References")]
    [SerializeField] private QuestionLibrary questionLibrary;
    
    private bool isOpen = false;
    private JournalTab currentTab = JournalTab.Questions;
    private List<GameObject> spawnedEntries = new List<GameObject>();
    
    public enum JournalTab { Questions, Rituals, Fragments }
    
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
    SetupTabButtons();

    // Ensure CanvasGroup exists
    if (journalCanvasGroup == null && journalPanel != null)
    {
        journalCanvasGroup = journalPanel.GetComponent<CanvasGroup>();
        if (journalCanvasGroup == null)
        {
            journalCanvasGroup = journalPanel.AddComponent<CanvasGroup>();
            Debug.Log("CanvasGroup missing — automatically added to journalPanel.");
        }
        // Hide all panels initially
        if (questionsPanel != null) questionsPanel.SetActive(false);
        if (ritualsPanel != null) ritualsPanel.SetActive(false);
        if (fragmentsPanel != null) fragmentsPanel.SetActive(false);
    }

    CloseJournal();
}

    
    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleJournal();
        }
    }
    
    private void SetupTabButtons()
    {
        if (questionsTabButton != null)
            questionsTabButton.onClick.AddListener(() => SwitchTab(JournalTab.Questions));
            
        if (ritualsTabButton != null)
            ritualsTabButton.onClick.AddListener(() => SwitchTab(JournalTab.Rituals));
            
        if (fragmentsTabButton != null)
            fragmentsTabButton.onClick.AddListener(() => SwitchTab(JournalTab.Fragments));
    }
    
    public void ToggleJournal()
    {
        if (isOpen)
        {
            CloseJournal();
        }
        else
        {
            OpenJournal();
        }
    }

      private void CloseAllPanels()
{
    if (questionsPanel) questionsPanel.SetActive(false);
    if (ritualsPanel) ritualsPanel.SetActive(false);
    if (fragmentsPanel) fragmentsPanel.SetActive(false);
}
    
    public void OpenJournal()
    {
        isOpen = true;
        
        if (journalPanel != null)
            journalPanel.SetActive(true);
            
        if (journalCanvasGroup != null)
        {
            journalCanvasGroup.alpha = 1f;
            journalCanvasGroup.interactable = true;
            journalCanvasGroup.blocksRaycasts = true;
        }
        
        // Pause game time if needed
        Time.timeScale = 0f;
        
        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        

        // Don't automatically show any tab - wait for player to click
        // Keep all panels hidden until tab button is pressed
       
        Debug.Log("Journal opened - select a tab to view");
    }

    
    public void CloseJournal()
    {
        isOpen = false;
        
        if (journalPanel != null)
            journalPanel.SetActive(false);
            
        if (journalCanvasGroup != null)
        {
            journalCanvasGroup.alpha = 0f;
            journalCanvasGroup.interactable = false;
            journalCanvasGroup.blocksRaycasts = false;
        }
        
        // Resume game
        Time.timeScale = 1f;
        
        // Lock cursor back
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Debug.Log("Journal closed");
    }
    
    public void SwitchTab(JournalTab tab)
    {
        currentTab = tab;
        
        // Hide all panels
        if (questionsPanel != null) questionsPanel.SetActive(false);
        if (ritualsPanel != null) ritualsPanel.SetActive(false);
        if (fragmentsPanel != null) fragmentsPanel.SetActive(false);
        
        // Show selected panel
        switch (tab)
        {
            case JournalTab.Questions:
                if (questionsPanel != null) questionsPanel.SetActive(true);
                RefreshQuestionsTab();
                break;
                
            case JournalTab.Rituals:
                if (ritualsPanel != null) ritualsPanel.SetActive(true);
                RefreshRitualsTab();
                break;
                
            case JournalTab.Fragments:
                if (fragmentsPanel != null) fragmentsPanel.SetActive(true);
                RefreshFragmentsTab();
                break;
        }
    }
    
    public void RefreshCurrentTab()
    {
        SwitchTab(currentTab);
    }
    public void CloseCurrentPanel()
{
    CloseAllPanels();
}

    
    // ===== QUESTIONS TAB =====
    
    // ===== QUESTIONS TAB =====

private void RefreshQuestionsTab()
{
    ClearSpawnedEntries();
    
    if (questionLibrary == null || SpiritEncounterManager.Instance == null)
    {
        Debug.LogWarning("Question library or encounter manager not found!");
        return;
    }
    
    // Get current unlock progress from SpiritEncounterManager
    var progress = SpiritEncounterManager.Instance.unlockProgress;
    
    // Get available questions
    var unlocked = questionLibrary.GetAvailableQuestions(progress);
    var allQuestions = questionLibrary.allQuestions;
    
    int unlockedCount = 0;
    int totalCount = allQuestions.Length;
    
    foreach (var question in allQuestions)
    {
        bool isUnlocked = questionLibrary.IsQuestionUnlocked(question, progress);
        
        if (isUnlocked)
            unlockedCount++;
        
        // Skip locked questions if configured
        if (!isUnlocked && !showLockedQuestions)
            continue;
            
        CreateQuestionEntry(question, isUnlocked);
    }
    
    // Update header
    if (questionsHeaderText != null)
    {
        questionsHeaderText.text = $"Questions Known: {unlockedCount}/{totalCount}";
    }
}

private void CreateQuestionEntry(QuestionTemplate question, bool isUnlocked)
{
    if (questionEntryPrefab == null || questionsContainer == null)
        return;
        
    GameObject entry = Instantiate(questionEntryPrefab, questionsContainer);
    spawnedEntries.Add(entry);
    
    // Set text color based on unlock status
    var textComponent = entry.GetComponentInChildren<TMP_Text>();
    if (textComponent != null)
    {
        textComponent.text = question.questionText;
        textComponent.color = isUnlocked ? unlockedColor : lockedColor;
    }
    
    // Show sequence notation if available
    var sequenceText = entry.GetComponentsInChildren<TMP_Text>();
    if (sequenceText.Length > 1)
    {
        sequenceText[1].text = question.sequenceNotation ?? "???";
        sequenceText[1].color = isUnlocked ? unlockedColor : lockedColor;
    }
}

    
    private TMP_Text FindTextComponent(Transform parent, string name)
    {
        // Try direct child first
        Transform child = parent.Find(name);
        if (child != null)
        {
            TMP_Text text = child.GetComponent<TMP_Text>();
            if (text != null) return text;
        }
        
        // Try GetComponentInChildren as fallback
        TMP_Text[] allTexts = parent.GetComponentsInChildren<TMP_Text>(true);
        foreach (var text in allTexts)
        {
            if (text.gameObject.name == name)
                return text;
        }
        
        return null;
    }
    
    private string GetSequenceNotation(QuestionTemplate question)
    {
        if (!string.IsNullOrEmpty(question.sequenceNotation))
            return question.sequenceNotation;
            
        // Generate notation from keys
        string notation = "";
        if (question.noteSequence != null)
        {
            foreach (var key in question.noteSequence)
            {
                notation += key.ToString() + "-";
            }
            notation = notation.TrimEnd('-');
        }
        return notation;
    }
    
    // ===== RITUALS TAB =====
    
    private void RefreshRitualsTab()
    {
        ClearSpawnedEntries();
        
        // Get all ritual types
        var ritualTypes = System.Enum.GetValues(typeof(RitualType));
        
        foreach (RitualType ritual in ritualTypes)
        {
            CreateRitualEntry(ritual);
        }
        
        // Update header
        if (ritualsHeaderText != null)
        {
            ritualsHeaderText.text = $"Known Rituals: {ritualTypes.Length}";
        }
    }
    
    private void CreateRitualEntry(RitualType ritual)
    {
        if (ritualEntryPrefab == null || ritualsContainer == null) return;
        
        GameObject entry = Instantiate(ritualEntryPrefab, ritualsContainer);
        spawnedEntries.Add(entry);
        
        // Force RectTransform setup
        RectTransform entryRect = entry.GetComponent<RectTransform>();
        if (entryRect != null)
        {
            entryRect.localScale = Vector3.one;
        }
        
        // Find UI components
        TMP_Text titleText = FindTextComponent(entry.transform, "TitleText");
        TMP_Text descriptionText = FindTextComponent(entry.transform, "DescriptionText");
        
        if (titleText != null)
        {
            titleText.text = GetRitualDisplayName(ritual);
            titleText.enabled = true;
            Debug.Log($"Ritual Entry Created: {titleText.text}");
        }
        else
        {
            Debug.LogWarning($"TitleText not found in ritual prefab!");
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = GetRitualDescription(ritual);
            descriptionText.enabled = true;
        }
        else
        {
            Debug.LogWarning($"DescriptionText not found in ritual prefab!");
        }
        
        // Force layout rebuild
        LayoutRebuilder.ForceRebuildLayoutImmediate(entryRect);
    }
    
    private string GetRitualDisplayName(RitualType ritual)
    {
        return ritual.ToString().Replace("_", " ");
    }
    
    private string GetRitualDescription(RitualType ritual)
    {
        switch (ritual)
        {
            case RitualType.CleansingFlame:
                return "For those consumed by fire or regret. Purifies through flame.";
            case RitualType.BindingWaters:
                return "For those taken by river or sea. Releases through water's flow.";
            case RitualType.SongOfUnmasking:
                return "For those hidden by deceit or lies. Reveals true face.";
            case RitualType.WeepingWillowRite:
                return "For betrayal beneath sacred trees. Untangles sorrow from roots.";
            case RitualType.EchoOfBlades:
                return "For warriors and soldiers. Silences the steel's cry.";
            case RitualType.FeastOfSilence:
                return "For those who starved or died alone. Fills the empty table.";
            case RitualType.StoneToAshes:
                return "For those buried or crushed. Returns dust to dust.";
            case RitualType.LanternLight:
                return "For those who wait or guide. Illuminates the path home.";
            case RitualType.RenewalRite:
                return "For unfulfilled duty or care. Brings growth from loss.";
            case RitualType.BindingFlames:
                return "For lovers parted by death. Reunites through fire.";
            case RitualType.WarChant:
                return "For dishonored or forgotten soldiers. Restores the warrior's song.";
            case RitualType.BlossomRite:
                return "For those tied to earth and growth. Blooms despite winter.";
            case RitualType.MirrorRitual:
                return "For masks, jealousy, false faces. Shatters the reflection.";
            case RitualType.ReleaseArmy:
                return "For commanders haunted by their fallen. Dismisses the legion.";
            case RitualType.RebuildAltar:
                return "For monks and sacred martyrs. Reconstructs faith from ash.";
            case RitualType.PlaceFlower:
                return "For final offerings and last rites. Completes the farewell.";
            default:
                return "Purpose unknown.";
        }
    }
    
    // ===== FRAGMENTS TAB =====
    
    private void RefreshFragmentsTab()
    {
        ClearSpawnedEntries();
        
        if (StoryFragmentManager.Instance == null)
        {
            Debug.LogWarning("Story fragment manager not found!");
            return;
        }
        
        var fragments = StoryFragmentManager.Instance.GetAllFragments();
        
        // Sort by collection date (newest first)
        fragments = fragments.OrderByDescending(f => f.collectedDate).ToList();
        
        foreach (var fragment in fragments)
        {
            CreateFragmentEntry(fragment);
        }
        
        // Update header
        if (fragmentsHeaderText != null)
        {
            int total = fragments.Count;
            int needed = StoryFragmentManager.Instance.FragmentsNeeded;
            fragmentsHeaderText.text = $"Story Fragments: {total}/{needed}";
            
            if (StoryFragmentManager.Instance.IsTrueEndingUnlocked)
            {
                fragmentsHeaderText.text += " ✨ TRUE ENDING UNLOCKED";
            }
        }
        
        // Scroll to top
        if (fragmentsScrollRect != null)
        {
            fragmentsScrollRect.verticalNormalizedPosition = 1f;
        }
    }
    
    private void CreateFragmentEntry(StoryFragment fragment)
    {
        if (fragmentEntryPrefab == null || fragmentsContainer == null) return;
        
        GameObject entry = Instantiate(fragmentEntryPrefab, fragmentsContainer);
        spawnedEntries.Add(entry);
        
        // Force RectTransform setup
        RectTransform entryRect = entry.GetComponent<RectTransform>();
        if (entryRect != null)
        {
            entryRect.localScale = Vector3.one;
        }
        
        // Find UI components
        TMP_Text spiritText = FindTextComponent(entry.transform, "SpiritText");
        TMP_Text fragmentText = FindTextComponent(entry.transform, "FragmentText");
        TMP_Text dateText = FindTextComponent(entry.transform, "DateText");
        
        if (spiritText != null)
        {
            spiritText.text = $"Spirit: {fragment.spiritID}";
            spiritText.enabled = true;
            Debug.Log($"Fragment Entry Created: {fragment.spiritID}");
        }
        else
        {
            Debug.LogWarning($"SpiritText not found in fragment prefab!");
        }
        
        if (fragmentText != null)
        {
            fragmentText.text = fragment.fragmentText;
            fragmentText.enabled = true;
        }
        else
        {
            Debug.LogWarning($"FragmentText not found in fragment prefab!");
        }
        
        if (dateText != null)
        {
            dateText.text = $"Collected: {fragment.collectedDate}";
            dateText.enabled = true;
        }
        else
        {
            Debug.LogWarning($"DateText not found in fragment prefab!");
        }
        
        // Force layout rebuild
        LayoutRebuilder.ForceRebuildLayoutImmediate(entryRect);
    }
    
    // ===== UTILITY =====
    
    private void ClearSpawnedEntries()
    {
        foreach (var entry in spawnedEntries)
        {
            if (entry != null)
                Destroy(entry);
        }
        spawnedEntries.Clear();
    }
    
    public void UpdateJournal()
    {
        if (isOpen)
        {
            RefreshCurrentTab();
        }
    }
}
