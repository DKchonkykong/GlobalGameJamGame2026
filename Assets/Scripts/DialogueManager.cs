using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI")]
    public GameObject panel;
    public TMP_Text speakerText;
    public TMP_Text bodyText;
    public Button continueButton;

    [Header("Player Control")]
    public GameObject playerObject; // Changed to GameObject reference

    private readonly Queue<DialogueLine> lines = new();
    private DialogueActor currentActor;

    public bool IsOpen => panel.activeSelf;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
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

        if (advance) NextLine();
    }

    // New method for DialogueActor
    public void StartConversation(DialogueActor actor)
    {
        if (actor == null) return;

        currentActor = actor;
        DialogueNode node = actor.GetCurrentNode();

        if (node != null && node.lines != null && node.lines.Length > 0)
        {
            ShowDialogue(node.lines);

            // Set up evidence receiver if this node has evidence requirements
            if (node.requiredEvidence != null && ConversationContext.Instance != null)
            {
                ConversationContext.Instance.SetActiveReceiver(actor);
            }
        }
    }

    // Overload for simple string array dialogue (used by SimpleInteractable and EvidencePickup)
    public void ShowDialogue(string speaker, string[] dialogueLines)
    {
        if (dialogueLines == null || dialogueLines.Length == 0) return;

        currentActor = null;
        lines.Clear();

        foreach (string text in dialogueLines)
        {
            lines.Enqueue(new DialogueLine { speaker = speaker, text = text });
        }

        panel.SetActive(true);
        LockPlayer();
        NextLine();
    }

    // Main method for DialogueLine array
    public void ShowDialogue(DialogueLine[] dialogueLines)
    {
        if (dialogueLines == null || dialogueLines.Length == 0) return;

        lines.Clear();
        foreach (var line in dialogueLines) lines.Enqueue(line);

        panel.SetActive(true);
        LockPlayer();
        NextLine();
    }

    void NextLine()
    {
        if (lines.Count == 0) { CloseDialogue(); return; }

        var line = lines.Dequeue();
        speakerText.text = line.speaker;
        bodyText.text = line.text;
    }

    void CloseDialogue()
    {
        panel.SetActive(false);

        // Clear conversation context when dialogue ends
        if (ConversationContext.Instance != null)
        {
            ConversationContext.Instance.Clear();
        }

        currentActor = null;
        UnlockPlayer();
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
}