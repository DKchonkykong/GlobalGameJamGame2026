using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EvidenceUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel;
    public Image iconImage;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public TMP_Text counterText;

    [Header("Buttons")]
    public Button prevButton;
    public Button nextButton;
    public Button closeButton;
    public Button presentButton;

    [Header("Optional Feedback")]
    public TMP_Text feedbackText;
    public float feedbackDuration = 2f;

    [Header("Player / Control")]
    public MonoBehaviour fpsController; // your FPSController script
    public Transform playerCamera;      // assign PlayerCamera transform (recommended)

    [Header("Present Settings")]
    public float presentRange = 3f;
    [Range(0f, 1f)] public float facingDot = 0.6f;
    public LayerMask npcLayerMask = ~0;

    private EvidenceManager evidenceManager;
    private int index = 0;
    private bool isOpen = false;
    private Coroutine feedbackRoutine;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        evidenceManager = FindAnyObjectByType<EvidenceManager>();
        if (evidenceManager != null)
            evidenceManager.OnEvidenceChanged += HandleEvidenceChanged;

        if (prevButton != null) prevButton.onClick.AddListener(Prev);
        if (nextButton != null) nextButton.onClick.AddListener(Next);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (presentButton != null) presentButton.onClick.AddListener(OnPresentPressed);

        if (panel != null) panel.SetActive(false);
        isOpen = false;
    }

    private void OnDestroy()
    {
        if (evidenceManager != null)
            evidenceManager.OnEvidenceChanged -= HandleEvidenceChanged;
    }

    // NOTE: Removed Update() – keyboard input is handled by UIInputRouter

    // Called by UIInputRouter (Tab)
    public void Toggle()
    {
        if (isOpen) Close();
        else Open();
    }

    public void Open()
    {
        // Don't open evidence while dialogue is open
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen)
            return;

        isOpen = true;
        if (panel != null) panel.SetActive(true);

        LockPlayer(true);
        ClampIndex();
        Refresh();
    }

    public void Close()
    {
        isOpen = false;
        if (panel != null) panel.SetActive(false);

        LockPlayer(false);
        ClearFeedback();
    }

    // wrappers so UIInputRouter compiles
    public void NextItem() => Next();
    public void PrevItem() => Prev();

    public void Next()
    {
        var list = GetEvidenceList();
        if (list.Count == 0) { Refresh(); return; }

        index = (index + 1) % list.Count;
        Refresh();
    }

    public void Prev()
    {
        var list = GetEvidenceList();
        if (list.Count == 0) { Refresh(); return; }

        index = (index - 1 + list.Count) % list.Count;
        Refresh();
    }

    public void OnPresentPressed()
    {
        var list = GetEvidenceList();
        if (list.Count == 0)
        {
            ShowFeedback("No evidence to present.");
            return;
        }

        EvidenceItem selected = list[index];

        // Find a valid NPC target in front of the player
        var target = FindPresentTarget();
        if (target == null)
        {
            ShowFeedback("No one to present this to.");
            return;
        }

        // use ReceiveEvidence (from IEvidenceReceiver) instead of OnEvidencePresented
        bool ok = target.ReceiveEvidence(selected);

        ShowFeedback(ok
            ? $"Presented: {selected.displayName}"
            : "That didn't work.");
        // Optional: Close();
    }

    private void HandleEvidenceChanged()
    {
        ClampIndex();
        if (isOpen) Refresh();
    }

    private void ClampIndex()
    {
        var list = GetEvidenceList();
        if (list.Count == 0) { index = 0; return; }
        index = Mathf.Clamp(index, 0, list.Count - 1);
    }

    private void Refresh()
    {
        var list = GetEvidenceList();

        if (list.Count == 0)
        {
            if (titleText) titleText.text = "No evidence";
            if (descriptionText) descriptionText.text = "No evidence";
            if (descriptionText) descriptionText.text = "You haven't collected any evidence yet.";
            if (counterText) counterText.text = "0/0";
            if (iconImage) iconImage.sprite = null;

            SetNavInteractable(false);
            return;
        }

        EvidenceItem item = list[index];

        // use displayName instead of title
        if (titleText) titleText.text = item.displayName;
        if (descriptionText) descriptionText.text = item.description;
        if (counterText) counterText.text = $"{index + 1}/{list.Count}";
        if (iconImage) iconImage.sprite = item.icon;

        SetNavInteractable(list.Count > 1);
    }

    private void SetNavInteractable(bool canNavigate)
    {
        if (prevButton) prevButton.interactable = canNavigate;
        if (nextButton) nextButton.interactable = canNavigate;
    }

    private List<EvidenceItem> GetEvidenceList()
    {
        if (evidenceManager == null)
            return new List<EvidenceItem>();

        // use EvidenceManager.Evidence instead of non‑existent CurrentEvidence
        return new List<EvidenceItem>(evidenceManager.Evidence);
    }

    private void LockPlayer(bool locked)
    {
        if (fpsController != null)
            fpsController.enabled = !locked;

        Cursor.visible = locked;
        Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void ShowFeedback(string msg)
    {
        if (feedbackText == null) return;

        if (feedbackRoutine != null)
            StopCoroutine(feedbackRoutine);

        feedbackRoutine = StartCoroutine(FeedbackRoutine(msg));
    }

    private IEnumerator FeedbackRoutine(string msg)
    {
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = msg;

        yield return new WaitForSecondsRealtime(feedbackDuration);

        feedbackText.text = "";
        feedbackText.gameObject.SetActive(false);
        feedbackRoutine = null;
    }

    private void ClearFeedback()
    {
        if (feedbackText == null) return;

        if (feedbackRoutine != null)
            StopCoroutine(feedbackRoutine);

        feedbackText.text = "";
        feedbackText.gameObject.SetActive(false);
        feedbackRoutine = null;
    }

    private NPCDialogue FindPresentTarget()
    {
        Transform cam = playerCamera != null
            ? playerCamera
            : Camera.main != null ? Camera.main.transform : null;

        if (cam == null) return null;

        Collider[] hits = Physics.OverlapSphere(
            cam.position,
            presentRange,
            npcLayerMask,
            QueryTriggerInteraction.Collide);

        NPCDialogue best = null;
        float bestDot = -1f;

        foreach (var h in hits)
        {
            NPCDialogue npc = h.GetComponentInParent<NPCDialogue>();
            if (npc == null) continue;

            Vector3 toNpc = npc.transform.position - cam.position;
            toNpc.y = 0;
            Vector3 forward = cam.forward;
            forward.y = 0;

            float dot = Vector3.Dot(forward.normalized, toNpc.normalized);
            if (dot >= facingDot && dot > bestDot)
            {
                bestDot = dot;
                best = npc;
            }
        }

        return best;
    }
}
