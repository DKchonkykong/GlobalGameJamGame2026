using UnityEngine;

public class EvidencePickup : MonoBehaviour, IInteractable
{
    public EvidenceItem evidence;
    public string prompt = "Inspect";

    [TextArea(2, 5)]
    public string[] dialogueLinesOnPickup;

    public bool disableAfterPickup = true;

    public string PromptText => $"Press E to {prompt}";

    public void Interact()
    {
        bool added = EvidenceManager.Instance.AddEvidence(evidence);

        if (added)
        {
            if (dialogueLinesOnPickup != null && dialogueLinesOnPickup.Length > 0)
                DialogueManager.Instance.ShowDialogue("Detective", dialogueLinesOnPickup);
            else
                DialogueManager.Instance.ShowDialogue("Detective", new[] { $"Added {evidence.displayName} to evidence." });

            if (disableAfterPickup)
                gameObject.SetActive(false);
        }
        else
        {
            DialogueManager.Instance.ShowDialogue("Detective", new[] { "Can't pick that up (evidence full or already collected)." });
        }
    }

}
