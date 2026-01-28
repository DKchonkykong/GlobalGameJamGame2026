using System.Collections.Generic;
using UnityEngine;

public class NPCStateManager : MonoBehaviour
{
    public static NPCStateManager Instance { get; private set; }

    private Dictionary<string, NPCState> npcStates = new Dictionary<string, NPCState>();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SaveNPCState(string actorId, string currentNodeId, bool hasReceivedEvidence)
    {
        if (npcStates.ContainsKey(actorId))
        {
            npcStates[actorId].currentNodeId = currentNodeId;
            npcStates[actorId].hasReceivedEvidence = hasReceivedEvidence;
        }
        else
        {
            npcStates[actorId] = new NPCState
            {
                actorId = actorId,
                currentNodeId = currentNodeId,
                hasReceivedEvidence = hasReceivedEvidence
            };
        }
    }

    public NPCState GetNPCState(string actorId)
    {
        if (npcStates.TryGetValue(actorId, out NPCState state))
        {
            return state;
        }
        return null;
    }

    public bool HasInteractedWith(string actorId)
    {
        return npcStates.ContainsKey(actorId);
    }

    // Optional: Clear all states (for new game)
    public void ClearAllStates()
    {
        npcStates.Clear();
    }
}

[System.Serializable]
public class NPCState
{
    public string actorId;
    public string currentNodeId;
    public bool hasReceivedEvidence;
    public List<string> evidenceGiven = new List<string>();
}