using UnityEngine;

public class PoliceEvidenceGiver : MonoBehaviour
{
    [Header("Evidence to Give")]
    public EvidenceItem autopsyReport; // The autopsy report to give
    
    [Header("Dialogue")]
    public DialogueNode autopsyDialogueNode; // Dialogue about the autopsy report
    
    private bool hasGivenAutopsy = false;
    private const string SAVE_KEY = "Police_HasGivenAutopsy";

    void Awake()
    {
        LoadState();
    }

    void LoadState()
    {
        hasGivenAutopsy = PlayerPrefs.GetInt(SAVE_KEY, 0) == 1;
        Debug.Log($"[PoliceEvidenceGiver] Loaded state - hasGivenAutopsy: {hasGivenAutopsy}");
    }

    void SaveState()
    {
        PlayerPrefs.SetInt(SAVE_KEY, hasGivenAutopsy ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"[PoliceEvidenceGiver] Saved state - hasGivenAutopsy: {hasGivenAutopsy}");
    }

    // Check if we should give the autopsy report on this interaction
    public bool ShouldGiveAutopsy()
    {
        return !hasGivenAutopsy;
    }

    // This gets called when the autopsy dialogue finishes
    public void GiveAutopsyReport()
    {
        if (hasGivenAutopsy)
        {
            Debug.LogWarning("[PoliceEvidenceGiver] Autopsy report already given!");
            return;
        }

        Debug.Log($"[PoliceEvidenceGiver] Giving autopsy report");
        
        // Add evidence to player's inventory
        if (EvidenceManager.Instance != null && autopsyReport != null)
        {
            EvidenceManager.Instance.AddEvidence(autopsyReport);
        }
        
        // Mark as given
        hasGivenAutopsy = true;
        SaveState();
    }

    public DialogueNode GetAutopsyDialogue()
    {
        return autopsyDialogueNode;
    }

    // Debug method
    [ContextMenu("Reset Autopsy Given State")]
    void ResetState()
    {
        hasGivenAutopsy = false;
        SaveState();
        Debug.Log("[PoliceEvidenceGiver] State reset - autopsy can be given again");
    }
}