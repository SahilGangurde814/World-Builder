using System;
using System.Collections.Generic;
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
    [SerializeField] private List<PlaceablePrefabs> preveiwObjectsData;
    [SerializeField] private Material invalidPosMaterial;
    [SerializeField] private Material validPosMaterial;
 
    private Camera mainCamera;
    private bool hasCancelledPlacement = true;
    private Vector3 previewRotation = Vector3.zero;
    private int selectedObjectIndex;
    private Transform placeableObject;
    private Transform placeableObjectPreview;
    private float halfHeight;
    private Dictionary<Vector3Int, GameObject> placeObjectsData = new();
    //private PrefabTypes currentSelectedObjectType = PrefabTypes.Wall;

    private void Start()
    {
        mainCamera = Camera.main;

        placeableObject = objectPool.GetCurrentPrefab(PrefabTypes.Wall).objectPrefab;
        //placeableObjectPreview = objectPool.GetCurrentPrefab(PrefabTypes.Wall).objectPreview;
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

                if (Input.GetMouseButtonDown(1) && hasCancelledPlacement)
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

                Vector3Int cellPos = grid.WorldToCell(gridCellToWorldPos);
                if (placeObjectsData.ContainsKey(cellPos))
                {
                    Transform[] materialHoldingObjectsArr = objectPlaceHolder.GetComponent<MaterialData>().materialHoldingObjects;
                    foreach (Transform materialHoldingObject in materialHoldingObjectsArr)
                    {
                        materialHoldingObject.GetComponent<MeshRenderer>().material = invalidPosMaterial;
                    }
                }
                else
                {
                    Transform[] materialHoldingObjectsArr = objectPlaceHolder.GetComponent<MaterialData>().materialHoldingObjects;
                    foreach (Transform materialHoldingObject in materialHoldingObjectsArr)
                    {
                        materialHoldingObject.GetComponent<MeshRenderer>().material = validPosMaterial;
                    }
                }

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
                Vector3Int cellPos = grid.WorldToCell(gridCellToWorldPos);
                if(!placeObjectsData.ContainsKey(cellPos))
                {
                    PlaceObject(placeableObject, gridCellToWorldPos, Quaternion.Euler(previewRotation), placedObjectParent);
                }
                else
                {
                    Debug.Log("Position occupied");
                }

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
        Vector3Int cellPos = grid.WorldToCell(position);
        placeObjectsData.Add(cellPos, objectHolder.gameObject);
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
        PrefabTypes prefabTypes = PrefabTypes.Wall;

        if (scroll != 0)
        {
            if(scroll > 0)
            {
                if (objectPlaceHolder.gameObject.activeSelf)
                {
                    PreveiwObjectState(false);
                }
                selectedObjectIndex = (selectedObjectIndex - 1 + listCount) % listCount;
                prefabTypes = objectPool.placeablePrefabs[selectedObjectIndex].PrefabType;
                PlaceablePrefabs placeablePrefabs = objectPool.GetCurrentPrefab(prefabTypes);
                
                selectedObject = placeablePrefabs.objectPrefab;
                //previewObject = placeablePrefabs.objectPreview;
                objectPlaceHolder = preveiwObjectsData.Find((x) => x.PrefabType == prefabTypes).objectPrefab;

            }
            else
            {
                if (objectPlaceHolder.gameObject.activeSelf)
                {
                    PreveiwObjectState(false);
                }
                selectedObjectIndex = (selectedObjectIndex + 1) % listCount;
                prefabTypes = objectPool.placeablePrefabs[selectedObjectIndex].PrefabType;
                PlaceablePrefabs placeablePrefabs = objectPool.GetCurrentPrefab(prefabTypes);

                selectedObject = placeablePrefabs.objectPrefab;
                //previewObject = placeablePrefabs.objectPreview;
                objectPlaceHolder = preveiwObjectsData.Find((x) => x.PrefabType == prefabTypes).objectPrefab;
            }
        }

        placeableObject = selectedObject;
        //objectPlaceHolder = previewObject;
        //Debug.Log("Object Holder : " + objectPlaceHolder.name);
    }
}
