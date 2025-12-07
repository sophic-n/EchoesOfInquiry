using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

/// <summary>
/// Manages story fragment collection and persistence across runs
/// Tracks progress toward true ending
/// </summary>
public class StoryFragmentManager : MonoBehaviour
{
    public static StoryFragmentManager Instance { get; private set; }
    
    [Header("Configuration")]
    [SerializeField] private int fragmentsNeededForTrueEnding = 30;
    [SerializeField] private bool enableAutoSave = true;
    
    [Header("Save Configuration")]
    [SerializeField] private string saveFileName = "story_progress.json";
    
    private StoryProgress storyProgress = new StoryProgress();
    
    public int TotalFragmentsCollected => storyProgress.collectedFragments.Count;
    public int FragmentsNeeded => fragmentsNeededForTrueEnding;
    public float CompletionPercentage => (float)TotalFragmentsCollected / fragmentsNeededForTrueEnding;
    public bool IsTrueEndingUnlocked => TotalFragmentsCollected >= fragmentsNeededForTrueEnding;
    
    public event Action<string> OnFragmentCollected;
    public event Action OnTrueEndingUnlocked;
    
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
        LoadProgress();
    }
    
    void OnApplicationQuit()
    {
        if (enableAutoSave)
        {
            SaveProgress();
        }
    }
    
    public void CollectFragment(string fragmentText, string spiritID)
    {
        // Create unique fragment ID
        string fragmentID = $"{spiritID}_{fragmentText.GetHashCode()}";
        
        // Check if already collected
        if (storyProgress.collectedFragments.ContainsKey(fragmentID))
        {
            Debug.Log($"Fragment already collected: {fragmentID}");
            return;
        }
        
        // Add new fragment
        var fragment = new StoryFragment
        {
            fragmentID = fragmentID,
            spiritID = spiritID,
            fragmentText = fragmentText,
            collectedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
        
        storyProgress.collectedFragments.Add(fragmentID, fragment);
        storyProgress.totalFragmentsCollected++;
        
        Debug.Log($"Story Fragment Collected: {TotalFragmentsCollected}/{fragmentsNeededForTrueEnding}");
        
        OnFragmentCollected?.Invoke(fragmentText);
        
        // Check for true ending unlock
        if (TotalFragmentsCollected >= fragmentsNeededForTrueEnding && !storyProgress.trueEndingUnlocked)
        {
            UnlockTrueEnding();
        }
        
        if (enableAutoSave)
        {
            SaveProgress();
        }
    }
    
    private void UnlockTrueEnding()
    {
        storyProgress.trueEndingUnlocked = true;
        storyProgress.trueEndingUnlockDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        Debug.Log("TRUE ENDING UNLOCKED!");
        
        OnTrueEndingUnlocked?.Invoke();
        
        if (enableAutoSave)
        {
            SaveProgress();
        }
    }
    
    public bool HasFragment(string fragmentID)
    {
        return storyProgress.collectedFragments.ContainsKey(fragmentID);
    }
    
    public List<StoryFragment> GetAllFragments()
    {
        return new List<StoryFragment>(storyProgress.collectedFragments.Values);
    }
    
    public List<StoryFragment> GetFragmentsBySpirit(string spiritID)
    {
        List<StoryFragment> fragments = new List<StoryFragment>();
        foreach (var fragment in storyProgress.collectedFragments.Values)
        {
            if (fragment.spiritID == spiritID)
            {
                fragments.Add(fragment);
            }
        }
        return fragments;
    }
    
    public void SaveProgress()
    {
        try
        {
            string savePath = Path.Combine(Application.persistentDataPath, saveFileName);
            string json = JsonUtility.ToJson(storyProgress, true);
            File.WriteAllText(savePath, json);
            
            Debug.Log($"Story progress saved to: {savePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save story progress: {e.Message}");
        }
    }
    
    public void LoadProgress()
    {
        try
        {
            string savePath = Path.Combine(Application.persistentDataPath, saveFileName);
            
            if (File.Exists(savePath))
            {
                string json = File.ReadAllText(savePath);
                storyProgress = JsonUtility.FromJson<StoryProgress>(json);
                
                Debug.Log($"Story progress loaded: {TotalFragmentsCollected} fragments");
            }
            else
            {
                Debug.Log("No save file found. Starting fresh.");
                storyProgress = new StoryProgress();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load story progress: {e.Message}");
            storyProgress = new StoryProgress();
        }
    }
    
    public void ResetProgress()
    {
        storyProgress = new StoryProgress();
        SaveProgress();
        Debug.Log("Story progress reset.");
    }
    
    public void DebugPrintAllFragments()
    {
        Debug.Log($"=== COLLECTED STORY FRAGMENTS ({TotalFragmentsCollected}) ===");
        foreach (var fragment in storyProgress.collectedFragments.Values)
        {
            Debug.Log($"[{fragment.spiritID}] {fragment.fragmentText.Substring(0, Mathf.Min(50, fragment.fragmentText.Length))}...");
        }
    }
}

[Serializable]
public class StoryProgress
{
    public Dictionary<string, StoryFragment> collectedFragments = new Dictionary<string, StoryFragment>();
    public int totalFragmentsCollected = 0;
    public bool trueEndingUnlocked = false;
    public string trueEndingUnlockDate = "";
    public int totalRunsCompleted = 0;
    public int totalSpiritsFreed = 0;
}

[Serializable]
public class StoryFragment
{
    public string fragmentID;
    public string spiritID;
    public string fragmentText;
    public string collectedDate;
}