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
    public MonoBehaviour fpsController;   // drag your movement script
    public MonoBehaviour interactor;      // drag Interactor (optional)

    private int index = 0;

    public bool IsOpen => panel != null && panel.activeSelf;

    void Start()
    {
        panel.SetActive(false);

        prevButton.onClick.AddListener(Prev);
        nextButton.onClick.AddListener(Next);
        closeButton.onClick.AddListener(Close);

        if (EvidenceManager.Instance != null)
            EvidenceManager.Instance.OnEvidenceChanged += HandleEvidenceChanged;
    }

    void OnDestroy()
    {
        if (EvidenceManager.Instance != null)
            EvidenceManager.Instance.OnEvidenceChanged -= HandleEvidenceChanged;
    }

    private void HandleEvidenceChanged()
    {
        if (!IsOpen) return;

        // keep index valid when items added/removed
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
        panel.SetActive(true);
        LockPlayer();

        index = 0;
        Refresh();
    }

    public void Close()
    {
        panel.SetActive(false);
        UnlockPlayer();
    }

    // Public wrappers for router/keyboard
    public void NextItem() => Next();
    public void PrevItem() => Prev();

    void Prev()
    {
        var list = EvidenceManager.Instance.Collected;
        if (list.Count == 0) return;

        index = (index - 1 + list.Count) % list.Count;
        Refresh();
    }

    void Next()
    {
        var list = EvidenceManager.Instance.Collected;
        if (list.Count == 0) return;

        index = (index + 1) % list.Count;
        Refresh();
    }

    void Refresh()
    {
        var list = EvidenceManager.Instance.Collected;

        if (list.Count == 0)
        {
            iconImage.enabled = false;
            titleText.text = "No evidence";
            descriptionText.text = "You haven't collected any evidence yet.";
            counterText.text = "0/0";
            return;
        }

        // Safety clamp
        index = Mathf.Clamp(index, 0, list.Count - 1);

        iconImage.enabled = true;

        EvidenceItem item = list[index];
        iconImage.sprite = item.icon;
        titleText.text = item.displayName;
        descriptionText.text = item.description;
        counterText.text = $"{index + 1}/{list.Count}";
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
