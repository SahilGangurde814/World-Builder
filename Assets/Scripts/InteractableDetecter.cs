using UnityEngine;

public class InteractableDetecter : MonoBehaviour
{
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private float maxDistance = 10f;

    private Camera mainCamera;
    Interactable lastInteractable;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        bool isHIt = Physics.Raycast(ray, out RaycastHit hit, maxDistance, interactableLayer);
        Interactable interactable;

        if (isHIt && hit.transform.TryGetComponent(out interactable))
        {
            if(interactable != lastInteractable)
            {
                lastInteractable?.InteractableUiState(false);
                interactable.InteractableUiState(true);
            }

            interactable.Interact();
            lastInteractable = interactable;
        }

        else if(lastInteractable != null)
        {
            lastInteractable.InteractableUiState(false);
            lastInteractable = null;
        }
    }
}
