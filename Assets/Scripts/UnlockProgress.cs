using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks player progression for unlock conditions
/// Persists across encounters within a single run
/// </summary>
public class UnlockProgress
{
    // Total questions asked across entire run
    public int totalQuestionsAsked = 0;
    
    // Which specific questions have been asked (by text)
    public List<string> questionsAsked = new List<string>();
    
    // How many spirits have been freed
    public int spiritsFreed = 0;
    
    // How many story fragments collected
    public int storyFragmentsCollected = 0;
    
    /// <summary>
    /// Reset progress (called at start of new run)
    /// </summary>
    public void Reset()
    {
        totalQuestionsAsked = 0;
        questionsAsked.Clear();
        spiritsFreed = 0;
        storyFragmentsCollected = 0;
    }
    
    /// <summary>
    /// Record that a question was asked
    /// </summary>
    public void RecordQuestionAsked(string questionText)
    {
        totalQuestionsAsked++;
        
        if (!questionsAsked.Contains(questionText))
        {
            questionsAsked.Add(questionText);
        }
        
        Debug.Log($"[UnlockProgress] Question asked: '{questionText}' (Total: {totalQuestionsAsked})");
    }
    
    /// <summary>
    /// Record that a spirit was freed
    /// </summary>
    public void RecordSpiritFreed()
    {
        spiritsFreed++;
        Debug.Log($"[UnlockProgress] Spirit freed (Total freed: {spiritsFreed})");
    }
    
    /// <summary>
    /// Record that a fragment was collected
    /// </summary>
    public void RecordFragmentCollected()
    {
        storyFragmentsCollected++;
        Debug.Log($"[UnlockProgress] Fragment collected (Total: {storyFragmentsCollected})");
    }
    
    /// <summary>
    /// Get current progress summary
    /// </summary>
    public string GetProgressSummary()
    {
        return $"Questions: {totalQuestionsAsked} | Spirits Freed: {spiritsFreed} | Fragments: {storyFragmentsCollected}";
    }
}