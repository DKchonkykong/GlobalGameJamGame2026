using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Node")]
public class DialogueNode : ScriptableObject
{
    public string nodeId; // unique, like "receptionist_intro"
    public DialogueLine[] lines;

    [Header("Optional Evidence Gate")]
    public EvidenceItem requiredEvidence;     // e.g. DetectiveBadge
    public DialogueNode ifEvidenceCorrect;    // next node if correct evidence presented
    public DialogueNode ifEvidenceWrong;      // optional (can be null)
    public DialogueNode ifNoEvidence;         // optional (can be null)

    [Header("Fallback / Default Next")]
    public DialogueNode next;                 // when lines finish and no gate is active
}
