using UnityEngine;
using UnityEngine.InputSystem;

public class UIInputRouter : MonoBehaviour
{
    public EvidenceUI evidenceUI;

    public void OnInventory(InputValue value)
    {
        if (!value.isPressed) return;

        // Don't open evidence while dialogue is open
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen)
            return;

        evidenceUI.Toggle();
    }
}
