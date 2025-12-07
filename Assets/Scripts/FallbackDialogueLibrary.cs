using UnityEngine;

/// <summary>
/// Centralized library of fallback dialogue
/// Provides automatic responses when spirits don't have custom ones
/// Attach to GameManagers or make it a ScriptableObject
/// </summary>
public static class FallbackDialogueLibrary
{
    /// <summary>
    /// Get fallback responses based on spirit personality
    /// Called when question has no specific responses AND spirit has no fallback array
    /// </summary>
    public static string GetPersonalityFallback(Personality personality)
    {
        switch (personality)
        {
            case Personality.Mournful:
                string[] mournfulResponses = new string[]
                {
                    "It is not mine to answer.",
                    "The silence is heavier than truth.",
                    "That question weighs heavier than I can bear…",
                    "Not every wound has words.",
                    "The silence answers for me."
                };
                return mournfulResponses[Random.Range(0, mournfulResponses.Length)];
                
            case Personality.Trickster:
                string[] tricksterResponses = new string[]
                {
                    "Why ask what you already know?",
                    "Perhaps I'll answer… if your melody pleases me.",
                    "You'll find only riddles where you dig.",
                    "Ah, that's a question worth asking… but not of me.",
                    "What would you say, if you were me?",
                    "Perhaps yes. Perhaps no. Which answer do you seek?"
                };
                return tricksterResponses[Random.Range(0, tricksterResponses.Length)];
                
            case Personality.Hostile:
                string[] hostileResponses = new string[]
                {
                    "You dare demand this of me?",
                    "Rot with your questions!",
                    "Your strings scrape like knives — stop!",
                    "Do not pry where you don't belong.",
                    "Your strings bite, cultivator. Play another.",
                    "I owe you no truth."
                };
                return hostileResponses[Random.Range(0, hostileResponses.Length)];
                
            case Personality.Meek:
                string[] meekResponses = new string[]
                {
                    "I don't remember… it hurts.",
                    "Can I tell you later?",
                    "The words hide from me.",
                    "I… I don't know how to answer that.",
                    "Please… not that question.",
                    "The memory fades, forgive me."
                };
                return meekResponses[Random.Range(0, meekResponses.Length)];
                
            case Personality.Evasive:
                string[] evasiveResponses = new string[]
                {
                    "Ask the right thing… not all questions are welcome.",
                    "The truth lies elsewhere.",
                    "Listen closer…",
                    "Do you hear the weeping willow?",
                    "The stars remember what I cannot."
                };
                return evasiveResponses[Random.Range(0, evasiveResponses.Length)];
                
            case Personality.Agitated:
                string[] agitatedResponses = new string[]
                {
                    "Every step treads deeper into my grave.",
                    "Your melody stirs old wounds.",
                    "The spirit recoils.",
                    "Do not pry into what is mine!",
                    "You play lies upon your strings!"
                };
                return agitatedResponses[Random.Range(0, agitatedResponses.Length)];
                
            case Personality.Neutral:
            default:
                string[] neutralResponses = new string[]
                {
                    "The question passes through me like wind.",
                    "Not every truth belongs to me.",
                    "This is not my burden to tell.",
                    "The strings pluck at emptiness.",
                    "Another asks this, but not I."
                };
                return neutralResponses[Random.Range(0, neutralResponses.Length)];
        }
    }
    
    /// <summary>
    /// Get ritual success dialogue
    /// Called when spirit has no custom success dialogue
    /// </summary>
    public static string GetRitualSuccessDialogue(RitualType ritualType, Personality personality)
    {
        // Ritual-specific messages
        string[] ritualMessages = ritualType switch
        {
            RitualType.CleansingFlame => new string[]
            {
                "The flame consumes the ash, leaving only peace.",
                "The fire purifies… I am free from regret.",
                "The embers fade, and with them, my sorrow."
            },
            RitualType.BindingWaters => new string[]
            {
                "The river releases me… I can finally drift away.",
                "The waters part, no longer binding.",
                "The current weakens… I am free to flow."
            },
            RitualType.WeepingWillowRite => new string[]
            {
                "The willow's roots untangle from my heart.",
                "The grove awakens, roots untangle sorrow.",
                "The tree weeps no more… neither do I."
            },
            RitualType.EchoOfBlades => new string[]
            {
                "The steel falls silent. My battle is done.",
                "The echo fades… my oath fulfilled.",
                "The blades rest. So can I."
            },
            _ => new string[]
            {
                "The chains fall away… I can finally rest.",
                "The air softens… the grove leans closer.",
                "The whispers hush, as if listening.",
                "The sigil glows, threads of fate weaving shut."
            }
        };
        
        return ritualMessages[Random.Range(0, ritualMessages.Length)];
    }
    
    /// <summary>
    /// Get ritual failure dialogue
    /// Called when spirit has no custom failure dialogue
    /// </summary>
    public static string GetRitualFailureDialogue(Personality personality)
    {
        switch (personality)
        {
            case Personality.Mournful:
                string[] mournfulFails = new string[]
                {
                    "Your melody falters — as did I.",
                    "Not yet… the song is incomplete.",
                    "The notes fall like tears, unfinished."
                };
                return mournfulFails[Random.Range(0, mournfulFails.Length)];
                
            case Personality.Hostile:
            case Personality.Agitated:
                string[] hostileFails = new string[]
                {
                    "You dare to bind me with broken notes?!",
                    "The grove weeps with me! You have failed!",
                    "Wrong… wrong… the truth rots on your tongue."
                };
                return hostileFails[Random.Range(0, hostileFails.Length)];
                
            case Personality.Meek:
                string[] meekFails = new string[]
                {
                    "I… I'm still here. The binding didn't work.",
                    "Please… try again?",
                    "The ritual fades before it completes…"
                };
                return meekFails[Random.Range(0, meekFails.Length)];
                
            default:
                string[] genericFails = new string[]
                {
                    "The air shudders; the spirit recoils.",
                    "Branches creak, water blackens, the note twists.",
                    "Your song falters… the truth is not yet mine.",
                    "You dare to bind me again?"
                };
                return genericFails[Random.Range(0, genericFails.Length)];
        }
    }
    
    /// <summary>
    /// Get refusal message when player tries to ask more questions than allowed
    /// </summary>
    public static string GetQuestionRefusal(Personality personality)
    {
        switch (personality)
        {
            case Personality.Mournful:
                return "No more questions… please. The memories fade like smoke.";
                
            case Personality.Trickster:
                return "That's quite enough questions for one spirit, don't you think?";
                
            case Personality.Hostile:
                return "ENOUGH! Your strings wound me with every note!";
                
            case Personality.Meek:
                return "I… I can't answer anymore… the words hurt too much…";
                
            case Personality.Evasive:
                return "The truth hides now. You've asked enough.";
                
            case Personality.Agitated:
                return "Your melody falters — as did I. No more.";
                
            case Personality.Neutral:
            default:
                return "Your scroll has no more ink. The spirit waits in silence.";
        }
    }
    
    /// <summary>
    /// Get message for wrong sequence input
    /// </summary>
    public static string GetSequenceFailureMessage(Personality personality)
    {
        switch (personality)
        {
            case Personality.Mournful:
                string[] mournfulFails = new string[]
                {
                    "Your song falters… the truth is not yet mine.",
                    "Your melody falters — as did I.",
                    "The silence answers for me."
                };
                return mournfulFails[Random.Range(0, mournfulFails.Length)];
                
            case Personality.Hostile:
            case Personality.Agitated:
                string[] hostileFails = new string[]
                {
                    "You dare play those notes at me?!",
                    "Your strings falter. Do you not hear me?",
                    "Wrong… wrong… the truth rots on your tongue.",
                    "You play lies upon your strings!"
                };
                return hostileFails[Random.Range(0, hostileFails.Length)];
                
            case Personality.Meek:
                string[] meekFails = new string[]
                {
                    "Your song wounds me as theirs once did…",
                    "Silence me, and you silence yourself.",
                    "The words hide from me."
                };
                return meekFails[Random.Range(0, meekFails.Length)];
                
            default:
                string[] genericFails = new string[]
                {
                    "The spirit does not understand that melody.",
                    "Your notes fall into silence, unanswered.",
                    "The sequence fades, incomplete.",
                    "That rhythm means nothing here.",
                    "The spirit recoils.",
                    "A fragment of memory surfaces… but not the one you sought."
                };
                return genericFails[Random.Range(0, genericFails.Length)];
        }
    }
}