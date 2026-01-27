using System.Collections.Generic;
using UnityEngine;

public class EvidenceManager : MonoBehaviour
{
    public static EvidenceManager Instance { get; private set; }

    private readonly List<EvidenceItem> collected = new List<EvidenceItem>();

    public IReadOnlyList<EvidenceItem> Collected => collected;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool AddEvidence(EvidenceItem item)
    {
        if (item == null) return false;
        if (collected.Contains(item)) return false;

        collected.Add(item);
        return true;
    }
}
