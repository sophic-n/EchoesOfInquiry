using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    
    [Header("Game State")]
    public GameState currentState = GameState.MainMenu;
    
    [Header("Statistics")]
    public GameStatistics statistics = new GameStatistics();
    
    [Header("Current Encounter")]
    private EncounterRecord currentEncounterRecord;
    private float encounterStartTime;
    
    public enum GameState
    {
        MainMenu,
        RunSetup,
        Encounter,
        Deduction,
        Ritual,
        RunComplete,
        Paused
    }
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    void Start()
    {
        InitializeGame();
    }
    
    void Update()
    {
        if (currentState == GameState.Encounter || currentState == GameState.Ritual || currentState == GameState.Deduction)
        {
            statistics.totalPlayTime += Time.deltaTime;
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
    
    private void InitializeGame()
    {
        if (RitualManager.Instance != null)
        {
            RitualManager.Instance.OnRitualComplete += HandleRitualComplete;
        }
        
        if (SpiritualEnergyManager.Instance != null)
        {
            SpiritualEnergyManager.Instance.OnEnergyDepleted += HandleEnergyDepleted;
        }
        
        if (StoryFragmentManager.Instance != null)
        {
            StoryFragmentManager.Instance.OnTrueEndingUnlocked += HandleTrueEndingUnlocked;
        }
        
        Debug.Log("Game initialized");
    }
    
    public void StartNewRun()
    {
        statistics.Reset();
        
        if (SpiritualEnergyManager.Instance != null)
        {
            SpiritualEnergyManager.Instance.ResetEnergy();
        }
        
        if (InstrumentManager.Instance != null)
        {
            InstrumentManager.Instance.FullyRepairInstrument();
        }
        
        if (RunManager.Instance != null)
        {
            RunManager.Instance.SetupRun();
        }
        
        currentState = GameState.RunSetup;
        StartCoroutine(StartFirstEncounterDelayed());
    }
    
    private IEnumerator StartFirstEncounterDelayed()
    {
        yield return new WaitForSeconds(1.5f);
        
        SpiritData firstSpirit = RunManager.Instance?.DrawNextSpirit();
        
        if (firstSpirit != null)
        {
            BeginEncounter(firstSpirit);
        }
        else
        {
            Debug.LogError("No spirits available to start run!");
        }
    }
    
    public void BeginEncounter(SpiritData spirit)
    {
        currentState = GameState.Encounter;
        encounterStartTime = Time.time;
        
        currentEncounterRecord = new EncounterRecord(spirit);
        
        if (EnvironmentController.Instance != null)
        {
            EnvironmentController.Instance.SetupEnvironmentForSpirit(spirit);
        }
        
        if (SpiritEncounterManager.Instance != null)
        {
            SpiritEncounterManager.Instance.BeginEncounter(spirit);
        }
        
        Debug.Log($"Beginning encounter with: {spirit.spiritName}");
    }
    
    /// <summary>
    /// Called when ritual completes
    /// performanceSucceeded = player played melody correctly
    /// wasCorrectRitual = player chose correct ritual type
    /// </summary>
    private void HandleRitualComplete(SpiritData spirit, bool performanceSucceeded, bool wasCorrectRitual)
    {
        currentState = GameState.RunComplete; // Temporary - will auto-progress after brief delay
        
        if (currentEncounterRecord != null)
        {
            currentEncounterRecord.encounterDuration = Time.time - encounterStartTime;
            
            // Record outcome
            if (performanceSucceeded && wasCorrectRitual)
            {
                currentEncounterRecord.ritualSucceeded = true;
                statistics.RecordEncounter(currentEncounterRecord);
                
                Debug.Log($"✅ Perfect Success: {spirit.spiritName} freed!");
                
                // Spirit freed - collect rewards
                HandleSpiritFreed(spirit);
                
                // Check if run is complete
                if (RunManager.Instance != null && RunManager.Instance.IsRunComplete())
                {
                    CompleteRun();
                    return;
                }
            }
            else
            {
                currentEncounterRecord.ritualSucceeded = false;
                statistics.RecordEncounter(currentEncounterRecord);
                
                if (performanceSucceeded && !wasCorrectRitual)
                {
                    Debug.Log($"❌ Wrong Ritual (Performed Well): {spirit.spiritName} - Energy -25");
                }
                else if (!performanceSucceeded && wasCorrectRitual)
                {
                    Debug.Log($"❌ Right Ritual (Failed Performance): {spirit.spiritName} - Energy -15");
                }
                else
                {
                    Debug.Log($"❌ Total Failure: {spirit.spiritName} - Energy -35");
                }
            }
        }
        
        // Auto-progress to next spirit after 3 seconds
        StartCoroutine(ProgressToNextSpiritDelayed(3f));
    }
    
    private void HandleSpiritFreed(SpiritData spirit)
    {
        statistics.totalSpiritsFreed++;
        
        // Notify encounter manager for unlock tracking
        if (SpiritEncounterManager.Instance != null)
        {
            SpiritEncounterManager.Instance.OnSpiritFreed();
        }
        
        // Collect story fragment
        if (spirit.storyFragments != null && spirit.storyFragments.Length > 0 && StoryFragmentManager.Instance != null)
        {
            string fragment = spirit.storyFragments[Random.Range(0, spirit.storyFragments.Length)];
            StoryFragmentManager.Instance.CollectFragment(fragment, spirit.spiritID);
        }
    }
    
    private IEnumerator ProgressToNextSpiritDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        ProgressToNextSpirit();
    }
    
    public void ProgressToNextSpirit()
    {
        // Check if energy depleted
        if (SpiritualEnergyManager.Instance != null && SpiritualEnergyManager.Instance.CurrentEnergy <= 0)
        {
            HandleEnergyDepleted();
            return;
        }
        
        SpiritData nextSpirit = RunManager.Instance?.DrawNextSpirit();
        
        if (nextSpirit != null)
        {
            BeginEncounter(nextSpirit);
        }
        else
        {
            CompleteRun();
        }
    }
    
    private void HandleEnergyDepleted()
    {
        Debug.Log("Energy depleted - triggering game over");
        TriggerGameOver();
    }
    
    private void HandleTrueEndingUnlocked()
    {
        Debug.Log("TRUE ENDING UNLOCKED!");
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ShowPrompt("You have unlocked the True Ending!");
        }
    }
    
    public void TriggerGameOver()
    {
        currentState = GameState.RunComplete;
        
        Debug.Log("=== GAME OVER ===");
        Debug.Log($"Spirits Freed: {statistics.totalSpiritsFreed}");
        Debug.Log($"Cause: Spiritual Energy Depleted");
        
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.Hide();
        }
        
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.ShowRunComplete(
                statistics.totalSpiritsFreed,
                statistics.totalSpiritsEncountered,
                statistics.SuccessRate
            );
        }
    }
    
    private void CompleteRun()
    {
        currentState = GameState.RunComplete;
        
        Debug.Log("=== RUN COMPLETE ===");
        Debug.Log($"Spirits Freed: {statistics.totalSpiritsFreed}");
        Debug.Log($"Success Rate: {statistics.SuccessRate:P}");
        
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.ShowRunComplete(
                statistics.totalSpiritsFreed,
                statistics.totalSpiritsEncountered,
                statistics.SuccessRate
            );
        }
    }
    
    public void TogglePause()
    {
        if (currentState == GameState.Paused)
        {
            Time.timeScale = 1f;
            currentState = GameState.Encounter;
        }
        else if (currentState == GameState.Encounter || currentState == GameState.Ritual)
        {
            Time.timeScale = 0f;
            currentState = GameState.Paused;
        }
    }
    
    public void RestartRun()
    {
        Time.timeScale = 1f;
        StartNewRun();
    }
}