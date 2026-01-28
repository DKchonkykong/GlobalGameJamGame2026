using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EvidenceToastUI : MonoBehaviour
{
    public GameObject panel;
    public Image icon;
    public TMP_Text title;
    public TMP_Text description;

    [Range(0.5f, 5f)] public float showSeconds = 2f;

    Coroutine routine;

    void Start()
    {
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

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine(item));
    }

    IEnumerator ShowRoutine(EvidenceItem item)
    {
        icon.sprite = item.icon;
        icon.enabled = item.icon != null;

        title.text = $"Evidence Added: {item.displayName}";
        description.text = item.description;

        panel.SetActive(true);
        yield return new WaitForSeconds(showSeconds);
        panel.SetActive(false);
    }
}
