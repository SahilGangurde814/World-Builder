using TMPro;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] private Transform door;
    [SerializeField] private Transform interactabelUIObject;

    public void Interact()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Interact");
            door.transform.rotation = Quaternion.Euler(0, door.transform.rotation.y + -90, 0);
        }
    }

    public void InteractableUiState(bool isActive)
    {
        interactabelUIObject?.gameObject.SetActive(isActive);
    }
}
