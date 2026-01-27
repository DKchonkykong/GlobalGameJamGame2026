using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI")]
    public GameObject panel;
    public TMP_Text speakerText;
    public TMP_Text bodyText;
    public Button continueButton;

    public bool IsOpen => panel.activeSelf;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        panel.SetActive(false);
        continueButton.onClick.AddListener(Hide);
    }

    public void ShowMessage(string speaker, string message)
    {
        speakerText.text = speaker;
        bodyText.text = message;
        panel.SetActive(true);

        // optional: stop player look/move during dialogue
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    public void Hide()
    {
        panel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
