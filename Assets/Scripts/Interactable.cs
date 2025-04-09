using TMPro;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] private Transform door;
    [SerializeField] private Transform interactabelUIObject;
    [SerializeField] private float rotationSpeed = 0.5f;

    private DoorState doorState = DoorState.Close;

    public enum DoorState
    {
        Close,
        Open
    }

    public void Interact()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            OpenCloseDoor();
        }
    }

    public void InteractableUiState(bool isActive)
    {
        if (interactabelUIObject != null)
        {
            interactabelUIObject?.gameObject.SetActive(isActive);
        }
    }

    void OpenCloseDoor()
    {
        Quaternion startRotation = door.rotation;
        Quaternion resultRotation = Quaternion.Euler(0, 0, 0);

        switch (doorState)
        {
            case DoorState.Close:
                resultRotation = Quaternion.Euler(0, 90, 0);
                doorState = DoorState.Open;
                break;

            case DoorState.Open:
                resultRotation = Quaternion.Euler(0, 0, 0);
                doorState = DoorState.Close;
                break;
        }

        door.transform.rotation = resultRotation;
    }
}
