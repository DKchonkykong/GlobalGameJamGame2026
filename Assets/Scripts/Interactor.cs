using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
    public Camera cam;
    public float range = 3f;
    public LayerMask interactMask;
    public TMP_Text promptText;

    private IInteractable current;

    void Update()
    {
        current = null;
        if (promptText != null) promptText.gameObject.SetActive(false);

        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range, interactMask))
        {
            current = hit.collider.GetComponentInParent<IInteractable>();

            // Don't show prompt while dialogue is open
            if (current != null && (DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen))
            {
                if (promptText != null)
                {
                    promptText.text = current.PromptText;
                    promptText.gameObject.SetActive(true);
                }
            }
        }
    }

    public void OnInteract(InputValue value)
    {
        if (value == null || !value.isPressed) return;

        if (current != null && (DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen))
        {
            current.Interact();
        }
    }
}
