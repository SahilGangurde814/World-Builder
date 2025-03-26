using System;
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
    [SerializeField] private PlaceableObjectData objectPool;
    [SerializeField] private Vector3 horizontalRotationOffset = new Vector3(0, 30, 0);
    [SerializeField] private Vector3 verticalRotationOffset = new Vector3(0, 0, 30);
    [SerializeField] private Grid grid;
    [SerializeField] private Transform rayIndicator;
    [SerializeField] private Transform placedObjectParent;
    [SerializeField] private Transform preveiwObjectsParent;
 
    private Camera mainCamera;
    private bool hasCancelledPlacement = true;
    private Vector3 previewRotation = Vector3.zero;
    private int selectedObjectIndex;
    private Transform placeableObject;
    private Transform placeableObjectPreview;
    private float halfHeight;

    private void Start()
    {
        mainCamera = Camera.main;

        placeableObject = objectPool.GetCurrentPrefab(PrefabTypes.Wall).objectPrefab;
        placeableObjectPreview = objectPool.GetCurrentPrefab(PrefabTypes.Wall).objectPreview;
    }

    private void Update()
    {
        SetPlaceableObject();
        PlaceObject();
    }

    void PlaceObject()
    {
        Vector3 mainCameraPos = mainCamera.transform.position;
        Vector3 mainCameraForward = mainCamera.transform.forward;

        Ray ray = new Ray(mainCameraPos, mainCameraForward);
        RaycastHit hitInfo;

        Vector3 hitPos;
        Vector3 hitNorm;
        Vector3 currentHitPos = Vector3.zero;
        Vector3 gridCellToWorldPos;

        bool isRayHitSurface = Physics.Raycast(ray, out hitInfo, maxObjectPlacementDistance, layerToPlaceObjects);

        if (isRayHitSurface)
        {
            hitPos = hitInfo.point;
            hitNorm = hitInfo.normal;


            gridCellToWorldPos = GridToWorldPos(hitPos);

            //Debug.DrawRay(mainCameraPos, hitPos);

            if (Input.GetMouseButtonDown(0))
            {
                hasCancelledPlacement = true;
            }

            if (Input.GetMouseButton(0))
            {
                //isCastRay = true;

                if (Input.GetKeyDown(KeyCode.E) && hasCancelledPlacement)
                {
                    hasCancelledPlacement = false;
                    PreveiwObjectState(hasCancelledPlacement);
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

                //// if object's pivot is in center use this
                //MeshRenderer meshRenderer = objectPlaceHolder.GetComponentInChildren<MeshRenderer>();
                //float height = meshRenderer.bounds.size.y;
                //halfHeight = height / 2;


                if (hasCancelledPlacement)
                {
                    if (currentHitPos != hitPos)
                    {
                        currentHitPos = hitPos;
                        PreviewObjectSetup(gridCellToWorldPos /*+ new Vector3(0, halfHeight, 0)*/, objectPlaceHolder, mainCameraPos, previewRotation);
                    }
                    else
                    {
                        PreviewObjectSetup(gridCellToWorldPos /*+ new Vector3(0, halfHeight, 0)*/, objectPlaceHolder, mainCameraPos, previewRotation);
                    }
                }
            }
            
            if(Input.GetMouseButtonUp(0))
            {
                if (hasCancelledPlacement == false) return;

                PreveiwObjectState(false);
                Vector3 direction = mainCameraPos - objectPlaceHolder.position;
                direction.y = 0;
                PlaceObject(placeableObject, gridCellToWorldPos, Quaternion.Euler(previewRotation), placedObjectParent);

            }
        }
        else
        {
            rayIndicator.gameObject.SetActive(false);
            PreveiwObjectState(false);
        }
    }

    void PlaceObject(Transform objectHolder, Vector3 position, Quaternion rotation, Transform parent)
    {
        Instantiate(objectHolder, position /*+ new Vector3(0, halfHeight, 0)*/, rotation, parent);
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

    Vector3 GridToWorldPos(Vector3 hitPos)
    {
        Vector3Int cellPos = grid.WorldToCell(hitPos);
        rayIndicator.gameObject.SetActive(true);
        rayIndicator.transform.position = grid.CellToWorld(cellPos);
        Vector3 gridToWorldPos = grid.CellToWorld(cellPos);

        return gridToWorldPos;
    }

    void SetPlaceableObject()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        int listCount = objectPool.placeablePrefabs.Count;
        Transform selectedObject = placeableObject;
        Transform previewObject = objectPlaceHolder;

        if (scroll != 0)
        {
            if(scroll > 0)
            {
                selectedObjectIndex = (selectedObjectIndex - 1 + listCount) % listCount;
                PrefabTypes prefabTypes = objectPool.placeablePrefabs[selectedObjectIndex].PrefabType;
                PlaceablePrefabs placeablePrefabs = objectPool.GetCurrentPrefab(prefabTypes);
                
                selectedObject = placeablePrefabs.objectPrefab;
                previewObject = placeablePrefabs.objectPreview;

            }
            else
            {
                selectedObjectIndex = (selectedObjectIndex + 1) % listCount;
                PrefabTypes prefabTypes = objectPool.placeablePrefabs[selectedObjectIndex].PrefabType;
                PlaceablePrefabs placeablePrefabs = objectPool.GetCurrentPrefab(prefabTypes);

                selectedObject = placeablePrefabs.objectPrefab;
                previewObject = placeablePrefabs.objectPreview;
            }
        }

        placeableObject = selectedObject;
        objectPlaceHolder = previewObject;
    }
}
