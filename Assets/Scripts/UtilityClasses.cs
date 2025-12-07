using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Utility functions for sequence comparison and game state management
/// </summary>
public static class SequenceUtils
{
    /// <summary>
    /// Compare two integer arrays for equality
    /// </summary>
    public static bool CompareSequences(int[] a, int[] b)
    {
        if (a == null || b == null) return false;
        if (a.Length != b.Length) return false;
        
        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i]) return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Compare two KeyCode arrays for equality
    /// </summary>
    public static bool CompareSequences(KeyCode[] a, KeyCode[] b)
    {
        if (a == null || b == null) return false;
        if (a.Length != b.Length) return false;
        
        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i]) return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Shuffle a list in place using Fisher-Yates algorithm
    /// </summary>
    public static void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    
    /// <summary>
    /// Get a random element from an array
    /// </summary>
    public static T GetRandom<T>(T[] array)
    {
        if (array == null || array.Length == 0)
        {
            Debug.LogWarning("Trying to get random from null or empty array");
            return default(T);
        }
        
        return array[Random.Range(0, array.Length)];
    }
    
    /// <summary>
    /// Get a random element from a list
    /// </summary>
    public static T GetRandom<T>(List<T> list)
    {
        if (list == null || list.Count == 0)
        {
            Debug.LogWarning("Trying to get random from null or empty list");
            return default(T);
        }
        
        return list[Random.Range(0, list.Count)];
    }
}

/// <summary>
/// Manages instrument state and note durability
/// </summary>
[System.Serializable]
public class InstrumentState
{
    public List<string> availableNotes = new List<string>();
    public Dictionary<string, float> noteDurability = new Dictionary<string, float>();
    
    public InstrumentState()
    {
        InitializeDefaultNotes();
    }
    
    private void InitializeDefaultNotes()
    {
        string[] defaultNotes = { "J", "K", "L", "I" };
        
        foreach (string note in defaultNotes)
        {
            availableNotes.Add(note);
            noteDurability[note] = 1.0f;
        }
    }
    
    public void DamageNote(string note, float amount)
    {
        if (noteDurability.ContainsKey(note))
        {
            noteDurability[note] = Mathf.Max(0f, noteDurability[note] - amount);
            
            if (noteDurability[note] <= 0f)
            {
                RemoveNote(note);
            }
        }
    }
    
    public void RemoveNote(string note)
    {
        if (availableNotes.Contains(note))
        {
            availableNotes.Remove(note);
            Debug.Log($"Note {note} has been lost!");
        }
    }
    
    public void RepairNote(string note, float amount)
    {
        if (noteDurability.ContainsKey(note))
        {
            noteDurability[note] = Mathf.Min(1.0f, noteDurability[note] + amount);
        }
    }
    
    public bool IsNoteAvailable(string note)
    {
        return availableNotes.Contains(note) && noteDurability[note] > 0f;
    }
    
    public float GetNoteDurability(string note)
    {
        return noteDurability.ContainsKey(note) ? noteDurability[note] : 0f;
    }
}

/// <summary>
/// Stores data about a single spirit encounter
/// </summary>
[System.Serializable]
public class EncounterRecord
{
    public string spiritID;
    public string spiritName;
    public Dictionary<string, string> questionsAsked = new Dictionary<string, string>();
    public bool ritualSucceeded;
    public float encounterDuration;
    public RitualType ritualType;
    
    public EncounterRecord(SpiritData spirit)
    {
        spiritID = spirit.spiritID;
        spiritName = spirit.spiritName;
        ritualType = spirit.ritualType;
    }
    
    public void AddAnswer(string question, string answer)
    {
        if (!questionsAsked.ContainsKey(question))
        {
            questionsAsked.Add(question, answer);
        }
    }
}

/// <summary>
/// Tracks overall game statistics
/// </summary>
[System.Serializable]
public class GameStatistics
{
    public int totalSpiritsEncountered;
    public int totalSpiritsFreed;
    public int totalRitualsFailed;
    public int totalQuestionsAsked;
    public float totalPlayTime;
    public List<EncounterRecord> encounterHistory = new List<EncounterRecord>();
    
    public float SuccessRate => totalSpiritsEncountered > 0 
        ? (float)totalSpiritsFreed / totalSpiritsEncountered 
        : 0f;
    
    public void RecordEncounter(EncounterRecord record)
    {
        totalSpiritsEncountered++;
        
        if (record.ritualSucceeded)
        {
            totalSpiritsFreed++;
        }
        else
        {
            totalRitualsFailed++;
        }
        
        totalQuestionsAsked += record.questionsAsked.Count;
        encounterHistory.Add(record);
    }
    
    public void Reset()
    {
        totalSpiritsEncountered = 0;
        totalSpiritsFreed = 0;
        totalRitualsFailed = 0;
        totalQuestionsAsked = 0;
        totalPlayTime = 0f;
        encounterHistory.Clear();
    }
}