using UnityEngine;

public class ConversationContext : MonoBehaviour
{
    public static ConversationContext Instance { get; private set; }
    public IEvidenceReceiver ActiveReceiver { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void SetActiveReceiver(IEvidenceReceiver receiver)
    {
        ActiveReceiver = receiver;
    }

    public void Clear()
    {
        ActiveReceiver = null;
    }
}
