using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Manages instrument durability and note availability
/// Failed rituals can damage instrument, locking note options
/// </summary>
public class InstrumentManager : MonoBehaviour
{
    public static InstrumentManager Instance { get; private set; }
    
    [Header("Instrument Configuration")]
    [SerializeField] private InstrumentState instrumentState;
    [SerializeField] private float baseDurability = 100f;
    [SerializeField] private float durabilityLossPerFailure = 15f;
    [SerializeField] private float noteBreakThreshold = 20f; // durability % when note locks
    
    [Header("Note Configuration")]
    [SerializeField] private List<NoteConfig> noteConfigs = new List<NoteConfig>();
    
    [Header("UI References")]
    [SerializeField] private Transform noteStatusPanel;
    [SerializeField] private GameObject noteStatusPrefab;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip stringBreakSound;
    [SerializeField] private AudioClip instrumentDamageSound;
    [SerializeField] private AudioClip repairSound;
    
    private Dictionary<KeyCode, float> noteDurabilities = new Dictionary<KeyCode, float>();
    private Dictionary<KeyCode, bool> noteAvailability = new Dictionary<KeyCode, bool>();
    private Dictionary<KeyCode, GameObject> noteUIElements = new Dictionary<KeyCode, GameObject>();
    
    public event Action<KeyCode> OnNoteBreak;
    public event Action<KeyCode> OnNoteRepaired;
    public event Action OnInstrumentDamaged;
    
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
        InitializeInstrument();
        UpdateUI();
    }
    
    private void InitializeInstrument()
    {
        // Initialize all notes with full durability
        if (NoteInputManager.Instance != null)
        {
            foreach (var key in NoteInputManager.Instance.noteKeys)
            {
                noteDurabilities[key] = baseDurability;
                noteAvailability[key] = true;
            }
            
            Debug.Log($"Instrument initialized with {NoteInputManager.Instance.noteKeys.Length} notes");
        }
        else
        {
            Debug.LogError("NoteInputManager not found! Cannot initialize instrument.");
        }
        
        // Setup note configs if not assigned
        if (noteConfigs.Count == 0 && NoteInputManager.Instance != null)
        {
            foreach (var key in NoteInputManager.Instance.noteKeys)
            {
                noteConfigs.Add(new NoteConfig
                {
                    key = key,
                    noteName = key.ToString(),
                    noteSymbol = GetNoteSymbol(key)
                });
            }
            
            Debug.Log($"Created {noteConfigs.Count} note configs");
        }
    }
    
    private string GetNoteSymbol(KeyCode key)
    {
        // Map keys to musical notation or symbols
        Dictionary<KeyCode, string> symbolMap = new Dictionary<KeyCode, string>
        {
            { KeyCode.Z, "♪" },
            { KeyCode.X, "♫" },
            { KeyCode.C, "♬" },
            { KeyCode.V, "♩" },
            { KeyCode.B, "♭" },
            { KeyCode.N, "♮" },
            { KeyCode.M, "♯" },
            { KeyCode.Comma, "♩♩" }
        };
        
        return symbolMap.ContainsKey(key) ? symbolMap[key] : key.ToString();
    }
    
    public void DamageInstrument(float damageAmount)
    {
        OnInstrumentDamaged?.Invoke();
        
        // Damage random notes
        var availableNotes = noteAvailability.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
        
        if (availableNotes.Count == 0) return;
        
        int notesToDamage = Mathf.Max(1, Mathf.RoundToInt(availableNotes.Count * 0.3f));
        
        for (int i = 0; i < notesToDamage; i++)
        {
            var noteKey = availableNotes[UnityEngine.Random.Range(0, availableNotes.Count)];
            DamageNote(noteKey, damageAmount);
            availableNotes.Remove(noteKey);
        }
        
        if (instrumentDamageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(instrumentDamageSound);
        }
    }
    
    public void DamageNote(KeyCode key, float damage)
    {
        if (!noteDurabilities.ContainsKey(key)) return;
        
        noteDurabilities[key] = Mathf.Max(0, noteDurabilities[key] - damage);
        
        Debug.Log($"Note {key} damaged: {noteDurabilities[key]:F1}%");
        
        // Check if note should break
        if (noteDurabilities[key] <= noteBreakThreshold && noteAvailability[key])
        {
            BreakNote(key);
        }
        
        UpdateUI();
    }
    
    private void BreakNote(KeyCode key)
    {
        noteAvailability[key] = false;
        
        Debug.LogWarning($"Note {key} has broken and is no longer available!");
        
        OnNoteBreak?.Invoke(key);
        
        if (stringBreakSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(stringBreakSound);
        }
        
        // Disable this note in the input manager
        if (NoteInputManager.Instance != null)
        {
            // Could add method to NoteInputManager to disable specific keys
        }
        
        UpdateUI();
    }
    
    public void RepairNote(KeyCode key, float repairAmount)
    {
        if (!noteDurabilities.ContainsKey(key)) return;
        
        float previousDurability = noteDurabilities[key];
        noteDurabilities[key] = Mathf.Min(baseDurability, noteDurabilities[key] + repairAmount);
        
        // Re-enable note if it was broken
        if (previousDurability <= noteBreakThreshold && noteDurabilities[key] > noteBreakThreshold)
        {
            noteAvailability[key] = true;
            OnNoteRepaired?.Invoke(key);
            
            if (repairSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(repairSound);
            }
            
            Debug.Log($"Note {key} has been repaired!");
        }
        
        UpdateUI();
    }
    
    public void RepairAllNotes(float repairAmount)
    {
        foreach (var key in noteDurabilities.Keys.ToList())
        {
            RepairNote(key, repairAmount);
        }
    }
    
    public void FullyRepairInstrument()
    {
        foreach (var key in noteDurabilities.Keys.ToList())
        {
            noteDurabilities[key] = baseDurability;
            noteAvailability[key] = true;
        }
        
        Debug.Log("Instrument fully repaired!");
        UpdateUI();
    }
    
    public bool IsNoteAvailable(KeyCode key)
    {
        return noteAvailability.ContainsKey(key) && noteAvailability[key];
    }
    
    public float GetNoteDurability(KeyCode key)
    {
        return noteDurabilities.ContainsKey(key) ? noteDurabilities[key] : 0f;
    }
    
    public int GetAvailableNoteCount()
    {
        return noteAvailability.Count(kvp => kvp.Value);
    }
    
    public List<KeyCode> GetAvailableNotes()
    {
        return noteAvailability.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
    }
    
    private void UpdateUI()
    {
        if (noteStatusPanel == null || noteStatusPrefab == null) return;
        
        foreach (var noteConfig in noteConfigs)
        {
            if (!noteUIElements.ContainsKey(noteConfig.key))
            {
                var uiElement = Instantiate(noteStatusPrefab, noteStatusPanel);
                noteUIElements[noteConfig.key] = uiElement;
            }
            
            var ui = noteUIElements[noteConfig.key];
            var text = ui.GetComponentInChildren<Text>();
            var slider = ui.GetComponentInChildren<Slider>();
            var image = ui.GetComponent<Image>();
            
            if (text != null)
            {
                text.text = $"{noteConfig.noteSymbol} ({noteConfig.key})";
            }
            
            if (slider != null)
            {
                float durabilityPercent = noteDurabilities[noteConfig.key] / baseDurability;
                slider.value = durabilityPercent;
            }
            
            if (image != null)
            {
                image.color = noteAvailability[noteConfig.key] ? Color.white : Color.gray;
            }
        }
    }
    
    public void ApplyRitualFailureDamage()
    {
        DamageInstrument(durabilityLossPerFailure);
    }
}

[Serializable]
public class NoteConfig
{
    public KeyCode key;
    public string noteName;
    public string noteSymbol;
    public Color noteColor = Color.white;
}