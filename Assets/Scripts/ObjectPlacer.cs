using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    [Header("Placement Attributes")]
    [SerializeField] private LayerMask layerToPlaceObjects = 6;
    [SerializeField, Range(5, 30)] private float maxObjectPlacementDistance = 10;
    [SerializeField] private Transform objectToPlace;
    [SerializeField] private Transform objectPlaceHolder;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        SurfaceNormalChecker();
    }

    void SurfaceNormalChecker()
    {
        Vector3 mainCameraPos = mainCamera.transform.position;
        Vector3 mainCameraForward = mainCamera.transform.forward;

        Ray ray = new Ray(mainCameraPos, mainCameraForward);
        RaycastHit hitInfo;

        Vector3 hitPos;
        Vector3 hitNorm;
        Vector3 currentHitPos = Vector3.zero;

        if(Physics.Raycast(ray, out hitInfo, maxObjectPlacementDistance, layerToPlaceObjects))
        {
            hitPos = hitInfo.point;
            hitNorm = hitInfo.normal;
            Quaternion rotation = Quaternion.identity;

            Debug.DrawRay(mainCameraPos, hitPos);

            if (Input.GetMouseButton(0))
            {
                if(currentHitPos != hitPos)
                {
                    currentHitPos = hitPos;

                    ObjectHolderState(true);
                    objectPlaceHolder.position = currentHitPos;
                }
                else
                {
                    ObjectHolderState(true);
                    objectPlaceHolder.position = currentHitPos;
                }
            }
            else if(Input.GetMouseButtonUp(0))
            {
                ObjectHolderState(false);
                PlaceObject(objectToPlace, hitPos, rotation);
            }
        }
    }

    void PlaceObject(Transform objectHolder, Vector3 position, Quaternion rotation)
    {
        Instantiate(objectToPlace, position, rotation);
    }

    void ObjectHolderState(bool isActive)
    {
        objectPlaceHolder.gameObject.SetActive(isActive);
    }
}
