using System.Collections;
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

    [Header("Present")]
    public Button presentButton;
    public TMP_Text feedbackText;
    public float feedbackDuration = 2f;

    [Header("Present Targeting")]
    public Camera playerCamera;
    public float presentRange = 3.0f;
    public LayerMask presentMask = ~0;
    public bool requireFacing = true;

    [Header("Player Control")]
    public GameObject playerObject;

    private int currentIndex = 0;
    private Coroutine feedbackRoutine;
    private IEvidenceReceiver cachedReceiver;

    public bool IsOpen => panel != null && panel.activeSelf;

    void Awake()
    {
        if (panel != null)
            panel.SetActive(false);

        if (prevButton != null) prevButton.onClick.AddListener(PrevEvidence);
        if (nextButton != null) nextButton.onClick.AddListener(NextEvidence);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (presentButton != null) presentButton.onClick.AddListener(OnPresentPressed);

        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);

        if (EvidenceManager.Instance != null)
            EvidenceManager.Instance.OnEvidenceChanged += Refresh;
    }

    void OnDestroy()
    {
        if (EvidenceManager.Instance != null)
            EvidenceManager.Instance.OnEvidenceChanged -= Refresh;
    }

    public void Toggle()
    {
        if (panel == null) return;

        if (IsOpen)
            Close();
        else
            Open();
    }

    public void Open()
    {
        if (panel == null) return;

        panel.SetActive(true);
        LockPlayer();

        currentIndex = 0;
        Refresh();

        // Find potential evidence receiver when opening
        cachedReceiver = FindReceiverInFront();
    }

    public void Close()
    {
        if (panel == null) return;
        if (!IsOpen) return;

        panel.SetActive(false);
        UnlockPlayer();
        cachedReceiver = null;
    }

    void LockPlayer()
    {
        if (playerObject != null)
        {
            var fpsController = playerObject.GetComponent<FPSController>();
            if (fpsController != null) fpsController.enabled = false;

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
            var fpsController = playerObject.GetComponent<FPSController>();
            if (fpsController != null) fpsController.enabled = true;

            var interactor = playerObject.GetComponent<Interactor>();
            if (interactor != null) interactor.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Made public so UIInputRouter can call them
    public void PrevEvidence()
    {
        var list = EvidenceManager.Instance?.Evidence;
        if (list == null || list.Count == 0) return;

        currentIndex = (currentIndex - 1 + list.Count) % list.Count;
        Refresh();
    }

    // Made public so UIInputRouter can call them
    public void NextEvidence()
    {
        var list = EvidenceManager.Instance?.Evidence;
        if (list == null || list.Count == 0) return;

        currentIndex = (currentIndex + 1) % list.Count;
        Refresh();
    }

    void Refresh()
    {
        var list = EvidenceManager.Instance?.Evidence;

        if (list == null || list.Count == 0)
        {
            if (iconImage != null) iconImage.enabled = false;
            if (titleText != null) titleText.text = "No evidence";
            if (descriptionText != null) descriptionText.text = "You haven't collected any evidence yet.";
            if (counterText != null) counterText.text = "0/0";
            if (presentButton != null) presentButton.interactable = false;
            return;
        }

        currentIndex = Mathf.Clamp(currentIndex, 0, list.Count - 1);
        EvidenceItem item = list[currentIndex];

        if (iconImage != null)
        {
            iconImage.enabled = item.icon != null;
            iconImage.sprite = item.icon;
        }

        if (titleText != null) titleText.text = item.displayName;
        if (descriptionText != null) descriptionText.text = item.description;
        if (counterText != null) counterText.text = $"{currentIndex + 1}/{list.Count}";

        // Enable present button if receiver exists
        if (presentButton != null)
        {
            bool canPresent = cachedReceiver != null || FindReceiverInFront() != null;
            presentButton.interactable = canPresent;
        }
    }

    public void OnPresentPressed()
    {
        var list = EvidenceManager.Instance?.Evidence;
        if (list == null || list.Count == 0)
        {
            ShowFeedback("No evidence to present.");
            return;
        }

        // Try to find receiver
        IEvidenceReceiver receiver = cachedReceiver ?? FindReceiverInFront();

        if (receiver == null)
        {
            ShowFeedback("No one is here to show this to.");
            return;
        }

        EvidenceItem currentItem = list[currentIndex];
        Debug.Log($"Presenting {currentItem.displayName}");

        receiver.ReceiveEvidence(currentItem);
        Close();
    }

    IEvidenceReceiver FindReceiverInFront()
    {
        // First check ConversationContext (if in dialogue)
        if (ConversationContext.Instance != null && ConversationContext.Instance.ActiveReceiver != null)
        {
            return ConversationContext.Instance.ActiveReceiver;
        }

        // Otherwise raycast to find someone in front
        if (playerCamera == null || !requireFacing)
            return null;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, presentRange, presentMask))
        {
            return hit.collider.GetComponentInParent<IEvidenceReceiver>();
        }

        return null;
    }

    void ShowFeedback(string message)
    {
        if (feedbackText == null)
        {
            Debug.Log(message);
            return;
        }

        if (feedbackRoutine != null)
            StopCoroutine(feedbackRoutine);

        feedbackRoutine = StartCoroutine(FeedbackRoutine(message));
    }

    IEnumerator FeedbackRoutine(string message)
    {
        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);
        yield return new WaitForSeconds(feedbackDuration);
        feedbackText.gameObject.SetActive(false);
    }
}
