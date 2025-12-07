using UnityEngine;

[CreateAssetMenu(fileName = "Spirit_", menuName = "Echoes/Spirit Data", order = 1)]
public class SpiritData : ScriptableObject
{
    [Header("Identity")]
    public string spiritID;
    public string spiritName; // Display name for UI
    public Personality personality;
    
    [Header("Opening Riddles")]
    [TextArea(2, 4)]
    public string[] openingRiddles;
    
    [Header("Specific Answers")]
    [Tooltip("Specific responses to particular questions - these override fallback responses")]
    public SpiritAnswer[] specificAnswers;
    
    [Header("Fallback Responses")]
    [Tooltip("Generic responses when a question doesn't have specific answers")]
    [TextArea(2, 4)]
    public string[] fallbackResponses;
    
    [Header("Ritual Configuration")]
    public RitualType ritualType;
    
    [Header("Ritual Dialogue")]
    [Tooltip("Custom success dialogue")]
    [TextArea(2, 4)]
    public string[] ritualSuccessDialogue;
    [Tooltip("Custom failure dialogue")]
    [TextArea(2, 4)]
    public string[] ritualFailureDialogue;
    
    [Header("Story Fragments")]
    [Tooltip("Revealed after successful ritual")]
    [TextArea(3, 6)]
    public string[] storyFragments;
    
    [Header("Environmental Tags")]
    public EnvironmentalTag[] environmentTags;
    
    /// <summary>
    /// Get the specific answer for a question, or null if none exists
    /// </summary>
    public SpiritAnswer GetAnswerForQuestion(string questionText)
    {
        if (specificAnswers == null) return null;
        
        foreach (var answer in specificAnswers)
        {
            if (answer.questionText == questionText)
            {
                return answer;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Check if this spirit has a specific answer for a question
    /// </summary>
    public bool HasSpecificAnswer(string questionText)
    {
        return GetAnswerForQuestion(questionText) != null;
    }
}

[System.Serializable]
public class SpiritAnswer
{
    [Tooltip("The question text (must match question from QuestionLibrary)")]
    public string questionText;
    
    [Tooltip("Responses for this specific spirit (2-3 variants recommended for variety)")]
    [TextArea(2, 4)]
    public string[] responses;
    
    [Tooltip("Does this response reveal truth or misdirect?")]
    public bool isTruthful = true;
    
    [Tooltip("Optional: Additional context about this answer")]
    public string designNotes;
    
    /// <summary>
    /// Get a random response from the available variants
    /// </summary>
    public string GetRandomResponse()
    {
        if (responses == null || responses.Length == 0)
        {
            return "...";
        }
        
        return responses[Random.Range(0, responses.Length)];
    }
}

public enum RitualType 
{ 
    CleansingFlame,
    BindingWaters,
    SongOfUnmasking,
    WeepingWillowRite,
    EchoOfBlades,
    FeastOfSilence,
    StoneToAshes,
    LanternLight,
    RenewalRite,
    BindingFlames,
    WarChant,
    BlossomRite,
    MirrorRitual,
    ReleaseArmy,
    RebuildAltar,
    PlaceFlower
}

public enum Personality 
{ 
    Mournful,
    Trickster,
    Hostile,
    Meek,
    Neutral,
    Evasive,
    Agitated
}

public enum EnvironmentalTag 
{ 
    Fire,
    Water,
    Willows,
    Swords,
    Toys,
    Chains,
    Mirrors,
    Books,
    Music,
    Petals,
    Lanterns,
    Ashes,
    Drums,
    Veils
}