// Verification Script - Delete after checking
using UnityEngine;

public class VersionCheck : MonoBehaviour
{
    void Start()
    {
        // Check 1: NoteInputManager uses new keys
        var noteInput = FindObjectOfType<NoteInputManager>();
        if (noteInput != null && noteInput.noteKeys.Length == 8)
        {
            Debug.Log("✅ NoteInputManager: Latest (8 keys)");
        }
        else
        {
            Debug.LogError("❌ NoteInputManager: OLD VERSION");
        }
        
        // Check 2: SpiritEncounterManager has UnlockProgress
        var encounter = FindObjectOfType<SpiritEncounterManager>();
        if (encounter != null && encounter.globalQuestionLibrary != null)
        {
            Debug.Log("✅ SpiritEncounterManager: Latest (global library)");
        }
        else
        {
            Debug.LogError("❌ SpiritEncounterManager: OLD VERSION");
        }
        
        // Check 3: FallbackDialogueLibrary exists
        string test = FallbackDialogueLibrary.GetPersonalityFallback(Personality.Mournful);
        if (!string.IsNullOrEmpty(test))
        {
            Debug.Log("✅ FallbackDialogueLibrary: Latest");
        }
        else
        {
            Debug.LogError("❌ FallbackDialogueLibrary: MISSING");
        }
        
        Debug.Log("=== VERSION CHECK COMPLETE ===");
    }
}
