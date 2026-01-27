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
    public MonoBehaviour fpsController;   // drag FPSController
    public MonoBehaviour interactor;      // drag Interactor (optional)

    private int index = 0;

    public bool IsOpen => panel.activeSelf;

    void Start()
    {
        panel.SetActive(false);

        prevButton.onClick.AddListener(Prev);
        nextButton.onClick.AddListener(Next);
        closeButton.onClick.AddListener(Close);
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
