using System;
using System.Collections.Generic;
using UnityEngine;

public class EvidenceManager : MonoBehaviour
{
    public static EvidenceManager Instance { get; private set; }

    [Header("Capacity / Starting Evidence")]
    [SerializeField] private int maxEvidence = 4;
    [SerializeField] private List<EvidenceItem> startingEvidence = new();

    private readonly List<EvidenceItem> collected = new();
    public IReadOnlyList<EvidenceItem> Collected => collected;

    // Fired when the list changes (added/removed/cleared)
    public event Action OnEvidenceChanged;

    // Fired only when a new item is successfully added
    public event Action<EvidenceItem> OnEvidenceAdded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Optional: persist between scenes
        // DontDestroyOnLoad(gameObject);

        InitialiseStartingEvidence();
    }

    private void InitialiseStartingEvidence()
    {
        collected.Clear();

        foreach (var item in startingEvidence)
        {
            if (item == null) continue;
            if (collected.Count >= maxEvidence) break;
            if (!collected.Contains(item))
                collected.Add(item);
        }

        OnEvidenceChanged?.Invoke();
    }

    public bool AddEvidence(EvidenceItem item)
    {
        if (item == null) return false;
        if (collected.Contains(item)) return false;
        if (collected.Count >= maxEvidence) return false;

        collected.Add(item);

        OnEvidenceAdded?.Invoke(item);
        OnEvidenceChanged?.Invoke();
        return true;
    }

    public int MaxEvidence => maxEvidence;
}
