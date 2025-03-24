using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    [Header("Placement Attributes")]
    [SerializeField] private LayerMask layerToPlaceObjects = 6;
    [SerializeField, Range(5, 30)] private float maxObjectPlacementDistance = 10;
    [SerializeField] private Transform objectToPlace;
    [SerializeField] private Transform objectPlaceHolder;
    [SerializeField] private PlaceableObjectData[] objectPool;
    [SerializeField] private Vector3 horizontalRotationOffset = new Vector3(0, 30, 0);
    [SerializeField] private Vector3 verticalRotationOffset = new Vector3(0, 0, 30);

    private Camera mainCamera;
    private bool hasCancelledPlacement = true;
    private Vector3 previewRotation = Vector3.zero;

    Transform wall;
    Transform wallPreveiew;
    float halfHeight;

    private void Start()
    {
        mainCamera = Camera.main;

        wall = objectPool[0].objectPrefab;
        wallPreveiew = objectPool[0].objectPreview;
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

        bool isRayHitSurface = Physics.Raycast(ray, out hitInfo, maxObjectPlacementDistance, layerToPlaceObjects);

        if (isRayHitSurface)
        {
            hitPos = hitInfo.point;
            hitNorm = hitInfo.normal;

            //Debug.DrawRay(mainCameraPos, hitPos);

            if (Input.GetMouseButtonDown(0))
            {
                hasCancelledPlacement = true;
            }

            if (Input.GetMouseButton(0))
            {
                //isCastRay = true;

                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (hasCancelledPlacement)
                    {
                        hasCancelledPlacement = false;
                        PreveiwObjectState(hasCancelledPlacement);
                        Debug.Log("isCastRay : " + hasCancelledPlacement);
                    }
                }

                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    previewRotation = previewRotation + horizontalRotationOffset;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    previewRotation = previewRotation - horizontalRotationOffset;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    previewRotation = previewRotation + verticalRotationOffset;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    previewRotation = previewRotation - verticalRotationOffset;
                }

                MeshRenderer meshRenderer = objectPlaceHolder.GetComponentInChildren<MeshRenderer>();
                float height = meshRenderer.bounds.size.y;
                halfHeight = height / 2;

                if (hasCancelledPlacement)
                {
                    if (currentHitPos != hitPos)
                    {
                        currentHitPos = hitPos;
                        PreviewObjectSetup(currentHitPos + new Vector3(0, halfHeight, 0), objectPlaceHolder, mainCameraPos, previewRotation);
                    }
                    else
                    {
                        PreviewObjectSetup(currentHitPos + new Vector3(0, halfHeight, 0), objectPlaceHolder, mainCameraPos, previewRotation);
                    }
                }
            }
            
            if(Input.GetMouseButtonUp(0))
            {
                if (hasCancelledPlacement == false) return;

                PreveiwObjectState(false);
                Vector3 direction = mainCameraPos - objectPlaceHolder.position;
                direction.y = 0;
                PlaceObject(objectToPlace, hitPos, Quaternion.Euler(previewRotation));

            }
        }
    }

    void PlaceObject(Transform objectHolder, Vector3 position, Quaternion rotation)
    {
        Instantiate(objectToPlace, position + new Vector3(0, halfHeight, 0), rotation);
    }

    void PreveiwObjectState(bool isActive)
    {
        objectPlaceHolder.gameObject.SetActive(isActive);
    }

    void PreviewObjectSetup(Vector3 position, Transform Object, Vector3 cameraPos, Vector3 rotation)
    {
        PreveiwObjectState(true);
        Object.position = position;
        Vector3 direction = cameraPos - Object.position;
        direction.y = 0;
        Object.rotation = Quaternion.Euler(rotation);
    }
}
