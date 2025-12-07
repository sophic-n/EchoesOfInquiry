using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NoteInputManager : MonoBehaviour
{
    public static NoteInputManager Instance { get; private set; }
    
    [Header("Input Configuration")]
    [Tooltip("8 keys for musical inquiry - default: Z X C V B N M K (bottom row of keyboard)")]
    public KeyCode[] noteKeys = new KeyCode[] { 
        KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V, 
        KeyCode.B, KeyCode.N, KeyCode.M, KeyCode.K 
    };
    public float maxTimeBetweenNotes = 0.8f;
    public float idleResetTime = 2f;
    
    [Header("Visual Feedback")]
    [Tooltip("Show key press feedback in UI")]
    public bool showVisualFeedback = true;
    public TextMeshProUGUI inputFeedbackText;

    public float feedbackDisplayTime = 0.3f;
    
    [Header("Audio")]
    public AudioSource audioSource;
    [Tooltip("Leave empty - question melodies play from QuestionEntry instead")]
    public AudioClip[] noteClips;
    
    private List<NoteEvent> recordedNotes = new List<NoteEvent>();
    private float lastInputTime;
    private NoteSequence activeSequence;
    private int currentMatchIndex;
    private Coroutine feedbackCoroutine;
    
    public event Action<KeyCode> OnNotePlayed;
    public event Action<bool> OnSequenceComplete;
    public event Action<NoteSequence> OnSequenceMatched;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    void Update()
    {
        CheckForInput();
        CheckIdleReset();
        
        if (activeSequence != null)
        {
            CheckActiveSequence();
        }
    }
    
    private void CheckForInput()
    {
        for (int i = 0; i < noteKeys.Length; i++)
        {
            if (Input.GetKeyDown(noteKeys[i]))
            {
                RecordNote(noteKeys[i], i);
            }
        }
    }
    
    private void RecordNote(KeyCode key, int noteIndex)
    {
        recordedNotes.Add(new NoteEvent 
        { 
            key = key, 
            time = Time.time,
            noteIndex = noteIndex
        });
        
        lastInputTime = Time.time;
        
        // Visual and debug feedback
        ShowInputFeedback(key);
        Debug.Log($"Note pressed: {key} (Index: {noteIndex}) - Buffer: {recordedNotes.Count} notes");
        
        // Don't play individual note sounds
        // PlayNoteAudio(noteIndex);
        
        OnNotePlayed?.Invoke(key);
    }
    
    private void ShowInputFeedback(KeyCode key)
    {
        if (!showVisualFeedback || inputFeedbackText == null) return;
        
        // Stop previous feedback coroutine
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }
        
        // Show current sequence
        string sequence = "";
        foreach (var note in recordedNotes)
        {
            sequence += note.key.ToString() + " ";
        }
        
        inputFeedbackText.text = $"Playing: {sequence}";
        inputFeedbackText.color = Color.cyan;
        
        // Start fade coroutine
        feedbackCoroutine = StartCoroutine(FadeOutFeedback());
    }
    
    private System.Collections.IEnumerator FadeOutFeedback()
    {
        yield return new WaitForSeconds(feedbackDisplayTime);
        
        if (inputFeedbackText != null)
        {
            float elapsed = 0f;
            float fadeTime = 0.5f;
            Color startColor = inputFeedbackText.color;
            
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                inputFeedbackText.color = Color.Lerp(startColor, new Color(startColor.r, startColor.g, startColor.b, 0f), elapsed / fadeTime);
                yield return null;
            }
            
            inputFeedbackText.text = "";
            inputFeedbackText.color = startColor; // Reset
        }
    }
    
    private void PlayNoteAudio(int noteIndex)
    {
        // Disabled - questions play their own melodies instead
        // if (audioSource != null && noteClips != null && noteIndex < noteClips.Length)
        // {
        //     audioSource.PlayOneShot(noteClips[noteIndex]);
        // }
    }
    
    private void CheckIdleReset()
    {
        if (recordedNotes.Count > 0 && Time.time - lastInputTime > idleResetTime)
        {
            recordedNotes.Clear();
        }
    }
    
    private void CheckActiveSequence()
    {
        if (recordedNotes.Count == 0) return;
        
        var lastNote = recordedNotes[recordedNotes.Count - 1];
        
        if (currentMatchIndex == 0 || Time.time - lastInputTime <= activeSequence.maxTimeBetweenNotes)
        {
            if (lastNote.key == activeSequence.notes[currentMatchIndex])
            {
                currentMatchIndex++;
                
                if (currentMatchIndex >= activeSequence.notes.Length)
                {
                    FinishSequence(true);
                }
            }
            else
            {
                FinishSequence(false);
            }
        }
        else
        {
            FinishSequence(false);
        }
    }
    
    public void StartMatching(NoteSequence sequence)
    {
        activeSequence = sequence;
        currentMatchIndex = 0;
        recordedNotes.Clear();
        lastInputTime = Time.time;
    }
    
    private void FinishSequence(bool success)
    {
        if (success)
        {
            OnSequenceMatched?.Invoke(activeSequence);
        }
        
        OnSequenceComplete?.Invoke(success);
        activeSequence = null;
        currentMatchIndex = 0;
        recordedNotes.Clear();
    }
    
    public bool CheckSequence(KeyCode[] sequence)
    {
        if (sequence == null || sequence.Length == 0) return false;
        if (recordedNotes.Count < sequence.Length) return false;
        
        int startIndex = recordedNotes.Count - sequence.Length;
        
        for (int i = 0; i < sequence.Length; i++)
        {
            if (recordedNotes[startIndex + i].key != sequence[i])
                return false;
            
            if (i > 0)
            {
                float timeDiff = recordedNotes[startIndex + i].time - recordedNotes[startIndex + i - 1].time;
                if (timeDiff > maxTimeBetweenNotes)
                    return false;
            }
        }
        
        recordedNotes.Clear();
        return true;
    }
    
    public int[] ConsumeBuffer()
    {
        List<int> indices = new List<int>();
        foreach (var note in recordedNotes)
        {
            indices.Add(note.noteIndex);
        }
        recordedNotes.Clear();
        return indices.ToArray();
    }
    
    public void ClearBuffer()
    {
        recordedNotes.Clear();
    }

    [Serializable]
    private class NoteEvent
    {
        public KeyCode key;
        public float time;
        public int noteIndex;
    }
    void Start()
{
    if (inputFeedbackText != null)
    {
        inputFeedbackText.text = "";
    }

    // Log available keys for debugging
    Debug.Log($"NoteInputManager initialized with {noteKeys.Length} keys: {string.Join(", ", noteKeys)}");
}
}