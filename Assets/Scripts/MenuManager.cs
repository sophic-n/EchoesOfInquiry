using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Manages all menu screens: Main Menu, Run Complete, True Ending
/// </summary>
public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }
    
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject runCompletePanel;
    [SerializeField] private GameObject trueEndingPanel;
    [SerializeField] private GameObject instructionsPanel;
    
    [Header("Main Menu Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button instructionsButton;
    [SerializeField] private Button quitButton;
    
    [Header("Instructions Panel")]
    [SerializeField] private Button instructionsBackButton;
    [SerializeField] private TMP_Text instructionsText;
    
    [Header("Run Complete Panel")]
    [SerializeField] private TMP_Text runCompleteStatsText;
    [SerializeField] private Button newRunButton;
    [SerializeField] private Button mainMenuFromRunButton;
    
    [Header("True Ending Panel")]
    [SerializeField] private TMP_Text trueEndingText;
    [SerializeField] private Button exitToMenuButton;
    
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
        SetupButtons();
        ShowMainMenu();
    }
    
    private void SetupButtons()
    {
        // Main Menu buttons
        if (newGameButton != null)
            newGameButton.onClick.AddListener(OnNewGame);
            
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinue);
            
        if (instructionsButton != null)
            instructionsButton.onClick.AddListener(OnInstructions);
            
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuit);
        
        // Instructions buttons
        if (instructionsBackButton != null)
            instructionsBackButton.onClick.AddListener(OnInstructionsBack);
        
        // Run Complete buttons
        if (newRunButton != null)
            newRunButton.onClick.AddListener(OnNewRun);
            
        if (mainMenuFromRunButton != null)
            mainMenuFromRunButton.onClick.AddListener(OnMainMenuFromRun);
        
        // True Ending buttons
        if (exitToMenuButton != null)
            exitToMenuButton.onClick.AddListener(OnExitToMenu);
    }
    
    // ===== MAIN MENU =====
    
    public void ShowMainMenu()
    {
        HideAllPanels();
        
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
        
        // Check if continue is available
        UpdateContinueButton();
        
        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
        
        Debug.Log("Main Menu shown");
    }
    
    private void UpdateContinueButton()
    {
        if (continueButton == null) return;
        
        // Check if there's save data
        bool hasSaveData = StoryFragmentManager.Instance != null && 
                           StoryFragmentManager.Instance.TotalFragmentsCollected > 0;
        
        continueButton.interactable = hasSaveData;
    }
    
    private void OnNewGame()
    {
        Debug.Log("New Game clicked");
        
        // Reset all progress (optional - you might want to keep fragments)
        if (StoryFragmentManager.Instance != null)
        {
            // Uncomment to reset fragments on new game:
            // StoryFragmentManager.Instance.ResetProgress();
        }
        
        StartGame();
    }
    
    private void OnContinue()
    {
        Debug.Log("Continue clicked");
        StartGame();
    }
    
    private void OnInstructions()
    {
        Debug.Log("Instructions clicked");
        HideAllPanels();
        
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(true);
            PopulateInstructions();
        }
    }
    
    private void OnInstructionsBack()
    {
        ShowMainMenu();
    }
    
    private void PopulateInstructions()
    {
        if (instructionsText == null) return;
        
        instructionsText.text = @"ECHOES OF INQUIRY

CONTROLS:
• WASD - Move
• Mouse - Look around
• Z X C V B N M K - Musical notes
• TAB - Open/Close Journal
• ESC - Pause
• E - Interact/Press twice to perform ritual

HOW TO PLAY:

1. ENCOUNTER SPIRITS
Each spirit speaks in riddles. Listen to their opening words.

2. ASK QUESTIONS
Play note sequences (e.g. Z-X-C) to ask questions.
You can ask 5 questions per spirit.
Check your Journal (TAB) to see all available questions.

3. DEDUCE THE TRUTH
Based on the spirit's answers, determine:
- How they died
- Why they linger
- What ritual will free them

4. PERFORM THE RITUAL
Choose the correct ritual type. They are scattered thorughout the world.
Repeat the melody sequence shown to you.
Success frees the spirit and reveals their story.

PROGRESSION:
• 15 spirits per run
• Collect story fragments from freed spirits
• Unlock new questions as you progress
• 30 fragments unlocks the True Ending

TIPS:
• Some spirits lie or evade - trust your deduction
• Advanced questions unlock after freeing spirits
• Failed rituals drain your spiritual energy
• Your instrument can break - notes become unavailable
• Story fragments persist between runs";
    }
    
    private void OnQuit()
    {
        Debug.Log("Quit clicked");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    private void StartGame()
    {
        HideAllPanels();
        
        // Resume time
        Time.timeScale = 1f;
        
        // Lock cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Start the run
        if (GameController.Instance != null)
        {
            GameController.Instance.StartNewRun();
        }
        else
        {
            Debug.LogError("GameController not found!");
        }
    }
    
    // ===== RUN COMPLETE SCREEN =====
    
    public void ShowRunComplete(int spiritsFreed, int totalSpirits, float successRate)
    {
        HideAllPanels();
        
        if (runCompletePanel != null)
            runCompletePanel.SetActive(true);
        
        if (runCompleteStatsText != null)
        {
            int fragmentsCollected = StoryFragmentManager.Instance != null ? 
                                     StoryFragmentManager.Instance.TotalFragmentsCollected : 0;
            int fragmentsNeeded = StoryFragmentManager.Instance != null ?
                                  StoryFragmentManager.Instance.FragmentsNeeded : 30;
            
            runCompleteStatsText.text = $@"RUN COMPLETE

Spirits Freed: {spiritsFreed}/{totalSpirits}
Success Rate: {successRate:P0}

Story Fragments: {fragmentsCollected}/{fragmentsNeeded}

{(fragmentsCollected >= fragmentsNeeded ? "✨ TRUE ENDING UNLOCKED! ✨" : $"Collect {fragmentsNeeded - fragmentsCollected} more fragments for the True Ending")}";
        }
        
        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
        
        Debug.Log("Run Complete screen shown");
    }
    
    private void OnNewRun()
    {
        Debug.Log("New Run clicked");
        
        HideAllPanels();
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (GameController.Instance != null)
        {
            GameController.Instance.RestartRun();
        }
    }
    
    private void OnMainMenuFromRun()
    {
        Debug.Log("Main Menu clicked from Run Complete");
        ShowMainMenu();
    }
    
    // ===== TRUE ENDING SCREEN =====
    
    public void ShowTrueEnding()
    {
        HideAllPanels();
        
        if (trueEndingPanel != null)
            trueEndingPanel.SetActive(true);
        
        if (trueEndingText != null)
        {
            trueEndingText.text = @"TRUE ENDING

Through countless encounters and endless questions,
you have pieced together the truth.

The spirits you freed were not merely restless souls,
but fragments of a greater tragedy.

[Your game's true ending text here]

The cultivator closes their eyes,
and the echoes finally fall silent.

Thank you for playing.";
        }
        
        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
        
        Debug.Log("True Ending screen shown");
    }
    
    private void OnExitToMenu()
    {
        Debug.Log("Exit to Menu clicked from True Ending");
        ShowMainMenu();
    }
    
    // ===== UTILITY =====
    
    private void HideAllPanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (runCompletePanel != null) runCompletePanel.SetActive(false);
        if (trueEndingPanel != null) trueEndingPanel.SetActive(false);
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
    }
    
    public void PauseGame()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}