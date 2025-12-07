using System.Collections;
using UnityEngine;
using System;
using TMPro;

public class RitualManager : MonoBehaviour
{
    public static RitualManager Instance { get; private set; }
    
    [Header("UI References")]
    public TMP_Text ritualPromptText;
    public DialogueManager dialogueManager;
    
    [Header("Ritual Configuration")]
    public float showNoteDelay = 0.6f;
    public float timeoutMultiplier = 4f;
    
    [Header("Audio")]
    public AudioSource ritualAudioSource;
    public AudioClip successClip;
    public AudioClip failureClip;
    
    private SpiritData currentSpirit;
    private RitualType chosenRitualType;
    private RitualType correctRitualType;
    private KeyCode[] currentMelody;
    private bool inRitual = false;
    private bool ritualPerformanceSucceeded = false;
    private bool wasCorrectRitual = false;
    
    public event Action<SpiritData, bool, bool> OnRitualComplete; // spirit, performanceSucceeded, wasCorrectRitual
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    /// <summary>
    /// Begin ritual with chosen ritual type from environmental altar
    /// </summary>
    public void BeginRitual(SpiritData spirit, RitualType chosenRitual)
    {

        if (inRitual)
        {
            Debug.LogWarning("Already in a ritual!");
            return;
        }
        
        currentSpirit = spirit;
        chosenRitualType = chosenRitual;
        correctRitualType = spirit.ritualType;
        wasCorrectRitual = (chosenRitual == correctRitualType);
        ritualPerformanceSucceeded = false;
        
        currentMelody = GenerateRitualMelody(chosenRitual);
        
        StartCoroutine(ExecuteRitual());
    }
    
     private KeyCode[] GenerateRitualMelody(RitualType ritualType)
    {
        KeyCode[] availableKeys = NoteInputManager.Instance.noteKeys;
        
        int melodyLength = ritualType switch
        {
            RitualType.CleansingFlame => 4,
            RitualType.BindingWaters => 5,
            RitualType.SongOfUnmasking => 4,
            RitualType.WeepingWillowRite => 5,
            RitualType.EchoOfBlades => 4,
            RitualType.FeastOfSilence => 6,
            RitualType.StoneToAshes => 4,
            RitualType.LanternLight => 6,
            RitualType.RenewalRite => 5,
            RitualType.BindingFlames => 6,
            RitualType.WarChant => 4,
            RitualType.BlossomRite => 4,
            RitualType.MirrorRitual => 5,
            RitualType.ReleaseArmy => 6,
            RitualType.RebuildAltar => 5,
            RitualType.PlaceFlower => 6,
            _ => 5
        };
        
        KeyCode[] melody = new KeyCode[melodyLength];
        
        switch (ritualType)
        {
            case RitualType.CleansingFlame:
                for (int i = 0; i < melodyLength; i++)
                {
                    melody[i] = availableKeys[Mathf.Min(i, availableKeys.Length - 1)];
                }
                break;
                
            case RitualType.BindingWaters:
                for (int i = 0; i < melodyLength; i++)
                {
                    melody[i] = availableKeys[Mathf.Max(0, availableKeys.Length - 1 - i)];
                }
                break;
                
            case RitualType.MirrorRitual:
                int half = melodyLength / 2;
                for (int i = 0; i < half; i++)
                {
                    KeyCode key = availableKeys[UnityEngine.Random.Range(0, availableKeys.Length)];
                    melody[i] = key;
                    melody[melodyLength - 1 - i] = key;
                }
                if (melodyLength % 2 != 0)
                {
                    melody[half] = availableKeys[UnityEngine.Random.Range(0, availableKeys.Length)];
                }
                break;
                
            default:
                for (int i = 0; i < melodyLength; i++)
                {
                    melody[i] = availableKeys[UnityEngine.Random.Range(0, availableKeys.Length)];
                }
                break;
        }
        
        return melody;
    }
    
    private IEnumerator ExecuteRitual()
    {
        inRitual = true;
        
        dialogueManager.ShowPrompt($"Performing {chosenRitualType} ritual...");
        yield return new WaitForSeconds(1.5f);
        
        yield return StartCoroutine(DisplayMelody());
        
        yield return StartCoroutine(WaitForPlayerInput());
        
        inRitual = false;
    }
    
    private IEnumerator DisplayMelody()
    {
        // DEBUG: Print full ritual melody
    Debug.Log("----- Ritual Melody Start -----");
    for (int i = 0; i < currentMelody.Length; i++)
    {
    Debug.Log($"Note {i+1}: {currentMelody[i]}");
    }
    Debug.Log("----- Ritual Melody End -----");

        ritualPromptText.text = "Listen carefully...";
        yield return new WaitForSeconds(1f);
        
        string melodyDisplay = "Melody: ";
        
        foreach (var note in currentMelody)
        {
            melodyDisplay += $"[{note}] ";
            ritualPromptText.text = melodyDisplay;
            
            int noteIndex = System.Array.IndexOf(NoteInputManager.Instance.noteKeys, note);
            if (noteIndex >= 0 && NoteInputManager.Instance.audioSource != null && 
                NoteInputManager.Instance.noteClips != null && noteIndex < NoteInputManager.Instance.noteClips.Length)
            {
                NoteInputManager.Instance.audioSource.PlayOneShot(NoteInputManager.Instance.noteClips[noteIndex]);
            }
            
            yield return new WaitForSeconds(showNoteDelay);
        }
        
        yield return new WaitForSeconds(0.5f);
        ritualPromptText.text = "Now play it back!";
    }
    
    private IEnumerator WaitForPlayerInput()
    {
        float timeout = currentMelody.Length * timeoutMultiplier;
        float elapsed = 0f;
        bool completed = false;
        
        NoteInputManager noteInput = NoteInputManager.Instance;
        noteInput.ClearBuffer();
        
        while (elapsed < timeout && !completed)
        {
            if (noteInput.CheckSequence(currentMelody))
            {
                completed = true;
                ritualPerformanceSucceeded = true;
                yield return StartCoroutine(HandleRitualComplete());
                yield break;
            }
            
            elapsed += Time.deltaTime;
            float timeRemaining = timeout - elapsed;
            ritualPromptText.text = $"Time remaining: {timeRemaining:F1}s";
            
            yield return null;
        }
        
        // Timeout - performance failed
        yield return StartCoroutine(HandleRitualComplete());
    }
    
    private IEnumerator HandleRitualComplete()
    {
        // Determine outcome
        if (ritualPerformanceSucceeded)
        {
            if (wasCorrectRitual)
            {
                // Perfect success
                yield return StartCoroutine(HandlePerfectSuccess());
            }
            else
            {
                // Wrong ritual performed well
                yield return StartCoroutine(HandleWrongRitualSuccess());
            }
        }
        else
        {
            if (wasCorrectRitual)
            {
                // Right ritual, wrong performance
                yield return StartCoroutine(HandleCorrectRitualFailed());
            }
            else
            {
                // Everything wrong
                yield return StartCoroutine(HandleTotalFailure());
            }
        }
        
        OnRitualComplete?.Invoke(currentSpirit, ritualPerformanceSucceeded, wasCorrectRitual);
    }
    
    private IEnumerator HandlePerfectSuccess()
    {
        ritualPromptText.text = "SUCCESS!";
        
        if (successClip != null && ritualAudioSource != null)
            ritualAudioSource.PlayOneShot(successClip);
        
        dialogueManager.ShowResponse(GetSpiritSuccessMessage());
        
        yield return new WaitForSeconds(2f);
        
        // Apply rewards
        if (SpiritualEnergyManager.Instance != null)
        {
            SpiritualEnergyManager.Instance.AddEnergy(25); // Bonus for correct deduction
        }
        
        // Clear the prompt text before transitioning
        if (ritualPromptText != null)
            ritualPromptText.text = "";
    }
    
    private IEnumerator HandleWrongRitualSuccess()
    {
        ritualPromptText.text = "WRONG RITUAL";
        
        if (failureClip != null && ritualAudioSource != null)
            ritualAudioSource.PlayOneShot(failureClip);
        
        // Show hint
        string hint = RitualHintSystem.GetWrongRitualMessage(chosenRitualType, currentSpirit.personality);
        dialogueManager.ShowResponse(hint);
        
        yield return new WaitForSeconds(2f);
        
        // Apply penalties
        if (SpiritualEnergyManager.Instance != null)
        {
            SpiritualEnergyManager.Instance.RemoveEnergy(25); // Wrong deduction penalty
        }
        
        if (InstrumentManager.Instance != null)
        {
            InstrumentManager.Instance.DamageInstrument(15); // Instrument damage
        }
        
        // Clear the prompt text before transitioning
        if (ritualPromptText != null)
            ritualPromptText.text = "";
    }
    
    private IEnumerator HandleCorrectRitualFailed()
    {
        ritualPromptText.text = "PERFORMANCE FAILED";
        
        if (failureClip != null && ritualAudioSource != null)
            ritualAudioSource.PlayOneShot(failureClip);
        
        string failureMsg = GetSpiritFailureMessage();
        dialogueManager.ShowResponse(failureMsg);
        
        yield return new WaitForSeconds(2f);
        
        // Apply penalties
        if (SpiritualEnergyManager.Instance != null)
        {
            SpiritualEnergyManager.Instance.RemoveEnergy(15); // Performance failure penalty
        }
        
        if (InstrumentManager.Instance != null)
        {
            InstrumentManager.Instance.DamageInstrument(15); // Instrument damage
        }
        
        // Clear the prompt text before transitioning
        if (ritualPromptText != null)
            ritualPromptText.text = "";
    }
    
    private IEnumerator HandleTotalFailure()
    {
        ritualPromptText.text = "CATASTROPHIC FAILURE";
        
        if (failureClip != null && ritualAudioSource != null)
            ritualAudioSource.PlayOneShot(failureClip);
        
        string failureMsg = "The ritual crumbles. The spirit thrashes in anguish!";
        dialogueManager.ShowResponse(failureMsg);
        
        yield return new WaitForSeconds(2f);
        
        // Apply massive penalties
        if (SpiritualEnergyManager.Instance != null)
        {
            SpiritualEnergyManager.Instance.RemoveEnergy(35); // Both penalties combined
        }
        
        if (InstrumentManager.Instance != null)
        {
            InstrumentManager.Instance.DamageInstrument(20); // Heavy instrument damage
        }
        
        // Clear the prompt text before transitioning
        if (ritualPromptText != null)
            ritualPromptText.text = "";
    }
    
    private string GetSpiritSuccessMessage()
    {
        if (currentSpirit.ritualSuccessDialogue != null && currentSpirit.ritualSuccessDialogue.Length > 0)
        {
            return currentSpirit.ritualSuccessDialogue[UnityEngine.Random.Range(0, currentSpirit.ritualSuccessDialogue.Length)];
        }
        
        return "At last... I am free...";
    }
    
    private string GetSpiritFailureMessage()
    {
        if (currentSpirit.ritualFailureDialogue != null && currentSpirit.ritualFailureDialogue.Length > 0)
        {
            return currentSpirit.ritualFailureDialogue[UnityEngine.Random.Range(0, currentSpirit.ritualFailureDialogue.Length)];
        }
        
        return currentSpirit.personality switch
        {
            Personality.Mournful => "Your attempt only deepens the sorrow...",
            Personality.Hostile => "You fail! The spirit's fury intensifies!",
            Personality.Meek => "I... I cannot... try again...",
            Personality.Trickster => "Amusing, but unsuccessful.",
            _ => "The ritual falters, and I remain bound..."
        };
    }
}