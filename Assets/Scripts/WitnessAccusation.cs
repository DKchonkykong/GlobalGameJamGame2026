using UnityEngine;
using UnityEngine.SceneManagement;

public class WitnessAccusation : MonoBehaviour, IInteractable
{
    [Header("Witness Identity")]
    public string witnessName = "Johanne"; // Johanne, Tony, or Joe
    
    [Header("Ending Scene")]
    public string endingSceneName = "EndingJohanne"; // Scene to load when accused
    
    [Header("Dialogue")]
    public DialogueNode normalDialogueNode; // Normal conversation
    public DialogueNode accusationDialogueNode; // Optional: dialogue before loading ending
    
    [Header("References")]
    public DialogueActor dialogueActor; // Reference to existing DialogueActor if they have one

    public string PromptText
    {
        get
        {
            if (InvestigationManager.Instance != null && InvestigationManager.Instance.IsAccusationPhaseActive())
            {
                return $"Press E to accuse {witnessName}";
            }
            return $"Press E to talk to {witnessName}";
        }
    }

    void Awake()
    {
        // Auto-find DialogueActor if not assigned
        if (dialogueActor == null)
        {
            dialogueActor = GetComponent<DialogueActor>();
        }
    }

    public void Interact()
    {
        // Check if we're in accusation phase
        if (InvestigationManager.Instance != null && InvestigationManager.Instance.IsAccusationPhaseActive())
        {
            // ACCUSATION MODE - Load ending scene
            AccuseWitness();
        }
        else
        {
            // NORMAL MODE - Regular dialogue
            NormalInteraction();
        }
    }

    void NormalInteraction()
    {
        // If they have a DialogueActor, use that
        if (dialogueActor != null)
        {
            dialogueActor.Interact();
        }
        // Otherwise use our normal dialogue node
        else if (normalDialogueNode != null && normalDialogueNode.lines != null && normalDialogueNode.lines.Length > 0)
        {
            DialogueManager.Instance.ShowDialogue(normalDialogueNode.lines);
        }
    }

    void AccuseWitness()
    {
        Debug.Log($"[WitnessAccusation] Accusing {witnessName}! Loading ending scene: {endingSceneName}");

        // Optional: Show accusation dialogue first
        if (accusationDialogueNode != null && accusationDialogueNode.lines != null && accusationDialogueNode.lines.Length > 0)
        {
            DialogueManager.Instance.ShowDialogue(accusationDialogueNode.lines);
            
            // Load scene after a delay (to let dialogue show)
            Invoke(nameof(LoadEndingScene), 2f);
        }
        else
        {
            // Load ending scene immediately
            LoadEndingScene();
        }
    }

    void LoadEndingScene()
    {
        if (!string.IsNullOrEmpty(endingSceneName))
        {
            Debug.Log($"[WitnessAccusation] Loading ending scene: {endingSceneName}");
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
}