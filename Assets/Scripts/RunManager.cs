using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages spirit deck for each run - shuffles and deals spirits
/// </summary>
public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }
    
    [Header("Spirit Pool")]
    public SpiritData[] allSpirits;
    public int spiritsPerRun = 15;
    
    [Header("Auto-load from Resources")]
    public bool autoLoadSpirits = true;
    public string spiritsResourcePath = "ScriptableObjects/Spirits";
    
    private Queue<SpiritData> runDeck;
    private List<SpiritData> completedSpirits = new List<SpiritData>();
    private int currentSpiritIndex = 0;
    
    public int SpiritsRemaining => runDeck != null ? runDeck.Count : 0;
    public int SpiritsCompleted => completedSpirits.Count;
    public int TotalSpiritsInRun => spiritsPerRun;
    
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
        if (autoLoadSpirits && (allSpirits == null || allSpirits.Length == 0))
        {
            LoadSpiritsFromResources();
        }
        
        SetupRun();
    }
    
    private void LoadSpiritsFromResources()
    {
        allSpirits = Resources.LoadAll<SpiritData>(spiritsResourcePath);
        
        if (allSpirits.Length == 0)
        {
            Debug.LogWarning($"No spirits found in Resources/{spiritsResourcePath}");
        }
        else
        {
            Debug.Log($"Loaded {allSpirits.Length} spirits from Resources");
        }
    }
    
    public void SetupRun()
    {
        if (allSpirits == null || allSpirits.Length == 0)
        {
            Debug.LogError("No spirits available for run setup!");
            return;
        }
        
        runDeck = new Queue<SpiritData>();
        completedSpirits.Clear();
        currentSpiritIndex = 0;
        
        List<SpiritData> shuffledPool = new List<SpiritData>(allSpirits);
        ShuffleList(shuffledPool);
        
        int count = Mathf.Min(spiritsPerRun, shuffledPool.Count);
        for (int i = 0; i < count; i++)
        {
            runDeck.Enqueue(shuffledPool[i]);
        }
        
        Debug.Log($"Run setup complete: {runDeck.Count} spirits in deck");
    }
    
    public SpiritData DrawNextSpirit()
    {
        if (runDeck == null || runDeck.Count == 0)
        {
            Debug.Log("No more spirits in run deck");
            return null;
        }
        
        SpiritData spirit = runDeck.Dequeue();
        currentSpiritIndex++;
        
        Debug.Log($"Drew spirit: {spirit.spiritName} ({currentSpiritIndex}/{spiritsPerRun})");
        
        return spirit;
    }
    
    public void MarkSpiritComplete(SpiritData spirit)
    {
        if (!completedSpirits.Contains(spirit))
        {
            completedSpirits.Add(spirit);
            Debug.Log($"Spirit completed: {spirit.spiritName} ({completedSpirits.Count} total)");
        }
    }
    
    public bool IsRunComplete()
    {
        return runDeck.Count == 0;
    }
    
    public void RestartRun()
    {
        SetupRun();
    }
    
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    
    public List<SpiritData> GetCompletedSpirits()
    {
        return new List<SpiritData>(completedSpirits);
    }
}