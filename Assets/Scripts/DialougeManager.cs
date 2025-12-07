using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    
    [Header("UI References")]
    public TMP_Text riddleText;
    public TMP_Text responseText;
    public TMP_Text promptText;
    public CanvasGroup canvasGroup;
    
    [Header("Type Effect")]
    public float typeDelay = 0.02f;
    public bool useTypeEffect = true;
    
    [Header("Auto-Fade Settings")]
    public bool autoFadeDialogue = true;
    public float dialogueFadeDelay = 30f; // Riddles and responses fade after 30 seconds
    public float fadeOutDuration = 2f;
    
    private Coroutine currentTypeCoroutine;
    private Coroutine riddleFadeCoroutine;
    private Coroutine responseFadeCoroutine;
    
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
        // Don't auto-hide on start - wait for first riddle
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }
    
    public void ShowRiddle(string riddle)
    {
        Show();
        
        // Cancel any existing fade
        if (riddleFadeCoroutine != null)
        {
            StopCoroutine(riddleFadeCoroutine);
        }
        
        if (useTypeEffect)
        {
            if (currentTypeCoroutine != null) StopCoroutine(currentTypeCoroutine);
            currentTypeCoroutine = StartCoroutine(TypeText(riddleText, riddle));
        }
        else
        {
            riddleText.text = riddle;
        }
        
        // Start auto-fade timer
        if (autoFadeDialogue)
        {
            riddleFadeCoroutine = StartCoroutine(FadeTextAfterDelay(riddleText, dialogueFadeDelay));
        }
    }
    
    public void ShowResponse(string response)
    {
        Show();
        
        // Cancel any existing fade
        if (responseFadeCoroutine != null)
        {
            StopCoroutine(responseFadeCoroutine);
        }
        
        if (useTypeEffect)
        {
            if (currentTypeCoroutine != null) StopCoroutine(currentTypeCoroutine);
            currentTypeCoroutine = StartCoroutine(TypeText(responseText, response));
        }
        else
        {
            responseText.text = response;
        }
        
        // Start auto-fade timer
        if (autoFadeDialogue)
        {
            responseFadeCoroutine = StartCoroutine(FadeTextAfterDelay(responseText, dialogueFadeDelay));
        }
    }
    
    public void ShowPrompt(string prompt)
    {
        Show();
        if (promptText != null)
        {
            promptText.text = prompt;
            // Prompts don't auto-fade
        }
    }
    
    private IEnumerator FadeTextAfterDelay(TMP_Text targetText, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Fade out the text
        Color originalColor = targetText.color;
        float elapsed = 0f;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            targetText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
        
        // Clear the text after fade
        targetText.text = "";
        targetText.color = originalColor; // Reset color for next use
    }
    
    public void ShowTextImmediate(string text, TMP_Text target = null)
    {
        Show();
        if (target == null) target = responseText;
        target.text = text;
    }
    
    public IEnumerator TypeText(TMP_Text target, string text)
    {
        if (target == null) yield break;
        
        // Reset alpha before typing
        Color originalColor = target.color;
        target.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        
        target.text = "";
        foreach (char c in text)
        {
            target.text += c;
            yield return new WaitForSeconds(typeDelay);
        }
    }
    
    public void Show()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }
    
    public void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        
        // Stop all fade coroutines
        if (riddleFadeCoroutine != null) StopCoroutine(riddleFadeCoroutine);
        if (responseFadeCoroutine != null) StopCoroutine(responseFadeCoroutine);
        
        if (riddleText != null) riddleText.text = "";
        if (responseText != null) responseText.text = "";
        if (promptText != null) promptText.text = "";
    }
    
    public void ClearAll()
    {
        // Stop all fades
        if (riddleFadeCoroutine != null) StopCoroutine(riddleFadeCoroutine);
        if (responseFadeCoroutine != null) StopCoroutine(responseFadeCoroutine);
        
        if (riddleText != null)
        {
            riddleText.text = "";
            riddleText.color = new Color(riddleText.color.r, riddleText.color.g, riddleText.color.b, 1f);
        }
        if (responseText != null)
        {
            responseText.text = "";
            responseText.color = new Color(responseText.color.r, responseText.color.g, responseText.color.b, 1f);
        }
        if (promptText != null) promptText.text = "";
    }
}