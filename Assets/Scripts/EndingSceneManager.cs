using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndingSceneManager : MonoBehaviour
{
    [Header("Scene Settings")]
    public string titleSceneName = "TitleScreen"; // Name of your title/main menu scene
    
    [Header("UI References")]
    public Button exitButton; // Drag your Exit button here in the Inspector

    void Start()
    {
        Debug.Log("[EndingSceneManager] Ending scene started");
        
        // Make sure cursor is visible and unlocked for UI interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Hook up the exit button if assigned
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ReturnToTitle);
            Debug.Log("[EndingSceneManager] Exit button listener added");
        }
        else
        {
            Debug.LogWarning("[EndingSceneManager] Exit button not assigned!");
        }
        
        // Clean up any persistent managers
        CleanupPersistentObjects();
    }

    public void ReturnToTitle()
    {
        Debug.Log("[EndingSceneManager] Returning to title screen");
        SceneManager.LoadScene(titleSceneName);
    }

    void CleanupPersistentObjects()
    {
        // Destroy any DontDestroyOnLoad objects that should be reset
        if (InvestigationManager.Instance != null)
        {
            Destroy(InvestigationManager.Instance.gameObject);
            Debug.Log("[EndingSceneManager] Destroyed InvestigationManager");
        }

        if (EvidenceManager.Instance != null)
        {
            Destroy(EvidenceManager.Instance.gameObject);
            Debug.Log("[EndingSceneManager] Destroyed EvidenceManager");
        }

        if (DialogueManager.Instance != null)
        {
            Destroy(DialogueManager.Instance.gameObject);
            Debug.Log("[EndingSceneManager] Destroyed DialogueManager");
        }

        if (NPCStateManager.Instance != null)
        {
            Destroy(NPCStateManager.Instance.gameObject);
            Debug.Log("[EndingSceneManager] Destroyed NPCStateManager");
        }
    }
}