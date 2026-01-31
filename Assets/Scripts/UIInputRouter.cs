using UnityEngine;
using UnityEngine.InputSystem;

public class UIInputRouter : MonoBehaviour
{
    public EvidenceUI evidenceUI;

    [Header("Keys")]
    public Key inventoryKey = Key.Tab;
    public Key nextKey = Key.E;
    public Key prevKey = Key.Q;
    public Key closeKey = Key.Escape;
    public Key presentKey = Key.F;

    public void Update()
    {
        var kb = Keyboard.current;
        if (kb == null)
        {
            Debug.LogWarning("UIInputRouter: no Keyboard.current (new Input System not active?)");
            return;
        }
        if (evidenceUI == null)
        {
            Debug.LogWarning("UIInputRouter: evidenceUI not assigned");
            return;
        }

        // Toggle inventory with Tab
        if (kb[inventoryKey].wasPressedThisFrame)
        {
            Debug.Log($"UIInputRouter: Tab pressed - toggling from IsOpen={evidenceUI.IsOpen}");
            evidenceUI.Toggle();
            return; // Skip processing other keys this frame after toggling
        }

        // Only process other keys if evidence UI is open
        if (!evidenceUI.IsOpen)
        {
            return;
        }

        if (kb[closeKey].wasPressedThisFrame)
        {
            Debug.Log("UIInputRouter: Escape pressed - closing");
            evidenceUI.Close();
        }

        if (kb[nextKey].wasPressedThisFrame)
        {
            Debug.Log("UIInputRouter: E pressed - next");
            evidenceUI.NextItem();
        }

        if (kb[prevKey].wasPressedThisFrame)
        {
            Debug.Log("UIInputRouter: Q pressed - prev");
            evidenceUI.PrevItem();
        }

        if (kb[presentKey].wasPressedThisFrame)
        {
            Debug.Log("UIInputRouter: F pressed - presenting evidence");
            evidenceUI.OnPresentPressed();
        }
    }

    public void NextEvidence() => evidenceUI?.NextItem();
    public void PrevEvidence() => evidenceUI?.PrevItem();
    public void PresentEvidence() => evidenceUI?.OnPresentPressed();
}
