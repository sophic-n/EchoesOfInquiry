using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Controls environmental effects based on spirit tags and encounter state
/// Handles lights, particles, audio, and atmosphere
/// </summary>
public class EnvironmentController : MonoBehaviour
{
    public static EnvironmentController Instance { get; private set; }
    
    [Header("Lighting")]
    [SerializeField] private Light mainLight;
    [SerializeField] private Light[] accentLights;
    [SerializeField] private float lightTransitionSpeed = 1f;
    
    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem fireParticles;
    [SerializeField] private ParticleSystem waterParticles;
    [SerializeField] private ParticleSystem willowParticles;
    [SerializeField] private ParticleSystem swordParticles;
    [SerializeField] private ParticleSystem toyParticles;
    [SerializeField] private ParticleSystem genericSpiritParticles;
    
    [Header("Audio")]
    [SerializeField] private AudioSource ambientAudioSource;
    [SerializeField] private AudioSource spiritWhisperSource;
    [SerializeField] private AudioClip[] whisperClips;
    [SerializeField] private float whisperInterval = 5f;
    
    [Header("Environment Presets")]
    [SerializeField] private EnvironmentPreset[] environmentPresets;
    
    [Header("Post Processing")]
    [SerializeField] private GameObject postProcessingVolume;
    
    private EnvironmentalTag currentEnvironmentTag;
    private Coroutine whisperCoroutine;
    private Dictionary<EnvironmentalTag, ParticleSystem> particleMap;
    
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
        InitializeParticleMap();
        SetDefaultEnvironment();
    }
    
    private void InitializeParticleMap()
    {
        particleMap = new Dictionary<EnvironmentalTag, ParticleSystem>
        {
            { EnvironmentalTag.Fire, fireParticles },
            { EnvironmentalTag.Water, waterParticles },
            { EnvironmentalTag.Willows, willowParticles },
            { EnvironmentalTag.Swords, swordParticles },
            { EnvironmentalTag.Toys, toyParticles }
        };
    }
    
    public void SetupEnvironmentForSpirit(SpiritData spirit)
    {
        if (spirit == null || spirit.environmentTags == null || spirit.environmentTags.Length == 0)
        {
            SetDefaultEnvironment();
            return;
        }
        
        // Use first environment tag as primary
        currentEnvironmentTag = spirit.environmentTags[0];
        
        ApplyEnvironmentPreset(currentEnvironmentTag);
        ActivateParticlesForTags(spirit.environmentTags);
        
        // Start whispers
        if (whisperCoroutine != null) StopCoroutine(whisperCoroutine);
        whisperCoroutine = StartCoroutine(PlayWhispersRoutine());
    }
    
    private void ApplyEnvironmentPreset(EnvironmentalTag tag)
    {
        EnvironmentPreset preset = GetPresetForTag(tag);
        
        if (preset == null)
        {
            Debug.LogWarning($"No preset found for {tag}");
            return;
        }
        
        // Apply lighting
        if (mainLight != null)
        {
            StartCoroutine(TransitionLightColor(mainLight, preset.mainLightColor, lightTransitionSpeed));
            StartCoroutine(TransitionLightIntensity(mainLight, preset.mainLightIntensity, lightTransitionSpeed));
        }
        
        // Apply accent lights
        if (accentLights != null)
        {
            foreach (var light in accentLights)
            {
                if (light != null)
                {
                    StartCoroutine(TransitionLightColor(light, preset.accentLightColor, lightTransitionSpeed));
                }
            }
        }
        
        // Apply fog
        RenderSettings.fogColor = preset.fogColor;
        RenderSettings.fogDensity = preset.fogDensity;
        
        // Apply ambient audio
        if (ambientAudioSource != null && preset.ambientSound != null)
        {
            ambientAudioSource.clip = preset.ambientSound;
            ambientAudioSource.loop = true;
            ambientAudioSource.Play();
        }
    }
    
    private void ActivateParticlesForTags(EnvironmentalTag[] tags)
{
    // Safety: ensure particleMap is initialized
    if (particleMap == null)
    {
        Debug.LogWarning("Particle map is not initialized. Using generic particles only.");
        genericSpiritParticles?.Play();
        return;
    }

    // Stop all currently active particles
    foreach (var particles in particleMap.Values)
    {
        if (particles != null)
            particles.Stop();
    }

    // Safety: check for null or empty tag array
    if (tags == null || tags.Length == 0)
    {
        Debug.LogWarning("No environment tags provided. Using generic particles.");
        genericSpiritParticles?.Play();
        return;
    }

    // Track if any tag successfully matched
    bool anyPlayed = false;

    // Enable particles for the provided tags
    foreach (var tag in tags)
    {
        if (particleMap.TryGetValue(tag, out var particles))
        {
            if (particles != null)
            {
                particles.Play();
                anyPlayed = true;
            }
            else
            {
                Debug.LogWarning($"Particle system for tag {tag} is null. Using generic particles.");
            }
        }
        else
        {
            Debug.LogWarning($"No particle mapping found for tag: {tag}. Using generic particles.");
        }
    }

    // Always show generic spirit particles if nothing else played or by design
    if (!anyPlayed && genericSpiritParticles != null)
    {
        genericSpiritParticles.Play();
    }
    else if (genericSpiritParticles != null)
    {
        // Optional: you can keep this line if you always want the generic ones active
        genericSpiritParticles.Play();
    }
}

    
    private EnvironmentPreset GetPresetForTag(EnvironmentalTag tag)
    {
        foreach (var preset in environmentPresets)
        {
            if (preset.tag == tag)
            {
                return preset;
            }
        }
        return null;
    }
    
    private IEnumerator TransitionLightColor(Light light, Color targetColor, float duration)
    {
        Color startColor = light.color;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            light.color = Color.Lerp(startColor, targetColor, elapsed / duration);
            yield return null;
        }
        
        light.color = targetColor;
    }
    
    private IEnumerator TransitionLightIntensity(Light light, float targetIntensity, float duration)
    {
        float startIntensity = light.intensity;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            light.intensity = Mathf.Lerp(startIntensity, targetIntensity, elapsed / duration);
            yield return null;
        }
        
        light.intensity = targetIntensity;
    }
    
    private IEnumerator PlayWhispersRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(whisperInterval + Random.Range(-2f, 2f));
            
            if (spiritWhisperSource != null && whisperClips != null && whisperClips.Length > 0)
            {
                AudioClip clip = whisperClips[Random.Range(0, whisperClips.Length)];
                spiritWhisperSource.PlayOneShot(clip);
            }
        }
    }
    
    public void PlaySpiritWhisper()
    {
        if (spiritWhisperSource != null && whisperClips != null && whisperClips.Length > 0)
        {
            AudioClip clip = whisperClips[Random.Range(0, whisperClips.Length)];
            spiritWhisperSource.PlayOneShot(clip);
        }
    }
    
    public void SetDefaultEnvironment()
    {
        // Neutral, calm environment
        if (mainLight != null)
        {
            mainLight.color = new Color(0.8f, 0.8f, 0.9f);
            mainLight.intensity = 0.5f;
        }
        
        RenderSettings.fogColor = new Color(0.5f, 0.5f, 0.6f);
        RenderSettings.fogDensity = 0.01f;
        
        // Stop all particles
        foreach (var particles in particleMap.Values)
        {
            if (particles != null)
            {
                particles.Stop();
            }
        }
        
        if (whisperCoroutine != null)
        {
            StopCoroutine(whisperCoroutine);
        }
    }
    
    public void ClearEnvironment()
    {
        SetDefaultEnvironment();
        
        if (ambientAudioSource != null)
        {
            ambientAudioSource.Stop();
        }
    }
}

[System.Serializable]
public class EnvironmentPreset
{
    public EnvironmentalTag tag;
    public Color mainLightColor = Color.white;
    public float mainLightIntensity = 1f;
    public Color accentLightColor = Color.white;
    public Color fogColor = Color.gray;
    public float fogDensity = 0.01f;
    public AudioClip ambientSound;
}