using UnityEngine;

public class NPCDialogue : MonoBehaviour, IEvidenceReceiver
{
    [Header("Start Node")]
    public DialogueNode startNode;

    [Header("After Verified Node (optional)")]
    public DialogueNode afterVerifiedNode;

    private DialogueNode currentNode;
    private bool verified;

    void Awake()
    {
        currentNode = startNode;
    }

    // Call this when player presses E on the NPC
    public void Talk()
    {
        if (verified && afterVerifiedNode != null)
            currentNode = afterVerifiedNode;

        if (currentNode == null) return;

        DialogueManager.Instance.ShowDialogue(currentNode.lines);

        // If this node is NOT evidence-gated, we can auto-advance currentNode
        if (currentNode.requiredEvidence == null)
            currentNode = currentNode.next;
        // If it IS gated, we wait for evidence via ReceiveEvidence(...)
    }

    // Called when player "presents" evidence while talking to this NPC
    public bool ReceiveEvidence(EvidenceItem item)
    {
        if (currentNode == null) return false;

        // Node not expecting evidence
        if (currentNode.requiredEvidence == null)
            return false;

        if (item == currentNode.requiredEvidence)
        {
            verified = true;
            if (currentNode.ifEvidenceCorrect != null)
                currentNode = currentNode.ifEvidenceCorrect;

            if (currentNode != null)
                DialogueManager.Instance.ShowDialogue(currentNode.lines);

            // after showing success node, hop to its next
            if (currentNode != null)
                currentNode = currentNode.next;

            return true;
        }
        else
        {
            // wrong evidence
            var wrong = currentNode.ifEvidenceWrong;
            if (wrong != null)
                DialogueManager.Instance.ShowDialogue(wrong.lines);
            else if (currentNode.ifNoEvidence != null)
                DialogueManager.Instance.ShowDialogue(currentNode.ifNoEvidence.lines);

            return false;
        }
    }
}
