
using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Manages individual ritual altars in the environment
/// Players interact with these to choose rituals during deduction phase
/// </summary>
public class RitualAltar : MonoBehaviour
{
    [Header("Ritual Configuration")]
    public RitualType ritualType;
    public string ritualDisplayName;
    [TextArea(2, 3)]
    public string ritualDescription;
    
    [Header("Interaction")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    
    [Header("UI References")]
    [SerializeField] private Canvas worldSpaceCanvas;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text promptText;
    
    [Header("Visual Feedback")]
    [SerializeField] private Light altarLight;
    [SerializeField] private ParticleSystem altarParticles;
    [SerializeField] private Renderer altarRenderer;
    
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color activeColor = Color.cyan;
    [SerializeField] private Color selectedColor = Color.green;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip selectSound;
    
    private Transform player;
    private bool isInRange = false;
    private bool isSelected = false;
    private EnvironmentalRitualManager ritualManager;
    
    void Start()
    {
        ritualManager = FindObjectOfType<EnvironmentalRitualManager>();
        
        if (worldSpaceCanvas != null)
            worldSpaceCanvas.enabled = false;
            
        if (nameText != null)
            nameText.text = ritualDisplayName;
            
        // Add sphere collider for trigger detection
        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider == null)
        {
            sphereCollider = gameObject.AddComponent<SphereCollider>();
        }
        sphereCollider.radius = interactionRange;
        sphereCollider.isTrigger = true;
        
        SetInactive();
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (ritualManager == null || !ritualManager.IsDeductionPhaseActive)
            return;
            
        if (other.CompareTag("Player") || other.transform.root.CompareTag("Player"))
        {
            player = other.transform.root;
            OnPlayerEnterRange();
            Debug.Log($"TRIGGER: Player entered range of {ritualDisplayName}");
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.transform.root.CompareTag("Player"))
        {
            OnPlayerExitRange();
            Debug.Log($"TRIGGER: Player exited range of {ritualDisplayName}");
        }
    }
    
    void Update()
    {
        if (ritualManager == null || !ritualManager.IsDeductionPhaseActive)
        {
            if (isInRange)
                OnPlayerExitRange();
            return;
        }
            
        if (player == null)
        {
            // Try multiple ways to find player
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else if (Camera.main != null)
            {
                // Try camera parent (common FPS setup)
                player = Camera.main.transform.parent;
                if (player == null)
                    player = Camera.main.transform;
            }
        }
            
        if (player == null)
        {
            Debug.LogWarning($"{ritualDisplayName}: Cannot find player!");
            return;
        }
        
        float distance = Vector3.Distance(transform.position, player.position);
        
        // Debug visualization with actual distance shown
        Debug.DrawLine(transform.position, player.position, 
            distance <= interactionRange ? Color.green : Color.red);
        
        if (distance <= interactionRange)
        {
            if (!isInRange)
            {
                OnPlayerEnterRange();
                Debug.Log($"<color=green>Player entered range of {ritualDisplayName} - Distance: {distance:F2}</color>");
            }
            
            // Check for E key press every frame when in range
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log($"<color=cyan>===== E KEY PRESSED =====</color>");
                Debug.Log($"<color=cyan>Altar: {ritualDisplayName}</color>");
                Debug.Log($"<color=cyan>RitualManager null? {ritualManager == null}</color>");
                Debug.Log($"<color=cyan>IsDeductionPhaseActive? {ritualManager?.IsDeductionPhaseActive}</color>");
                
                if (ritualManager != null)
                {
                    ritualManager.SelectAltar(this);
                }
                else
                {
                    Debug.LogError("RitualManager is null!");
                }
            }
            
            // Also check the configured interaction key in case it's not E
            if (interactionKey != KeyCode.E && Input.GetKeyDown(interactionKey))
            {
                Debug.Log($"<color=cyan>Interaction key {interactionKey} pressed near {ritualDisplayName}</color>");
                if (ritualManager != null)
                {
                    ritualManager.SelectAltar(this);
                }
            }
        }
        else if (isInRange)
        {
            OnPlayerExitRange();
        }
    }
    
    private void OnPlayerEnterRange()
    {
        isInRange = true;
        
        Debug.Log($"<color=yellow>OnPlayerEnterRange called for {ritualDisplayName}</color>");
        Debug.Log($"Canvas null? {worldSpaceCanvas == null}, PromptText null? {promptText == null}");
        
        if (worldSpaceCanvas != null)
        {
            worldSpaceCanvas.enabled = true;
            Debug.Log($"Canvas enabled for {ritualDisplayName}");
        }
            
        if (promptText != null)
        {
            promptText.text = $"[{interactionKey}] Perform {ritualDisplayName}";
            promptText.enabled = true;
            Debug.Log($"Prompt text set: {promptText.text}");
        }
        
        if (!isSelected)
            SetActive();
            
        PlaySound(hoverSound);
    }
    
    private void OnPlayerExitRange()
    {
        isInRange = false;
        
        if (worldSpaceCanvas != null)
            worldSpaceCanvas.enabled = false;
        
        if (!isSelected)
            SetInactive();
    }
    
    public void SelectAltar()
    {
        if (isSelected)
            return;
            
        isSelected = true;
        SetSelected();
        PlaySound(selectSound);
    }
    
    public void DeselectAltar()
    {
        isSelected = false;
        
        if (isInRange)
            SetActive();
        else
            SetInactive();
    }
    
    private void SetInactive()
    {
        if (altarLight != null)
            altarLight.enabled = false;
            
        if (altarRenderer != null)
            altarRenderer.material.color = inactiveColor;
            
        if (altarParticles != null && altarParticles.isPlaying)
            altarParticles.Stop();
    }
    
    private void SetActive()
    {
        if (altarLight != null)
        {
            altarLight.enabled = true;
            altarLight.color = activeColor;
        }
        
        if (altarRenderer != null)
            altarRenderer.material.color = activeColor;
            
        if (altarParticles != null && !altarParticles.isPlaying)
            altarParticles.Play();
    }
    
    private void SetSelected()
    {
        if (altarLight != null)
        {
            altarLight.enabled = true;
            altarLight.color = selectedColor;
        }
        
        if (altarRenderer != null)
            altarRenderer.material.color = selectedColor;
            
        if (altarParticles != null && !altarParticles.isPlaying)
            altarParticles.Play();
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
    
    // DEBUG: Visualize interaction range
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        // Draw label
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2, 
            $"{ritualDisplayName}\nRange: {interactionRange}");
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}