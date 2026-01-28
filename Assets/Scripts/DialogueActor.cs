using UnityEngine;

public class DialogueActor : MonoBehaviour, IInteractable, IEvidenceReceiver
{
    [Header("Identity")]
    public string actorId = "Receptionist";
    public string displayName = "Receptionist";

    [Header("Dialogue Flow")]
    public DialogueNode startingNode;
    public DialogueNode afterEvidenceNode;

    private DialogueNode currentNode;
    private bool hasReceivedCorrectEvidence;

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
                // You could also restore currentNode based on state.currentNodeId
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
        if (hasReceivedCorrectEvidence && afterEvidenceNode != null)
        {
            return afterEvidenceNode;
        }

        return currentNode;
    }

    public void ReceiveEvidence(EvidenceItem item)
    {
        if (currentNode == null || currentNode.requiredEvidence == null)
        {
            DialogueLine[] noNeedLines = new DialogueLine[]
            {
                new DialogueLine { speaker = displayName, text = "I don't need that right now." }
            };
            DialogueManager.Instance.ShowDialogue(noNeedLines);
            return;
        }

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

            SaveState();
        }
        else
        {
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

    public void ResetState()
    {
        currentNode = startingNode;
        hasReceivedCorrectEvidence = false;
        SaveState();
    }
}