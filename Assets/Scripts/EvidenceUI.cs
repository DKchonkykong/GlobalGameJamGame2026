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
    public LayerMask presentMask = ~0; // everything
    public bool requireFacing = true;  // must be looking at them

    [Header("Player Control")]
    public GameObject playerObject; // your FPS controller root, or movement script holder

    private Coroutine feedbackRoutine;
    private IEvidenceReceiver cachedReceiver; // who we present to (optional cache)

    private void Awake()
    {
        if (panel != null) panel.SetActive(false);

        if (prevButton != null) prevButton.onClick.AddListener(PrevEvidence);
        if (nextButton != null) nextButton.onClick.AddListener(NextEvidence);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (presentButton != null) presentButton.onClick.AddListener(OnPresentPressed);

        if (EvidenceManager.Instance != null)
            EvidenceManager.Instance.OnEvidenceChanged += Refresh;
    }

    private void OnDestroy()
    {
        if (EvidenceManager.Instance != null)
            EvidenceManager.Instance.OnEvidenceChanged -= Refresh;
    }

    public void Toggle()
    {
        if (panel == null) return;

        bool newState = !panel.activeSelf;
        panel.SetActive(newState);

        if (playerObject != null)
            playerObject.SetActive(!newState); // disables movement; if you prefer, disable only your controller script instead

        Cursor.lockState = newState ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = newState;

        if (newState)
        {
            // When opening, refresh UI + find a target in front of you
            Refresh();
            cachedReceiver = FindReceiverInFront();
        }
        else
        {
            cachedReceiver = null;
        }
    }

    public void Close()
    {
        if (panel == null) return;
        if (!panel.activeSelf) return
