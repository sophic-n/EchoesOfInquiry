using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// GLOBAL library of ALL questions in the game
/// All unlock logic is here - no per-spirit configuration needed
/// </summary>
[CreateAssetMenu(fileName = "GlobalQuestionLibrary", menuName = "Echoes/Global Question Library", order = 2)]
public class QuestionLibrary : ScriptableObject
{
    [Header("All Available Questions")]
    [Tooltip("Every question players can ask - available globally")]
    public QuestionTemplate[] allQuestions;
    
    /// <summary>
    /// Find a question template by its text
    /// </summary>
    public QuestionTemplate FindQuestion(string questionText)
    {
        foreach (var q in allQuestions)
        {
            if (q.questionText == questionText)
                return q;
        }
        
        return null;
    }
    
    /// <summary>
    /// Get the melody for a specific question
    /// </summary>
    public AudioClip GetMelodyForQuestion(string questionText)
    {
        var template = FindQuestion(questionText);
        return template?.questionMelody;
    }
    
    /// <summary>
    /// Get all questions that should be available to the player right now
    /// Based on unlock conditions only - works for ANY spirit
    /// </summary>
    public List<QuestionTemplate> GetAvailableQuestions(UnlockProgress progress)
    {
        List<QuestionTemplate> available = new List<QuestionTemplate>();
        
        foreach (var question in allQuestions)
        {
            if (IsQuestionUnlocked(question, progress))
            {
                available.Add(question);
            }
        }
        
        return available;
    }
    
    /// <summary>
    /// Check if a specific question is unlocked based on player progress
    /// This is the ONLY place unlock logic lives
    /// </summary>
    public bool IsQuestionUnlocked(QuestionTemplate question, UnlockProgress progress)
    {
        // Always available questions
        if (question.unlockCondition == null || question.unlockCondition.unlockType == UnlockType.AlwaysAvailable)
        {
            return true;
        }
        
        // Check unlock condition
        switch (question.unlockCondition.unlockType)
        {
            case UnlockType.MinimumQuestionsAsked:
                return progress.totalQuestionsAsked >= question.unlockCondition.requiredQuestionCount;
                
            case UnlockType.RequiresSpecificAnswer:
                // Check if player has asked the required question
                return progress.questionsAsked.Contains(question.unlockCondition.requiredQuestionText);
                
            case UnlockType.RequiresMultipleAnswers:
                // Check if player has asked ALL required questions
                if (question.unlockCondition.requiredQuestions == null || 
                    question.unlockCondition.requiredQuestions.Length == 0)
                    return false;
                    
                foreach (string reqQuestion in question.unlockCondition.requiredQuestions)
                {
                    if (!progress.questionsAsked.Contains(reqQuestion))
                        return false;
                }
                return true;
                
            case UnlockType.RequiresSpiritsFreed:
                return progress.spiritsFreed >= question.unlockCondition.requiredSpiritCount;
                
            case UnlockType.RequiresStoryFragments:
                return progress.storyFragmentsCollected >= question.unlockCondition.requiredFragmentCount;
                
            default:
                return false;
        }
    }
    
    /// <summary>
    /// Get only basic (always available) questions
    /// </summary>
    public List<QuestionTemplate> GetBasicQuestions()
    {
        return allQuestions.Where(q => q.unlockCondition == null || 
                                       q.unlockCondition.unlockType == UnlockType.AlwaysAvailable).ToList();
    }
    
    /// <summary>
    /// Get only unlockable questions
    /// </summary>
    public List<QuestionTemplate> GetUnlockableQuestions()
    {
        return allQuestions.Where(q => q.unlockCondition != null && 
                                       q.unlockCondition.unlockType != UnlockType.AlwaysAvailable).ToList();
    }
    
    /// <summary>
    /// Get newly unlocked questions based on progress change
    /// </summary>
    public List<QuestionTemplate> GetNewlyUnlockedQuestions(UnlockProgress oldProgress, UnlockProgress newProgress)
    {
        List<QuestionTemplate> newlyUnlocked = new List<QuestionTemplate>();
        
        foreach (var question in allQuestions)
        {
            bool wasUnlocked = IsQuestionUnlocked(question, oldProgress);
            bool isNowUnlocked = IsQuestionUnlocked(question, newProgress);
            
            if (!wasUnlocked && isNowUnlocked)
            {
                newlyUnlocked.Add(question);
            }
        }
        
        return newlyUnlocked;
    }
}

[System.Serializable]
public class NoteSequence
{
    [Tooltip("Display name for this sequence")]
    public string sequenceName;
    
    [Tooltip("The keys that must be pressed in order (use Z,X,C,V,B,N,M,Comma)")]
    public KeyCode[] notes;
    
    [Tooltip("Maximum time allowed between notes")]
    [Range(0.3f, 2f)]
    public float maxTimeBetweenNotes = 0.8f;
    
    [Tooltip("Visual representation (optional, for UI display)")]
    public string sequenceNotation; // e.g. "Z-X-C" or "♪-♫-♪"
}

[System.Serializable]
public class QuestionTemplate
{
    [Header("Question Identity")]
    public string questionText;
    
    [Header("Input Sequence")]
    [Tooltip("The key sequence to ask this question (Z,X,C,V,B,N,M,Comma)")]
    public KeyCode[] noteSequence;
    
    [Header("Audio")]
    [Tooltip("The melody that plays when this question is asked")]
    public AudioClip questionMelody;
    
    [Header("Properties")]
    [Tooltip("Maximum time between notes in the sequence")]
    [Range(0.3f, 2f)]
    public float maxTimeBetweenNotes = 0.8f;
    
    [Tooltip("Is this a truth-seeking question?")]
    public bool isTruthSeeking = true;
    
    [Tooltip("Difficulty level (for reference)")]
    [Range(1, 5)]
    public int difficulty = 1;
    
    [Header("Unlock Condition")]
    [Tooltip("How does this question unlock? Leave null for always available")]
    public UnlockCondition unlockCondition;
    
    [Header("Visual Representation")]
    [Tooltip("For UI display (e.g., Z-X-C)")]
    public string sequenceNotation;
    
    [Header("Category")]
    [Tooltip("For organization: Death, Love, Betrayal, Regret, etc.")]
    public string category;
}

[System.Serializable]
public class UnlockCondition
{
    public UnlockType unlockType = UnlockType.AlwaysAvailable;
    
    [Tooltip("For MinimumQuestionsAsked - how many questions must be asked")]
    public int requiredQuestionCount = 5;
    
    [Tooltip("For RequiresSpecificAnswer - which question must be asked")]
    public string requiredQuestionText = "";
    
    [Tooltip("For RequiresMultipleAnswers - which questions must all be asked")]
    public string[] requiredQuestions;
    
    [Tooltip("For RequiresSpiritsFreed - how many spirits must be freed")]
    public int requiredSpiritCount = 3;
    
    [Tooltip("For RequiresStoryFragments - how many fragments must be collected")]
    public int requiredFragmentCount = 2;
}

public enum UnlockType
{
    AlwaysAvailable,           // Question available from start
    MinimumQuestionsAsked,     // After asking X questions total
    RequiresSpecificAnswer,    // After asking a specific question
    RequiresMultipleAnswers,   // After asking ALL of several questions
    RequiresSpiritsFreed,      // After freeing X spirits
    RequiresStoryFragments     // After collecting X story fragments
}