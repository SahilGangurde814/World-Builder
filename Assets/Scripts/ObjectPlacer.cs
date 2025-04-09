using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    [Header("Placement Attributes")]
    [SerializeField] private LayerMask layerToPlaceObjects = 6;
    [SerializeField] private Transform objectToPlace;
    [SerializeField] private Vector3 horizontalRotationOffset = new Vector3(0, 30, 0);
    [SerializeField] private Vector3 verticalRotationOffset = new Vector3(0, 0, 30);
    [SerializeField] private Transform rayIndicator;
    [SerializeField] private Transform placedObjectParent;
    [SerializeField] private GridData gridData;
    [SerializeField] private ObjectPreview objectPreview;

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

            hasCancelledPlacement = Input.GetMouseButtonDown(0) ? true : hasCancelledPlacement;

            if (Input.GetMouseButton(0))
            {
                SetFloorEdge(hitInfo);

                if (Input.GetMouseButtonDown(1) && hasCancelledPlacement)
                {
                    hasCancelledPlacement = false;
                    objectPreview.PreveiwObjectState(hasCancelledPlacement);
                }

                RotatePlacingObject();

                objectPreview.SpawnPreviewObject(gridCellToWorldPos, currentHitPos, hitPos, mainCameraPos);
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (hasCancelledPlacement == false) return;

                switch (currentSelectedObjectData.PrefabType)
                {
                    case PrefabTypes.Wall:
                    case PrefabTypes.Window:
                    case PrefabTypes.Door1:
                        PlaceObjectOnFloor();
                        break;
                    case PrefabTypes.Floor:
                        PlaceFloor(hitPos, gridCellToWorldPos);
                        break;
                }
            }
        }
        else
        {
            rayIndicator.gameObject.SetActive(false);
            objectPreview.PreveiwObjectState(false);
        }
    }

    void RotatePlacingObject()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (currentSelectedObjectData.PrefabType == PrefabTypes.Floor) return;

            if (objectPreview.previewRotation.y == 90)
            {
                objectPreview.previewRotation = new Vector3(0, 0, 0);
                ObjectRotationType = Rotation.Horizontal;
            }
            else
            {
                objectPreview.previewRotation = new Vector3(0, 90, 0);
                ObjectRotationType = Rotation.Vertical;
            }

            Debug.Log(ObjectRotationType.ToString() + "Rotation Type");
        }
    }

    void PlaceFloor(Vector3 _hitPos, Vector3 _gridCellToWorldPos)
    {
        Vector3Int cellPos = grid.WorldToCell(_gridCellToWorldPos);
        Vector3 cellCenterWorld = grid.GetCellCenterWorld(cellPos);

        objectPreview.PreveiwObjectState(false);

        if (gridData.CanPlaceObject(cellPos, currentSelectedObjectData.size, ObjectRotationType))
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
        objectPreview.PreveiwObjectState(false);

        if (currentFloorData == null) return;

        if (currentFloorData.isEdgePlaceable(currentFloorEdge))
        {
            currentFloorData.SetFloorEdge(currentFloorEdge, currentSelectedObjectData.objectPrefab, currentPlacingPos, Quaternion.Euler(objectPreview.previewRotation));

            Debug.Log(currentFloorEdge.ToString());
        }
        else
        {
            Debug.Log("Edge already occupied");
        }
    }

    void SetFloorEdge(RaycastHit _hitInfo)
    {
        if (currentSelectedObjectData.PrefabType != PrefabTypes.Floor)
        {
            Transform hitTransform = _hitInfo.transform;

            if (hitTransform.tag == "Floor")
            {
                Vector3Int offset1 = Vector3Int.zero;
                Vector3Int offset2 = Vector3Int.zero;

                isRayhitFloor = true;
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
                }

                wallPosOffset1 = hitTransform.position + offset1;
                wallPosOffset2 = hitTransform.position + offset2;

                bool isPlaceable = currentFloorData.isEdgePlaceable(currentFloorEdge);
            }
            else
            {
                isRayhitFloor = false;
            }
        }
    }

    void PlaceObject(Transform _objectHolder, Vector3 _position, Quaternion _rotation, Transform _parent)
    {
        Instantiate(_objectHolder, _position, _rotation, _parent);
        Vector3Int cellPos = grid.WorldToCell(_position);
        Vector3 cellCenterWorld = grid.GetCellCenterWorld(cellPos);
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
        int listCount = objectPool.placeablePrefabs.Count;
        Transform selectedObject = placeableObject;
        Transform previewObject = objectPreview.objectPlaceHolder;
        PrefabTypes prefabTypes = PrefabTypes.Wall;

        if (scroll != 0)
        {
            if (scroll > 0)
            {
                if (objectPreview.objectPlaceHolder.gameObject.activeSelf)
                {
                    objectPreview.PreveiwObjectState(false);
                }

                selectedObjectIndex = (selectedObjectIndex - 1 + listCount) % listCount;
                selectedObject = objectPreview.SetPreviewObject(listCount, prefabTypes, previewObject);
            }
            else
            {
                if (objectPreview.objectPlaceHolder.gameObject.activeSelf)
                {
                    objectPreview.PreveiwObjectState(false);
                }

                selectedObjectIndex = (selectedObjectIndex + 1) % listCount;
                selectedObject = objectPreview.SetPreviewObject(listCount, prefabTypes, selectedObject);
            }
        }

        placeableObject = selectedObject;
    }
}
