using UnityEngine;

public class SimpleInteractable : MonoBehaviour, IInteractable
{
    [TextArea] public string message = "You found something...";
    public string prompt = "Inspect";

    public string PromptText => $"Press E to {prompt}";

    public void Interact()
    {
        DialogueManager.Instance.ShowMessage("Detective", message);
    }
}
