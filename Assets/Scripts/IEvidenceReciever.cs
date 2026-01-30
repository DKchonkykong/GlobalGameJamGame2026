public interface IEvidenceReceiver
{
    // Return true if the NPC accepted it (so you can branch dialogue/state).
    bool ReceiveEvidence(EvidenceItem item);
}
