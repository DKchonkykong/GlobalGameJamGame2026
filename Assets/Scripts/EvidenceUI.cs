using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EvidenceUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;
    public Image iconImage;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public TMP_Text counterText;
    public Button prevButton;
    public Button nextButton;
    public Button closeButton;
    public Button presentButton;

    [Header("Feedback")]
    public TMP_Text feedbackText;
    public float feedbackDuration = 2f;

    [Header("Player Control")]
    public GameObject playerObject; // Just reference the player GameObject

    private int index = 0;
    private float feedbackTimer = 0f;

    public bool IsOpen => panel != null && panel.activeSelf;

    void Start()
    {
        if (panel != null)
            panel.SetActive(false);

        if (prevButton != null) prevButton.onClick.AddListener(Prev);
        if (nextButton != null) nextButton.onClick.AddListener(Next);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (presentButton != null) presentButton.onClick.AddListener(OnPresentPressed);

        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);

        if (EvidenceManager.Instance != null)
            EvidenceManager.Instance.OnEvidenceChanged += HandleEvidenceChanged;
    }

    void OnDestroy()
    {
        if (EvidenceManager.Instance != null)
            EvidenceManager.Instance.OnEvidenceChanged -= HandleEvidenceChanged;
    }

    void Update()
    {
        if (feedbackTimer > 0f)
        {
            feedbackTimer -= Time.deltaTime;
            if (feedbackTimer <= 0f && feedbackText != null)
            {
                feedbackText.gameObject.SetActive(false);
            }
        }
    }

    private void HandleEvidenceChanged()
    {
        if (!IsOpen) return;

        var count = EvidenceManager.Instance.Collected.Count;
        if (count == 0) index = 0;
        else index = Mathf.Clamp(index, 0, count - 1);

        Refresh();
    }

    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    public void Open()
    {
        if (panel == null) return;

        panel.SetActive(true);
        LockPlayer();

        index = 0;
        Refresh();
    }

    public void Close()
    {
        if (panel == null) return;

        panel.SetActive(false);
        UnlockPlayer();
    }

    // Public wrappers for router/keyboard
    public void NextItem() => Next();
    public void PrevItem() => Prev();

    void Prev()
    {
        var list = EvidenceManager.Instance?.Collected;
        if (list == null || list.Count == 0) return;

        index = (index - 1 + list.Count) % list.Count;
        Refresh();
    }

    void Next()
    {
        var list = EvidenceManager.Instance?.Collected;
        if (list == null || list.Count == 0) return;

        index = (index + 1) % list.Count;
        Refresh();
    }

    void Refresh()
    {
        var list = EvidenceManager.Instance?.Collected;

        if (list == null || list.Count == 0)
        {
            if (iconImage != null) iconImage.enabled = false;
            if (titleText != null) titleText.text = "No evidence";
            if (descriptionText != null) descriptionText.text = "You haven't collected any evidence yet.";
            if (counterText != null) counterText.text = "0/0";
            
            // Disable present button if no evidence
            if (presentButton != null) presentButton.interactable = false;
            return;
        }

        index = Mathf.Clamp(index, 0, list.Count - 1);

        EvidenceItem item = list[index];

        if (iconImage != null)
        {
            iconImage.enabled = item.icon != null;
            iconImage.sprite = item.icon;
        }

        if (titleText != null) titleText.text = item.displayName;
        if (descriptionText != null) descriptionText.text = item.description;
        if (counterText != null) counterText.text = $"{index + 1}/{list.Count}";
        
        // Enable present button only if someone can receive evidence
        if (presentButton != null)
        {
            bool canPresent = ConversationContext.Instance != null && 
                             ConversationContext.Instance.ActiveReceiver != null;
            presentButton.interactable = canPresent;
        }
    }

    public void OnPresentPressed()
    {
        var list = EvidenceManager.Instance?.Collected;
        if (list == null || list.Count == 0)
        {
            ShowFeedback("No evidence to present.");
            return;
        }

        // Check if there's someone to present to
        IEvidenceReceiver receiver = null;
        if (ConversationContext.Instance != null)
        {
            receiver = ConversationContext.Instance.ActiveReceiver;
        }

        if (receiver == null)
        {
            ShowFeedback("No one is here to show this to.");
            return;
        }

        // Get current evidence item
        EvidenceItem currentItem = list[index];
        
        Debug.Log($"Presenting {currentItem.displayName} to NPC");

        // Present it
        receiver.ReceiveEvidence(currentItem);
        
        // Close the evidence UI after presenting
        Close();
    }

    void ShowFeedback(string message)
    {
        if (feedbackText == null)
        {
            Debug.Log(message);
            return;
        }

        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);
        feedbackTimer = feedbackDuration;
    }

    void LockPlayer()
    {
        if (playerObject != null)
        {
            // Disable FPSController
            var fpsController = playerObject.GetComponent<FPSController>();
            if (fpsController != null) fpsController.enabled = false;

            // Disable Interactor
            var interactor = playerObject.GetComponent<Interactor>();
            if (interactor != null) interactor.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void UnlockPlayer()
    {
        if (playerObject != null)
        {
            // Enable FPSController
            var fpsController = playerObject.GetComponent<FPSController>();
            if (fpsController != null) fpsController.enabled = true;

            // Enable Interactor
            var interactor = playerObject.GetComponent<Interactor>();
            if (interactor != null) interactor.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
