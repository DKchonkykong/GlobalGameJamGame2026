using System.Collections.Generic;
using UnityEngine;

public class PoliceInvestigator : MonoBehaviour, IInteractable, IEvidenceReceiver
{
    [Header("Identity")]
    public string displayName = "Police";

    [Header("Required Evidence for Solution")]
    public EvidenceItem detectiveBadge;
    public EvidenceItem brokenSwitch;
    public EvidenceItem silverMask;
    public EvidenceItem autopsyReport;

    [Header("Evidence Response Dialogues")]
    public DialogueNode badgeResponseNode;
    public DialogueNode switchResponseNode;
    public DialogueNode maskResponseNode;
    public DialogueNode autopsyResponseNode;

    [Header("Progress Dialogue States")]
    public DialogueNode greetingNode;
    public DialogueNode needMoreEvidenceNode;
    public DialogueNode allEvidenceReceivedNode;

    [Header("Fallback Response")]
    [TextArea(2, 3)]
    public string defaultResponseText = "I don't think that's relevant right now.";

    // Track which evidence has been presented
    private HashSet<string> receivedEvidence = new HashSet<string>();
    private bool hasAllEvidence = false;
    
    // Reference to evidence giver component
    private PoliceEvidenceGiver evidenceGiver;

    public string PromptText => $"Press E to talk to {displayName}";

    void Awake()
    {
        LoadState();
        
        // Get reference to evidence giver if it exists
        evidenceGiver = GetComponent<PoliceEvidenceGiver>();
    }

    void LoadState()
    {
        // Load from NPCStateManager if needed
        if (NPCStateManager.Instance != null)
        {
            NPCState state = NPCStateManager.Instance.GetNPCState("Police");
            if (state != null && state.evidenceGiven != null)
            {
                foreach (string evidenceId in state.evidenceGiven)
                {
                    receivedEvidence.Add(evidenceId);
                }

                // Check if we already have all evidence
                CheckIfAllEvidenceReceived();
            }
        }
    }

    void SaveState()
    {
        if (NPCStateManager.Instance != null)
        {
            NPCState state = NPCStateManager.Instance.GetNPCState("Police");
            if (state == null)
            {
                state = new NPCState
                {
                    actorId = "Police",
                    hasReceivedEvidence = hasAllEvidence
                };
            }

            state.evidenceGiven.Clear();
            foreach (string evidenceId in receivedEvidence)
            {
                state.evidenceGiven.Add(evidenceId);
            }

            NPCStateManager.Instance.SaveNPCState("Police", "", hasAllEvidence);
        }
    }

    public void Interact()
    {
        if (ConversationContext.Instance != null)
        {
            ConversationContext.Instance.SetActiveReceiver(this);
        }

        // Check if we should give autopsy report on first interaction
        if (evidenceGiver != null && evidenceGiver.ShouldGiveAutopsy())
        {
            // Show autopsy dialogue, then give the report
            DialogueNode autopsyDialogue = evidenceGiver.GetAutopsyDialogue();
            if (autopsyDialogue != null && autopsyDialogue.lines != null && autopsyDialogue.lines.Length > 0)
            {
                DialogueManager.Instance.ShowDialogue(autopsyDialogue.lines);
                
                // Give the autopsy report after showing dialogue
                evidenceGiver.GiveAutopsyReport();
            }
            return;
        }

        // Normal interaction - show dialogue based on evidence state
        DialogueNode nodeToShow = GetCurrentDialogueNode();

        if (nodeToShow != null && nodeToShow.lines != null && nodeToShow.lines.Length > 0)
        {
            DialogueManager.Instance.ShowDialogue(nodeToShow.lines);
        }
    }

    DialogueNode GetCurrentDialogueNode()
    {
        if (hasAllEvidence && allEvidenceReceivedNode != null)
        {
            return allEvidenceReceivedNode;
        }
        else if (receivedEvidence.Count > 0 && needMoreEvidenceNode != null)
        {
            return needMoreEvidenceNode;
        }
        else if (greetingNode != null)
        {
            return greetingNode;
        }

        return null;
    }

    public bool ReceiveEvidence(EvidenceItem item)
    {
        if (item == null) return false;

        Debug.Log($"[Police] Received evidence: {item.name}");

        // Check if this is one of the required evidence items
        if (IsRequiredEvidence(item))
        {
            bool isNewEvidence = !receivedEvidence.Contains(item.name);

            if (isNewEvidence)
            {
                receivedEvidence.Add(item.name);
                Debug.Log($"[Police] Added {item.name} to evidence collection. Total: {receivedEvidence.Count}/4");

                CheckIfAllEvidenceReceived();
                SaveState();

                // Show the specific evidence response + progress dialogue
                ShowEvidenceResponseWithProgress(item);
            }
            else
            {
                Debug.Log($"[Police] Already have {item.name}");
                // Show a "you already showed me this" response
                ShowAlreadySeenResponse(item);
            }

            return true;
        }
        else
        {
            // Not required evidence
            Debug.Log($"[Police] {item.name} is not relevant to the investigation");
            ShowDefaultResponse();
            return false;
        }
    }

    bool IsRequiredEvidence(EvidenceItem item)
    {
        return item == detectiveBadge ||
               item == brokenSwitch ||
               item == silverMask ||
               item == autopsyReport;
    }

    void CheckIfAllEvidenceReceived()
    {
        bool hadAllEvidence = hasAllEvidence;

        hasAllEvidence = receivedEvidence.Count >= 4 &&
                        receivedEvidence.Contains(detectiveBadge.name) &&
                        receivedEvidence.Contains(brokenSwitch.name) &&
                        receivedEvidence.Contains(silverMask.name) &&
                        receivedEvidence.Contains(autopsyReport.name);

        if (hasAllEvidence && !hadAllEvidence)
        {
            Debug.Log("[Police] ALL EVIDENCE COLLECTED! Investigation can proceed to final accusation.");
        }
    }

    void ShowEvidenceResponseWithProgress(EvidenceItem item)
    {
        // Get the specific dialogue node for this evidence
        DialogueNode evidenceNode = GetEvidenceDialogueNode(item);
        
        // Get the progress dialogue node
        DialogueNode progressNode = GetProgressDialogueNode();

        // Combine both dialogues
        List<DialogueLine> combinedLines = new List<DialogueLine>();

        // First, add the specific evidence response
        if (evidenceNode != null && evidenceNode.lines != null)
        {
            combinedLines.AddRange(evidenceNode.lines);
        }

        // Then, add the progress dialogue
        if (progressNode != null && progressNode.lines != null)
        {
            combinedLines.AddRange(progressNode.lines);
        }

        // Show the combined dialogue
        if (combinedLines.Count > 0)
        {
            DialogueManager.Instance.ShowDialogue(combinedLines.ToArray());
        }
    }

    DialogueNode GetEvidenceDialogueNode(EvidenceItem item)
    {
        if (item == detectiveBadge && badgeResponseNode != null)
            return badgeResponseNode;
        else if (item == brokenSwitch && switchResponseNode != null)
            return switchResponseNode;
        else if (item == silverMask && maskResponseNode != null)
            return maskResponseNode;
        else if (item == autopsyReport && autopsyResponseNode != null)
            return autopsyResponseNode;

        return null;
    }

    DialogueNode GetProgressDialogueNode()
    {
        if (hasAllEvidence && allEvidenceReceivedNode != null)
        {
            return allEvidenceReceivedNode;
        }
        else if (receivedEvidence.Count > 0 && needMoreEvidenceNode != null)
        {
            return needMoreEvidenceNode;
        }

        return null;
    }

    void ShowAlreadySeenResponse(EvidenceItem item)
    {
        string itemName = GetEvidenceName(item);
        
        DialogueLine[] responseLines = new DialogueLine[]
        {
            new DialogueLine { speaker = displayName, text = $"You already showed me the {itemName}." }
        };

        DialogueManager.Instance.ShowDialogue(responseLines);
    }

    string GetEvidenceName(EvidenceItem item)
    {
        if (item == detectiveBadge)
            return "Detective Badge";
        else if (item == brokenSwitch)
            return "Broken Light Switch";
        else if (item == silverMask)
            return "Silver Mask";
        else if (item == autopsyReport)
            return "Autopsy Report";

        return "evidence";
    }

    void ShowDefaultResponse()
    {
        DialogueLine[] defaultLines = new DialogueLine[]
        {
            new DialogueLine { speaker = displayName, text = defaultResponseText }
        };

        DialogueManager.Instance.ShowDialogue(defaultLines);
    }

    // Public method to check if investigation is ready
    public bool IsReadyForAccusation()
    {
        return hasAllEvidence;
    }

    // Public method to get evidence progress
    public int GetEvidenceCount()
    {
        return receivedEvidence.Count;
    }

    // Debug method to check status
    [ContextMenu("Debug Evidence Status")]
    void DebugStatus()
    {
        Debug.Log($"=== Police Evidence Status ===");
        Debug.Log($"Total Evidence: {receivedEvidence.Count}/4");
        Debug.Log($"Has All Evidence: {hasAllEvidence}");
        Debug.Log($"Received Items:");
        foreach (string evidence in receivedEvidence)
        {
            Debug.Log($"  - {evidence}");
        }
    }
}