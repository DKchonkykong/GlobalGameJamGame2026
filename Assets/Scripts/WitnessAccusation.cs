using UnityEngine;
using UnityEngine.SceneManagement;

public class WitnessAccusation : MonoBehaviour, IInteractable
{
    [Header("Witness Identity")]
    public string witnessName = "Johanne"; // Johanne, Tony, or Joe
    
    [Header("Ending Scene")]
    public string endingSceneName = "EndingJohanne"; // Scene to load when accused
    
    [Header("Dialogue")]
    public DialogueNode normalDialogueNode; // Normal conversation (optional fallback)
    public DialogueNode accusationDialogueNode; // Dialogue to show BEFORE loading ending scene
    
    [Header("References")]
    public DialogueActor dialogueActor; // Reference to existing DialogueActor

    private bool isAccusationPhase = false;

    public string PromptText
    {
        get
        {
            // Check accusation phase
            if (InvestigationManager.Instance != null)
            {
                isAccusationPhase = InvestigationManager.Instance.IsAccusationPhaseActive();
            }

            if (isAccusationPhase)
            {
                return $"Press E to accuse {witnessName}";
            }
            return $"Press E to talk to {witnessName}";
        }
    }

    void Start()
    {
        // Auto-find DialogueActor if not assigned
        if (dialogueActor == null)
        {
            dialogueActor = GetComponent<DialogueActor>();
        }

        // CRITICAL: Remove DialogueActor from being detected as IInteractable
        // We do this by destroying the component and storing its data
        if (dialogueActor != null)
        {
            Debug.Log($"[WitnessAccusation] Found DialogueActor on {witnessName}");
        }
    }

    public void Interact()
    {
        Debug.Log($"[WitnessAccusation] Interact() called on {witnessName}");
        
        // Re-check accusation phase (in case it changed)
        if (InvestigationManager.Instance != null)
        {
            isAccusationPhase = InvestigationManager.Instance.IsAccusationPhaseActive();
        }
        
        // Check if we're in accusation phase
        if (isAccusationPhase)
        {
            Debug.Log($"[WitnessAccusation] Accusation phase is ACTIVE - triggering accusation");
            AccuseWitness();
        }
        else
        {
            Debug.Log($"[WitnessAccusation] Accusation phase is NOT active - normal interaction");
            NormalInteraction();
        }
    }

    void NormalInteraction()
    {
        // If they have a DialogueActor, manually call its Interact method
        if (dialogueActor != null)
        {
            Debug.Log($"[WitnessAccusation] Delegating to DialogueActor for normal interaction");
            
            // Set conversation context
            if (ConversationContext.Instance != null)
            {
                ConversationContext.Instance.SetActiveReceiver(dialogueActor);
            }
            
            // Start conversation through DialogueManager
            DialogueManager.Instance.StartConversation(dialogueActor);
        }
        // Otherwise use our normal dialogue node
        else if (normalDialogueNode != null && normalDialogueNode.lines != null && normalDialogueNode.lines.Length > 0)
        {
            Debug.Log($"[WitnessAccusation] Using normal dialogue node");
            DialogueManager.Instance.ShowDialogue(normalDialogueNode.lines);
        }
        else
        {
            Debug.LogWarning($"[WitnessAccusation] No dialogue available for {witnessName}");
        }
    }

    void AccuseWitness()
    {
        Debug.Log($"[WitnessAccusation] ===== ACCUSING {witnessName}! =====");

        // Show accusation dialogue first
        if (accusationDialogueNode != null && accusationDialogueNode.lines != null && accusationDialogueNode.lines.Length > 0)
        {
            Debug.Log($"[WitnessAccusation] Showing accusation dialogue with {accusationDialogueNode.lines.Length} lines");
            DialogueManager.Instance.ShowDialogue(accusationDialogueNode.lines);
            
            // Load scene after dialogue finishes (estimate 3 seconds per line)
            float dialogueLength = accusationDialogueNode.lines.Length * 3f;
            Invoke(nameof(LoadEndingScene), dialogueLength);
        }
        else
        {
            Debug.LogWarning($"[WitnessAccusation] No accusation dialogue set for {witnessName} - loading ending immediately");
            LoadEndingScene();
        }
    }

    void LoadEndingScene()
    {
        if (!string.IsNullOrEmpty(endingSceneName))
        {
            Debug.Log($"[WitnessAccusation] ===== LOADING ENDING SCENE: {endingSceneName} =====");
            SceneManager.LoadScene(endingSceneName);
        }
        else
        {
            Debug.LogError($"[WitnessAccusation] Ending scene name is not set for {witnessName}!");
        }
    }

    // Debug method
    [ContextMenu("Test Accusation")]
    void TestAccusation()
    {
        AccuseWitness();
    }
    
    [ContextMenu("Check Accusation Phase Status")]
    void CheckStatus()
    {
        if (InvestigationManager.Instance != null)
        {
            bool isActive = InvestigationManager.Instance.IsAccusationPhaseActive();
            Debug.Log($"[WitnessAccusation] Accusation Phase Active: {isActive}");
        }
        else
        {
            Debug.LogError("[WitnessAccusation] InvestigationManager.Instance is NULL!");
        }
    }
}