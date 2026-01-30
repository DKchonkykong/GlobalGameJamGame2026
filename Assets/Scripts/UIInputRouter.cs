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

    public void Update()
    {
        var kb = Keyboard.current;
        if (kb == null || evidenceUI == null) return;

        // Toggle inventory with Tab
        if (kb[inventoryKey].wasPressedThisFrame)
        {
            evidenceUI.Toggle();
            return;
        }

        // Only process other keys if evidence UI is open
        if (!evidenceUI.IsOpen) return;

        if (kb[closeKey].wasPressedThisFrame)
        {
            evidenceUI.Close();
        }

        if (kb[nextKey].wasPressedThisFrame)
        {
            evidenceUI.NextEvidence();
        }

        if (kb[prevKey].wasPressedThisFrame)
        {
            evidenceUI.PrevEvidence();
        }
    }

    // Make public methods so buttons can call them too
    public void NextEvidence() => evidenceUI?.NextEvidence();
    public void PrevEvidence() => evidenceUI?.PrevEvidence();
}
