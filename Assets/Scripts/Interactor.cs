using TMPro;
using UnityEngine;

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
        promptText.gameObject.SetActive(false);

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range, interactMask))
        {
            current = hit.collider.GetComponentInParent<IInteractable>();
            if (current != null && !DialogueManager.Instance.IsOpen)
            {
                promptText.text = current.PromptText;
                promptText.gameObject.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    current.Interact();
                }
            }
        }
    }
}
