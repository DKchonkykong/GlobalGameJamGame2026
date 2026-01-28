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

    [Header("Player Control")]
    public MonoBehaviour fpsController;   // FPSController
    public MonoBehaviour interactor;      // Interactor (optional)

    private int index = 0;

    public bool IsOpen => panel != null && panel.activeSelf;

    void Awake()
    {
        // Safe default: keep closed in editor + play
        if (panel != null)
            panel.SetActive(false);
    }

    void OnEnable()
    {
        if (EvidenceManager.Instance != null)
            EvidenceManager.Instance.EvidenceChanged += OnEvidenceChanged;
    }

    void OnDisable()
    {
        if (EvidenceManager.Instance != null)
            EvidenceManager.Instance.EvidenceChanged -= OnEvidenceChanged;
    }

    void Start()
    {
        if (prevButton != null) prevButton.onClick.AddListener(Prev);
        if (nextButton != null) nextButton.onClick.AddListener(Next);
        if (closeButton != null) closeButton.onClick.AddListener(Close);

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

        // Clamp index in case list changed since last open
        index = Mathf.Clamp(index, 0, Mathf.Max(0, EvidenceManager.Instance.Collected.Count - 1));
        Refresh();
    }

    public void Close()
    {
        if (panel == null) return;

        panel.SetActive(false);
        UnlockPlayer();
    }

    void OnEvidenceChanged()
    {
        // If panel is open, update immediately.
        // If panel is closed, no need to do anything (but Refresh is cheap anyway).
        if (IsOpen)
            Refresh();
    }

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
            return;
        }

        // Ensure index valid
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
    }

    void LockPlayer()
    {
        if (fpsController != null) fpsController.enabled = false;
        if (interactor != null) interactor.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void UnlockPlayer()
    {
        if (fpsController != null) fpsController.enabled = true;
        if (interactor != null) interactor.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
