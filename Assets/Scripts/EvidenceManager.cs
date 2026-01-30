using System;
using System.Collections.Generic;
using UnityEngine;

public class EvidenceManager : MonoBehaviour
{
    public static EvidenceManager Instance { get; private set; }

    [Header("Capacity / Starting Evidence")]
    public int maxEvidence = 4;
    public List<EvidenceItem> startingEvidence = new();

    private readonly List<EvidenceItem> evidenceList = new();
    private int currentIndex = 0;

    public event Action OnEvidenceChanged;
    public event Action<EvidenceItem> OnEvidenceAdded; // Added this event

    public IReadOnlyList<EvidenceItem> Evidence => evidenceList;
    public int Count => evidenceList.Count;
    public int CurrentIndex => currentIndex;

    public EvidenceItem Current
    {
        get
        {
            if (evidenceList.Count == 0) return null;
            currentIndex = Mathf.Clamp(currentIndex, 0, evidenceList.Count - 1);
            return evidenceList[currentIndex];
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Optional: keep across scenes if you want
        // DontDestroyOnLoad(gameObject);

        // Load starting items once
        foreach (var item in startingEvidence)
            AddEvidence(item, showToast: false);

        if (evidenceList.Count > 0) currentIndex = 0;
        OnEvidenceChanged?.Invoke();
    }

    public bool AddEvidence(EvidenceItem item, bool showToast = true)
    {
        if (item == null) return false;
        if (evidenceList.Contains(item)) return false;
        if (evidenceList.Count >= maxEvidence) return false;

        evidenceList.Add(item);
        currentIndex = Mathf.Clamp(currentIndex, 0, evidenceList.Count - 1);
        
        OnEvidenceChanged?.Invoke();
        
        // Fire the OnEvidenceAdded event only if showToast is true
        if (showToast)
        {
            OnEvidenceAdded?.Invoke(item);
        }
        
        return true;
    }

    public void Next()
    {
        if (evidenceList.Count == 0) return;
        currentIndex = (currentIndex + 1) % evidenceList.Count;
        OnEvidenceChanged?.Invoke();
    }

    public void Prev()
    {
        if (evidenceList.Count == 0) return;
        currentIndex = (currentIndex - 1 + evidenceList.Count) % evidenceList.Count;
        OnEvidenceChanged?.Invoke();
    }
}
