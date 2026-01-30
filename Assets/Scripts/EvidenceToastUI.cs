using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EvidenceToastUI : MonoBehaviour
{
    public GameObject panel;
    public Image icon;
    public TMP_Text titleText;

    [Range(0.5f, 5f)] public float showSeconds = 3f;

    private Coroutine routine;

    void Start()
    {
        if (panel != null)
            panel.SetActive(false);

        if (EvidenceManager.Instance != null)
            EvidenceManager.Instance.OnEvidenceAdded += ShowToast;
    }

    void OnDestroy()
    {
        if (EvidenceManager.Instance != null)
            EvidenceManager.Instance.OnEvidenceAdded -= ShowToast;
    }

    void ShowToast(EvidenceItem item)
    {
        if (item == null) return;

        // Don't show toast during dialogue
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen)
            return;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine(item));
    }

    IEnumerator ShowRoutine(EvidenceItem item)
    {
        // Update icon
        if (icon != null)
        {
            icon.sprite = item.icon;
            icon.enabled = item.icon != null;
        }

        // Update text
        if (titleText != null)
        {
            titleText.text = $"Evidence Added: {item.displayName}";
        }

        // Show panel
        if (panel != null)
            panel.SetActive(true);

        yield return new WaitForSeconds(showSeconds);

        // Hide panel
        if (panel != null)
            panel.SetActive(false);
    }
}
