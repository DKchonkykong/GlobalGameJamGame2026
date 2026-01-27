using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

//this is now using the new input system furthermore it now advances multiple lines of dialogue instead of one
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI")]
    public GameObject panel;
    public TMP_Text speakerText;
    public TMP_Text bodyText;
    public Button continueButton;

    [Header("Player Control")]
    public MonoBehaviour fpsController;   // drag FPSController here
    public MonoBehaviour interactor;      // optional: drag Interactor here

    private Queue<string> lines = new Queue<string>();

    public bool IsOpen => panel.activeSelf;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        panel.SetActive(false);
        continueButton.onClick.AddListener(NextLine);
    }
    void Update()
    {
        if (!IsOpen) return;

        bool advance =
            (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) ||
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

        if (advance)
            NextLine();
    }

    public void ShowDialogue(string speaker, string[] dialogueLines)
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
            return;

        speakerText.text = speaker;
        lines.Clear();

        foreach (string line in dialogueLines)
            lines.Enqueue(line);

        panel.SetActive(true);
        LockPlayer();
        NextLine();
    }

    void NextLine()
    {
        if (lines.Count == 0)
        {
            CloseDialogue();
            return;
        }

        bodyText.text = lines.Dequeue();
    }

    void CloseDialogue()
    {
        panel.SetActive(false);
        UnlockPlayer();
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
