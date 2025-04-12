using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    [Header("Placement Attributes")]
    [SerializeField] private LayerMask layerToPlaceObjects = 6 ; // Ground Layer
    [SerializeField] private Transform objectToPlace;
    [SerializeField] private Vector3 horizontalRotationOffset = new Vector3(0, 30, 0);
    [SerializeField] private Vector3 verticalRotationOffset = new Vector3(0, 0, 30);
    [SerializeField] private Transform rayIndicator;
    [SerializeField] private Transform placedObjectParent;
    [SerializeField] private GridData gridData;
    [SerializeField] private ObjectPreview objectPreview;
    [SerializeField] private FloorPlacer floorPlacer;

    public Grid grid;
    public PlaceableObjectData objectPool;
    [Range(5, 30)] public float maxObjectPlacementDistance = 10;
    [HideInInspector] public bool hasCancelledPlacement = true;
    [HideInInspector] public int selectedObjectIndex;
    [HideInInspector] public PlaceablePrefabs currentSelectedObjectData;
    [HideInInspector] public Rotation ObjectRotationType = Rotation.Horizontal;
    [HideInInspector] public Vector3 wallPosOffset1;
    [HideInInspector] public Vector3 wallPosOffset2;
    [HideInInspector] public bool isRayhitFloor = false;
    [HideInInspector] public ObjectsOnFloorPlacement.Edge currentFloorEdge;
    [HideInInspector] public Vector3 currentPlacingPos;

    private Camera mainCamera;
    private Transform placeableObject;
    private Transform placeableObjectPreview;
    private float halfHeight;
    private Vector3 wallPos;
    private ObjectsOnFloorPlacement currentFloorData;

    public enum Rotation
    {
        Horizontal,
        Vertical
    }

    private void Start()
    {
        mainCamera = Camera.main;

        placeableObject = objectPool.GetCurrentPrefab(PrefabTypes.Wall).objectPrefab;
        currentSelectedObjectData = objectPool.GetCurrentPrefab(PrefabTypes.Wall);
    }

    private void Update()
    {
        SetPlaceableObject();
        PlacementHandler();
    }

    void PlacementHandler()
    {
        Vector3 mainCameraPos = mainCamera.transform.position;
        Vector3 mainCameraForward = mainCamera.transform.forward;
        Ray ray = new Ray(mainCameraPos, mainCameraForward);

        RaycastHit hitInfo;
        bool isRayHitSurface = Physics.Raycast(ray, out hitInfo, maxObjectPlacementDistance, layerToPlaceObjects);

        if (!isRayHitSurface)
        {
            rayIndicator.gameObject.SetActive(false);
            objectPreview.PreviewObjectState(false);

            return;
        }

        Vector3 hitPos = hitInfo.point;
        Vector3 hitNorm = hitInfo.normal;
        Vector3 gridCellToWorldPos = GridToWorldPos(hitPos);
        Vector3 currentHitPos = Vector3.zero;

        hasCancelledPlacement = Input.GetMouseButtonDown(0) ? true : hasCancelledPlacement;

        if (Input.GetMouseButton(0))
        {
            SetFloorEdge(hitInfo);
            RotatePlacingObject();

            if (Input.GetMouseButtonDown(1) && hasCancelledPlacement)
            {
                hasCancelledPlacement = false;
                objectPreview.PreviewObjectState(hasCancelledPlacement);
            }

            objectPreview.SpawnPreviewObject(gridCellToWorldPos, currentHitPos, hitPos, mainCameraPos);
        }

        if (Input.GetMouseButtonUp(0) && hasCancelledPlacement)
        {
            switch (currentSelectedObjectData.PrefabType)
            {
                case PrefabTypes.Wall:
                case PrefabTypes.Window:
                case PrefabTypes.Door1:
                    PlaceObjectOnFloor();
                    break;
                case PrefabTypes.Floor:
                    //PlaceFloor(hitPos, gridCellToWorldPos);
                    objectPreview.PreviewObjectState(false);
                    floorPlacer.PlaceFloor(hitPos, currentSelectedObjectData.objectPrefab);
                    break;
            }
        }
    }

    void RotatePlacingObject()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (currentSelectedObjectData.PrefabType == PrefabTypes.Floor) return;

            float yRotation = objectPreview.previewRotation.y;
            Vector3 rotationOffset = Vector3.zero;
            bool isHorizontal = Mathf.Approximately(yRotation, 0);

            rotationOffset = isHorizontal ? new Vector3(0, 90, 0) : new Vector3(0, 0, 0);
            ObjectRotationType = isHorizontal ? Rotation.Vertical : Rotation.Horizontal;

            objectPreview.previewRotation = rotationOffset;
            Debug.Log(ObjectRotationType.ToString() + "Rotation Type");
        }
    }

    void PlaceFloor(Vector3 _hitPos, Vector3 _gridCellToWorldPos)
    {
        Vector3Int cellPos = grid.WorldToCell(_gridCellToWorldPos);
        Vector3 cellCenterWorld = grid.GetCellCenterWorld(cellPos);
        bool canPlaceObject = gridData.CanPlaceObject(cellPos, currentSelectedObjectData.size, ObjectRotationType);

        objectPreview.PreviewObjectState(false);
        TryPlaceObject(canPlaceObject, _hitPos);
    }

    void TryPlaceObject(bool _canPlaceObject, Vector3 _hitPos)
    {
        if (_canPlaceObject)
        {
            PlaceObject(placeableObject, _hitPos, Quaternion.identity, placedObjectParent);
            Debug.Log("Placed object");
        }
        else
        {
            Debug.Log("Position occupied OR Object Type is not Floor");
        }
    }

    void PlaceObjectOnFloor()
    {
        objectPreview.PreviewObjectState(false);

        if (currentFloorData == null) return;

        if (!currentFloorData.isEdgePlaceable(currentFloorEdge))
        {
            Debug.Log("Edge already occupied");
            return;
        }

        currentFloorData.SetFloorEdge(
            currentFloorEdge, 
            currentSelectedObjectData.objectPrefab, 
            currentPlacingPos, 
            Quaternion.Euler(objectPreview.previewRotation));

        Debug.Log(currentFloorEdge.ToString());
    }

    void SetFloorEdge(RaycastHit _hitInfo)
    {
        //if (currentSelectedObjectData.PrefabType == PrefabTypes.Floor)
        //{
        //    wallPosOffset1 = _hitInfo.transform.position + new Vector3Int(3, 0, 0);
        //    return;
        //}

        Transform hitTransform = _hitInfo.transform;

        isRayhitFloor = hitTransform.tag == "Floor";


        if (currentSelectedObjectData.PrefabType == PrefabTypes.Floor)
        {
            Vector3 floorPos = hitTransform.position;

            wallPosOffset1 = isRayhitFloor ? floorPos + new Vector3Int(3, 0, 0) : _hitInfo.point;
            return;
        }
        
        if (!isRayhitFloor) return;


        Vector3Int offset1, offset2;
        currentFloorData = hitTransform.GetComponent<ObjectsOnFloorPlacement>();
        currentFloorEdge = new ObjectsOnFloorPlacement.Edge();

        switch (ObjectRotationType)
        {
            case Rotation.Horizontal:
                offset1 = new Vector3Int(0, 0, 0);
                offset2 = new Vector3Int(0, 0, 3);
                break;
            case Rotation.Vertical:
                offset1 = new Vector3Int(0, 0, 3);
                offset2 = new Vector3Int(3, 0, 3);
                break;
            default:
                offset1 = offset2 = Vector3Int.zero;
                break;
        }

        wallPosOffset1 = hitTransform.position + offset1;
        wallPosOffset2 = hitTransform.position + offset2;

        //bool isPlaceable = currentFloorData.isEdgePlaceable(currentFloorEdge);
    }

    void PlaceObject(Transform _objectHolder, Vector3 _position, Quaternion _rotation, Transform _parent)
    {
        Vector3Int cellPos = grid.WorldToCell(_position);
        Vector3 cellCenterWorld = grid.GetCellCenterWorld(cellPos);
        
        Instantiate(_objectHolder, _position, _rotation, _parent);
        gridData.AddData(cellPos, _objectHolder.gameObject, currentSelectedObjectData.size, ObjectRotationType);
    }

    Vector3 GridToWorldPos(Vector3 _hitPos)
    {
        Vector3Int cellPos = grid.WorldToCell(_hitPos);
        Vector3 gridToWorldPos = grid.CellToWorld(cellPos);

        return gridToWorldPos;
    }

    void SetPlaceableObject()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0) return;
        
        int listCount = objectPool.placeablePrefabs.Count;
        Transform previewObject = objectPreview.objectPlaceHolder;
        PrefabTypes prefabTypes = PrefabTypes.Wall;

        HidePreviewIfPossible();

        selectedObjectIndex = (scroll > 0) ? (selectedObjectIndex - 1 + listCount) % listCount : (selectedObjectIndex + 1) % listCount;
        placeableObject = objectPreview.SetPreviewObject(listCount, prefabTypes, placeableObject);
    }

    void HidePreviewIfPossible()
    {
        if (objectPreview.objectPlaceHolder.gameObject.activeSelf)
        {
            objectPreview.PreviewObjectState(false);
        }
    }
}
