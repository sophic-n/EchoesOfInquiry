using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpiritEncounterManager : MonoBehaviour
{
    public static SpiritEncounterManager Instance { get; private set; }
    
    [Header("Configuration")]
    public int maxQuestionsPerSpirit = 5;
    public int questionsToUnlockAdvanced = 10;
    
    [Header("References")]
    public DialogueManager dialogueManager;
    public NoteInputManager noteInputManager;
    public QuestionLibrary globalQuestionLibrary;
    
    private SpiritData currentSpirit;
    private int questionsAsked;
    private int totalQuestionsAskedThisRun = 0;
    private bool advancedQuestionsUnlocked = false;
    public UnlockProgress unlockProgress = new UnlockProgress();
    private List<QuestionTemplate> availableQuestions;
    private Dictionary<string, string> answersGiven = new Dictionary<string, string>();
    private List<string> askedQuestionTexts = new List<string>();
    private Coroutine currentMelodyCoroutine;
    
    public UnlockProgress UnlockProgress => unlockProgress;
    
    public SpiritData CurrentSpirit => currentSpirit;
    public int QuestionsAsked => questionsAsked;
    public int QuestionsRemaining => maxQuestionsPerSpirit - questionsAsked;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    void OnEnable()
    {
        // Note: OnSequenceComplete likely has Action<bool> signature
        // We don't need to subscribe here - CheckForQuestionSequences handles it in Update
    }
    
    void OnDisable()
    {
        // Cleanup if needed
    }
    
    public void BeginEncounter(SpiritData spirit)
    {
        currentSpirit = spirit;
        questionsAsked = 0;
        answersGiven.Clear();
        askedQuestionTexts.Clear();
        
        // Update fragments from persistence
        if (StoryFragmentManager.Instance != null)
        {
            unlockProgress.storyFragmentsCollected = StoryFragmentManager.Instance.TotalFragmentsCollected;
        }
        
        SetupAvailableQuestions();
        ShowOpeningRiddle();
    }
    
    private void SetupAvailableQuestions()
    {
        if (globalQuestionLibrary == null)
        {
            Debug.LogError("Global Question Library not assigned!");
            return;
        }
        
        // Get all questions available to player based on progression
        availableQuestions = globalQuestionLibrary.GetAvailableQuestions(unlockProgress);
        
        // Check if advanced questions just unlocked
        if (!advancedQuestionsUnlocked && totalQuestionsAskedThisRun >= questionsToUnlockAdvanced)
        {
            advancedQuestionsUnlocked = true;
            ShowAdvancedQuestionsUnlockedMessage();
        }
        
        Debug.Log($"Available questions: {availableQuestions.Count} (Total asked this run: {totalQuestionsAskedThisRun})");
    }
    
    private void ShowAdvancedQuestionsUnlockedMessage()
    {
        dialogueManager.ShowPrompt("Advanced questions have been unlocked...");
    }
    
    private void ShowOpeningRiddle()
    {
        if (currentSpirit.openingRiddles != null && currentSpirit.openingRiddles.Length > 0)
        {
            string riddle = currentSpirit.openingRiddles[Random.Range(0, currentSpirit.openingRiddles.Length)];
            dialogueManager.ShowRiddle(riddle);
        }
        else
        {
            dialogueManager.ShowRiddle($"A {currentSpirit.personality.ToString().ToLower()} spirit appears before you...");
        }
        
        dialogueManager.ShowPrompt("Play a note sequence to ask a question...");
    }
    
    void Update()
    {
        if (currentSpirit == null || noteInputManager == null) return;
        
        CheckForQuestionSequences();
    }
    
    private void CheckForQuestionSequences()
    {
        foreach (var question in availableQuestions)
        {
            if (question.noteSequence != null && 
                noteInputManager.CheckSequence(question.noteSequence))
            {
                AskQuestion(question);
                break;
            }
        }
    }
    
    private void CheckForNewlyUnlockedQuestions()
    {
        // Get newly unlocked questions from the global library based on progress
        if (globalQuestionLibrary == null) return;
        
        var allQuestions = globalQuestionLibrary.allQuestions;
        
        foreach (var question in allQuestions)
        {
            // Check if question is now unlocked but wasn't in available list
            if (!availableQuestions.Contains(question) && IsQuestionUnlocked(question))
            {
                availableQuestions.Add(question);
                ShowQuestionUnlockedFeedback(question);
            }
        }
    }
    
    private bool IsQuestionUnlocked(QuestionTemplate question)
    {
        if (globalQuestionLibrary == null) return false;
        return globalQuestionLibrary.IsQuestionUnlocked(question, unlockProgress);
    }
    
    private void ShowQuestionUnlockedFeedback(QuestionTemplate question)
    {
        dialogueManager.ShowPrompt($"A new question reveals itself: {question.questionText}");
    }
    
    private void AskQuestion(QuestionTemplate question)
    {
        if (questionsAsked >= maxQuestionsPerSpirit)
        {
            dialogueManager.ShowResponse(GetPersonalityBasedRefusal());
            return;
        }
        
        questionsAsked++;
        totalQuestionsAskedThisRun++;
        askedQuestionTexts.Add(question.questionText);
        
        // Play the question's specific melody
        PlayQuestionMelody(question);
        
        // Get response - use specific response if available, otherwise fallback
        string response = GetResponseForQuestion(question);
        
        // Store the answer
        answersGiven[question.questionText] = response;
        
        // Display the response with personality influence
        dialogueManager.ShowResponse(response);
        
        if (questionsAsked >= maxQuestionsPerSpirit)
        {
            StartCoroutine(TransitionToRitual());
        }
        else
        {
            dialogueManager.ShowPrompt($"Questions remaining: {QuestionsRemaining}");
        }
    }
    
    private void PlayQuestionMelody(QuestionTemplate question)
    {
        if (question.questionMelody != null && noteInputManager != null && noteInputManager.audioSource != null)
        {
            // Stop any currently playing melody
            if (noteInputManager.audioSource.isPlaying)
            {
                noteInputManager.audioSource.Stop();
            }
            
            // Stop any melody fade coroutine
            if (currentMelodyCoroutine != null)
            {
                StopCoroutine(currentMelodyCoroutine);
            }
            
            // Play the new melody
            noteInputManager.audioSource.PlayOneShot(question.questionMelody);
            Debug.Log($"Playing melody for: {question.questionText}");
        }
    }
    
    private string GetResponseForQuestion(QuestionTemplate question)
    {
        // Check if spirit has a specific answer for this question
        SpiritAnswer specificAnswer = currentSpirit.GetAnswerForQuestion(question.questionText);
        
        if (specificAnswer != null)
        {
            return specificAnswer.GetRandomResponse();
        }
        
        // Check fallback responses
        if (currentSpirit.fallbackResponses != null && currentSpirit.fallbackResponses.Length > 0)
        {
            return currentSpirit.fallbackResponses[Random.Range(0, currentSpirit.fallbackResponses.Length)];
        }
        
        // Final fallback to personality-based response
        return GetPersonalityBasedResponse();
    }
    
    private string GetPersonalityBasedResponse()
    {
        if (currentSpirit == null) return "...";
        
        return currentSpirit.personality switch
        {
            Personality.Mournful => "The spirit seems lost in sorrow...",
            Personality.Trickster => "Why ask what you already know?",
            Personality.Hostile => "You dare demand this of me?",
            Personality.Meek => "I don't remember... it hurts.",
            Personality.Neutral => "The question passes through me like wind.",
            Personality.Evasive => "Perhaps... but not today.",
            Personality.Agitated => "Leave me be!",
            _ => "..."
        };
    }
    
    private string GetPersonalityBasedRefusal()
    {
        if (currentSpirit == null) return "...";
        
        return currentSpirit.personality switch
        {
            Personality.Mournful => "It is not mine to answer.",
            Personality.Trickster => "I tire of your questions.",
            Personality.Hostile => "Enough! Ask no more!",
            Personality.Meek => "I cannot say anymore...",
            Personality.Neutral => "My answers are spent.",
            Personality.Evasive => "I have nothing left to give.",
            Personality.Agitated => "STOP!",
            _ => "..."
        };
    }
    
    private IEnumerator TransitionToRitual()
    {
        yield return new WaitForSeconds(1.5f);
        
        dialogueManager.ShowPrompt("The spirit begins to shimmer...");
        
        yield return new WaitForSeconds(1f);
        
        EndEncounter();
    }
    
    public void EndEncounter()
    {
        // Transition to deduction phase instead of directly to ritual
        if (EnvironmentalRitualManager.Instance != null)
        {
            EnvironmentalRitualManager.Instance.BeginDeductionPhase(currentSpirit, answersGiven);
        }
        else
        {
            Debug.LogError("EnvironmentalRitualManager not found!");
        }
    }
    
    public void OnSpiritFreed()
    {
        if (unlockProgress != null)
        {
            unlockProgress.spiritsFreed++;
            
            // Add all asked questions to progress
            foreach (var question in askedQuestionTexts)
            {
                if (!unlockProgress.questionsAsked.Contains(question))
                {
                    unlockProgress.questionsAsked.Add(question);
                }
            }
            
            unlockProgress.totalQuestionsAsked = totalQuestionsAskedThisRun;
        }
        
        CheckForNewUnlocks();
    }
    
    private void CheckForNewUnlocks()
    {
        // Check if any new questions or rituals should be unlocked
        if (globalQuestionLibrary != null)
        {
            var newlyUnlocked = globalQuestionLibrary.GetNewlyUnlockedQuestions(
                new UnlockProgress(), 
                unlockProgress
            );
            
            if (newlyUnlocked.Count > 0)
            {
                foreach (var question in newlyUnlocked)
                {
                    Debug.Log($"New question unlocked: {question.questionText}");
                }
            }
        }
    }
    
    private void HandleSequenceAttempt(bool success)
    {
        // This is called when player completes a sequence
        // The Update loop will handle matching it to questions
    }
    
    public Dictionary<string, string> GetAnswersGiven()
    {
        return new Dictionary<string, string>(answersGiven);
    }
}