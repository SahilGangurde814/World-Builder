using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        Vector3 lookVector = transform.position - mainCamera.transform.position;
        transform.rotation = Quaternion.LookRotation(lookVector);
        //transform.LookAt(mainCamera.transform.position);
    }
}
