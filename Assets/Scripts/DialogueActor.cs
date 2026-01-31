using System.Collections.Generic;
using UnityEngine;

public class DialogueActor : MonoBehaviour, IInteractable, IEvidenceReceiver
{
    [Header("Identity")]
    public string actorId = "Receptionist";
    public string displayName = "Receptionist";

    [Header("Dialogue Flow")]
    public DialogueNode startingNode;
    public DialogueNode afterEvidenceNode;

    [System.Serializable]
    public class EvidenceReaction
    {
        public EvidenceItem evidence;
        public DialogueNode onFirstPresent;  // First time showing this evidence
        public DialogueNode onRepeat;        // Subsequent times
    }

    [Header("Evidence Reactions")]
    public List<EvidenceReaction> evidenceReactions = new List<EvidenceReaction>();

    [Header("Fallback Responses")]
    [Tooltip("Show a generic response when presenting unrelated evidence? If false, NPC will be silent.")]
    public bool showDefaultResponse = false;
    [TextArea(2, 3)]
    public string defaultResponseText = "I don't think that's relevant right now.";

    // Private state (not shown in Inspector)
    private DialogueNode currentNode;
    private bool hasReceivedCorrectEvidence;
    private HashSet<string> presentedEvidenceIds = new HashSet<string>();

    public string PromptText => $"Press E to talk to {displayName}";

    void Awake()
    {
        LoadState();
    }

    void LoadState()
    {
        if (NPCStateManager.Instance != null)
        {
            NPCState state = NPCStateManager.Instance.GetNPCState(actorId);
            if (state != null)
            {
                hasReceivedCorrectEvidence = state.hasReceivedEvidence;

                // Restore presented evidence
                if (state.evidenceGiven != null)
                {
                    foreach (string evidenceId in state.evidenceGiven)
                    {
                        presentedEvidenceIds.Add(evidenceId);
                    }
                }
            }
        }

        if (currentNode == null)
        {
            currentNode = startingNode;
        }
    }

    void SaveState()
    {
        if (NPCStateManager.Instance != null)
        {
            NPCState state = NPCStateManager.Instance.GetNPCState(actorId);
            if (state == null)
            {
                state = new NPCState
                {
                    actorId = actorId,
                    hasReceivedEvidence = hasReceivedCorrectEvidence
                };
            }

            // Update evidence list
            state.evidenceGiven.Clear();
            foreach (string evidenceId in presentedEvidenceIds)
            {
                state.evidenceGiven.Add(evidenceId);
            }

            string nodeId = currentNode != null ? currentNode.nodeId : "";
            NPCStateManager.Instance.SaveNPCState(actorId, nodeId, hasReceivedCorrectEvidence);
        }
    }

    public void Interact()
    {
        if (ConversationContext.Instance != null)
        {
            ConversationContext.Instance.SetActiveReceiver(this);
        }

        DialogueManager.Instance.StartConversation(this);
    }

    public DialogueNode GetCurrentNode()
    {
        // If we've received correct evidence and have a special node for that
        if (hasReceivedCorrectEvidence && afterEvidenceNode != null)
        {
            return afterEvidenceNode;
        }

        return currentNode;
    }

    public bool ReceiveEvidence(EvidenceItem item)
    {
        if (item == null) return false;

        Debug.Log($"[{actorId}] Received evidence: {item.name}");

        // Check if this is evidence we're specifically waiting for (from dialogue nodes)
        if (currentNode != null && currentNode.requiredEvidence != null)
        {
            Debug.Log($"[{actorId}] Current node has required evidence: {currentNode.requiredEvidence.name}");
            return HandleRequiredEvidence(item);
        }

        // Check custom evidence reactions
        EvidenceReaction reaction = evidenceReactions.Find(r => r.evidence == item);
        if (reaction != null)
        {
            Debug.Log($"[{actorId}] Found evidence reaction for {item.name}");
            HandleEvidenceReaction(item, reaction);
            return true;
        }

        Debug.Log($"[{actorId}] No reaction found for {item.name}, using default response");

        // Default: This NPC doesn't recognize this evidence
        if (showDefaultResponse)
        {
            DialogueLine[] defaultLines = new DialogueLine[]
            {
                new DialogueLine { speaker = displayName, text = defaultResponseText }
            };
            DialogueManager.Instance.ShowDialogue(defaultLines);
        }
        
        return false;
    }

    bool HandleRequiredEvidence(EvidenceItem item)
    {
        if (item == currentNode.requiredEvidence)
        {
            Debug.Log($"[{actorId}] Correct evidence! Transitioning node.");
            hasReceivedCorrectEvidence = true;

            if (currentNode.ifEvidenceCorrect != null)
            {
                currentNode = currentNode.ifEvidenceCorrect;
                Debug.Log($"[{actorId}] Showing dialogue from node: {currentNode.nodeId}");
                DialogueManager.Instance.ShowDialogue(currentNode.lines);

                if (currentNode.next != null)
                {
                    currentNode = currentNode.next;
                }
            }

            // Track that we've presented this
            if (!presentedEvidenceIds.Contains(item.name))
            {
                presentedEvidenceIds.Add(item.name);
            }

            SaveState();
            return true;
        }
        else
        {
            Debug.Log($"[{actorId}] Wrong evidence presented.");
            // Wrong evidence
            DialogueNode wrongNode = currentNode.ifEvidenceWrong ?? currentNode.ifNoEvidence;

            if (wrongNode != null)
            {
                Debug.Log($"[{actorId}] Showing wrong evidence dialogue from node: {wrongNode.nodeId}");
                DialogueManager.Instance.ShowDialogue(wrongNode.lines);
            }
            else
            {
                DialogueLine[] wrongLines = new DialogueLine[]
                {
                    new DialogueLine { speaker = displayName, text = "That's not what I need." }
                };
                DialogueManager.Instance.ShowDialogue(wrongLines);
            }
            return false;
        }
    }

    void HandleEvidenceReaction(EvidenceItem item, EvidenceReaction reaction)
    {
        bool hasSeenBefore = presentedEvidenceIds.Contains(item.name);

        DialogueNode nodeToShow = null;

        if (hasSeenBefore && reaction.onRepeat != null)
        {
            // Already shown this before
            nodeToShow = reaction.onRepeat;
            Debug.Log($"[{actorId}] Using REPEAT node: {nodeToShow.nodeId}");
        }
        else if (!hasSeenBefore && reaction.onFirstPresent != null)
        {
            // First time showing this
            nodeToShow = reaction.onFirstPresent;
            Debug.Log($"[{actorId}] Using FIRST PRESENT node: {nodeToShow.nodeId}");
            presentedEvidenceIds.Add(item.name);
            SaveState();
        }

        if (nodeToShow != null && nodeToShow.lines != null && nodeToShow.lines.Length > 0)
        {
            Debug.Log($"[{actorId}] Showing {nodeToShow.lines.Length} dialogue lines. First speaker: {nodeToShow.lines[0].speaker}");
            DialogueManager.Instance.ShowDialogue(nodeToShow.lines);
        }
        else
        {
            Debug.LogWarning($"[{actorId}] Evidence reaction node is null or has no lines!");
            // Fallback
            string response = hasSeenBefore
                ? "You already showed me that."
                : "Interesting...";

            DialogueLine[] fallbackLines = new DialogueLine[]
            {
                new DialogueLine { speaker = displayName, text = response }
            };
            DialogueManager.Instance.ShowDialogue(fallbackLines);
        }
    }

    public void ResetState()
    {
        currentNode = startingNode;
        hasReceivedCorrectEvidence = false;
        presentedEvidenceIds.Clear();
        SaveState();
    }
}