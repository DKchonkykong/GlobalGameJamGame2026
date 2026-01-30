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

    public void ReceiveEvidence(EvidenceItem item)
    {
        if (item == null) return;

        // Check if this is evidence we're specifically waiting for (from dialogue nodes)
        if (currentNode != null && currentNode.requiredEvidence != null)
        {
            HandleRequiredEvidence(item);
            return;
        }

        // Check custom evidence reactions
        EvidenceReaction reaction = evidenceReactions.Find(r => r.evidence == item);
        if (reaction != null)
        {
            HandleEvidenceReaction(item, reaction);
            return;
        }

        // Default: not interested in this evidence
        DialogueLine[] defaultLines = new DialogueLine[]
        {
            new DialogueLine { speaker = displayName, text = "I don't think that's relevant right now." }
        };
        DialogueManager.Instance.ShowDialogue(defaultLines);
    }

    void HandleRequiredEvidence(EvidenceItem item)
    {
        if (item == currentNode.requiredEvidence)
        {
            hasReceivedCorrectEvidence = true;

            if (currentNode.ifEvidenceCorrect != null)
            {
                currentNode = currentNode.ifEvidenceCorrect;
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
        }
        else
        {
            // Wrong evidence
            DialogueNode wrongNode = currentNode.ifEvidenceWrong ?? currentNode.ifNoEvidence;

            if (wrongNode != null)
            {
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
        }
        else if (!hasSeenBefore && reaction.onFirstPresent != null)
        {
            // First time showing this
            nodeToShow = reaction.onFirstPresent;
            presentedEvidenceIds.Add(item.name);
            SaveState();
        }

        if (nodeToShow != null && nodeToShow.lines != null && nodeToShow.lines.Length > 0)
        {
            DialogueManager.Instance.ShowDialogue(nodeToShow.lines);
        }
        else
        {
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