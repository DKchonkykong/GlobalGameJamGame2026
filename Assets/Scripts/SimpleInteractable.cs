using UnityEngine;

public class SimpleInteractable : MonoBehaviour, IInteractable
{
    [TextArea(2, 4)]
    public string[] dialogueLines;

    public string prompt = "Inspect";

    public string PromptText => $"Press E to {prompt}";

    public void Interact()
    {
        DialogueManager.Instance.ShowDialogue("Detective", dialogueLines);
    }
}
