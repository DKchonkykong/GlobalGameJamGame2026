using UnityEngine;

public class InvestigationManager : MonoBehaviour
{
    public static InvestigationManager Instance { get; private set; }

    [Header("Investigation State")]
    private bool isAccusationPhaseActive = false;

    [Header("Police Reference")]
    public PoliceInvestigator policeInvestigator;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // Check if police has all evidence and activate accusation phase
        if (!isAccusationPhaseActive && policeInvestigator != null)
        {
            if (policeInvestigator.IsReadyForAccusation())
            {
                ActivateAccusationPhase();
            }
        }
    }

    void ActivateAccusationPhase()
    {
        if (isAccusationPhaseActive) return;

        Debug.Log("[InvestigationManager] ACCUSATION PHASE ACTIVATED - You can now accuse the suspects!");
        isAccusationPhaseActive = true;

        // Optional: Show a UI notification or message
        // UIManager.Instance.ShowNotification("All evidence collected! Now find the killer.");
    }

    public bool IsAccusationPhaseActive()
    {
        Debug.Log($"[InvestigationManager] IsAccusationPhaseActive called - returning: {isAccusationPhaseActive}");
        return isAccusationPhaseActive;
    }

    // Debug method
    [ContextMenu("Debug Investigation State")]
    void DebugState()
    {
        Debug.Log($"=== Investigation Manager State ===");
        Debug.Log($"Accusation Phase Active: {isAccusationPhaseActive}");
        if (policeInvestigator != null)
        {
            Debug.Log($"Police Ready for Accusation: {policeInvestigator.IsReadyForAccusation()}");
            Debug.Log($"Evidence Count: {policeInvestigator.GetEvidenceCount()}/4");
        }
    }

    [ContextMenu("Force Activate Accusation Phase")]
    void ForceActivateAccusation()
    {
        ActivateAccusationPhase();
    }

    [ContextMenu("Reset Investigation State")]
    void ResetState()
    {
        isAccusationPhaseActive = false;
        Debug.Log("[InvestigationManager] Investigation state reset");
    }
}