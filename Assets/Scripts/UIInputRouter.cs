using UnityEngine;
using UnityEngine.InputSystem;

public class UIInputRouter : MonoBehaviour
{
    public EvidenceUI evidenceUI;

    [Header("Keys")]
    public Key inventoryKey = Key.Tab;
    public Key nextKey1 = Key.E;
    public Key nextKey2 = Key.RightArrow;
    public Key prevKey1 = Key.Q;
    public Key prevKey2 = Key.LeftArrow;
    public Key closeKey = Key.Escape;

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null || evidenceUI == null) return;

        // Toggle inventory (only if NOT in evidence already OR you want it to close too)
        if (kb[inventoryKey].wasPressedThisFrame)
        {
            evidenceUI.Toggle();
            return;
        }

        // Only allow cycling when panel is open
        if (!evidenceUI.IsOpen) return;

        if (kb[closeKey].wasPressedThisFrame)
        {
            evidenceUI.Close();
            return;
        }

        if (kb[nextKey1].wasPressedThisFrame || kb[nextKey2].wasPressedThisFrame)
            evidenceUI.NextItem();

        if (kb[prevKey1].wasPressedThisFrame || kb[prevKey2].wasPressedThisFrame)
            evidenceUI.PrevItem();
    }
}
