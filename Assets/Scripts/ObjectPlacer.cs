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

    private Camera mainCamera;
    private Transform placeableObject;
    private Transform placeableObjectPreview;
    private float halfHeight;
    private Vector3 wallPos;
    private ObjectsOnFloorPlacement currentFloorData;
    [HideInInspector] public ObjectsOnFloorPlacement.Edge currentFloorEdge;

    public enum Rotation
    {
        //Forward,
        //Backward,
        //Right,
        //Left,
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

            //Debug.DrawRay(mainCameraPos, hitPos);

            if (Input.GetMouseButtonDown(0))
            {
                hasCancelledPlacement = true;
            }

            if (Input.GetMouseButton(0))
            {
                SetFloorEdge(hitInfo);

                if (Input.GetMouseButtonDown(1) && hasCancelledPlacement)
                {
                    hasCancelledPlacement = false;
                    objectPreview.PreveiwObjectState(hasCancelledPlacement);
                }

                //SetObjectRotation();

                if (Input.GetKeyDown(KeyCode.R))
                {
                    if (currentSelectedObjectData.PrefabType == PrefabTypes.Floor) return;

                    if (objectPreview.previewRotation.y == 90)
                    {
                        objectPreview.previewRotation = new Vector3(0, 0, 0);
                        ObjectRotationType = Rotation.Vertical;
                    }
                    else
                    {
                        objectPreview.previewRotation = new Vector3(0, 90, 0);
                        ObjectRotationType = Rotation.Horizontal;
                    }
                    //previewRotation = previewRotation.y == 90 ? new Vector3(0, 0, 0) : new Vector3(0, 90, 0);
                    //SetRotationType(previewRotation);
                    
                }

                objectPreview.SpawnPreviewObject(gridCellToWorldPos, currentHitPos, hitPos, mainCameraPos);
            }
            
            if(Input.GetMouseButtonUp(0))
            {
                if (hasCancelledPlacement == false) return;

                if (currentSelectedObjectData.PrefabType == PrefabTypes.Floor) 
                { 
                    objectPreview.PreveiwObjectState(false);
                    //Vector3 direction = mainCameraPos - objectPlaceHolder.position;
                    //direction.y = 0;    for object to not rotate on x axis
                    Vector3Int cellPos = grid.WorldToCell(gridCellToWorldPos);
                    Vector3 cellCenterWorld = grid.GetCellCenterWorld(cellPos);
                    if(gridData.CanPlaceObject(cellPos, currentSelectedObjectData.size, ObjectRotationType) /*&& currentSelectedObjectData.PrefabType == PrefabTypes.Floor*/)
                    {
                        PlaceObject(placeableObject, /*cellPos*/ hitPos, /*Quaternion.Euler(previewRotation)*/ Quaternion.identity, placedObjectParent);
                        Debug.Log("Placed object");
                    }
                    else
                    {
                        Debug.Log("Position occupied OR Object Type is not Floor");
                    }
                }
                else if(currentSelectedObjectData.PrefabType != PrefabTypes.Floor)
                {
                    objectPreview.PreveiwObjectState(false);
                    //PlaceObject(placeableObject, wallPos, Quaternion.Euler(previewRotation), placedObjectParent);
                    if(currentFloorData == null) return;
                    if (currentFloorData.isEdgePlaceable(currentFloorEdge))
                    {
                        currentFloorData.SetFloorEdge(currentFloorEdge, currentSelectedObjectData.objectPrefab, wallPos, Quaternion.Euler(objectPreview.previewRotation));
                        Debug.Log(currentFloorEdge.ToString());
                    }
                    else
                    {
                        Debug.Log("Edge already occupied");
                    }
                }
            }
        }
        else
        {
            rayIndicator.gameObject.SetActive(false);
            objectPreview.PreveiwObjectState(false);
        }
    }

    //void SetObjectRotation()
    //{
    //    if (currentSelectedObjectData.PrefabType == PrefabTypes.Floor) return;
        
    //    if (Input.GetKeyDown(KeyCode.Alpha1))
    //    {
    //        objectPreview.previewRotation = objectPreview.previewRotation + horizontalRotationOffset;
    //        SetRotationType(objectPreview.previewRotation);
    //    }
    //    else if (Input.GetKeyDown(KeyCode.Alpha2))
    //    {
    //        objectPreview.previewRotation = objectPreview.previewRotation - horizontalRotationOffset;
    //        SetRotationType(objectPreview.previewRotation);
    //    }
    //    else if (Input.GetKeyDown(KeyCode.Alpha3))
    //    {
    //        objectPreview.previewRotation = objectPreview.previewRotation + verticalRotationOffset;
    //        SetRotationType(objectPreview.previewRotation);
    //    }
    //    else if (Input.GetKeyDown(KeyCode.Alpha4))
    //    {
    //        objectPreview.previewRotation = objectPreview.previewRotation - verticalRotationOffset;
    //        SetRotationType(objectPreview.previewRotation);
    //    }
    //}

    void SetFloorEdge(RaycastHit _hitInfo)
    {
        if (currentSelectedObjectData.PrefabType != PrefabTypes.Floor)
        {
            Transform hitTransform = _hitInfo.transform;
            if (hitTransform.tag == "Floor")
            {
                //Debug.Log("Wall Type");
                isRayhitFloor = true;
                currentFloorData = hitTransform.GetComponent<ObjectsOnFloorPlacement>();
                currentFloorEdge = new ObjectsOnFloorPlacement.Edge();
                Vector3Int floorPos = grid.WorldToCell(_hitInfo.transform.position);
                Vector3Int offset = Vector3Int.zero;
                Vector3Int offset1 = Vector3Int.zero;
                Vector3Int offset2 = Vector3Int.zero;
                switch (ObjectRotationType)
                {
                    //case Rotation.Forward:
                    //    currentFloorEdge = ObjectsOnFloorPlacement.Edge.Up;
                    //    offset = new Vector3Int(0, 0, 2);
                    //    break;
                    //case Rotation.Backward:
                    //    currentFloorEdge = ObjectsOnFloorPlacement.Edge.Down;
                    //    offset = new Vector3Int(2, 0, 0);
                    //    break;
                    //case Rotation.Left:
                    //    currentFloorEdge = ObjectsOnFloorPlacement.Edge.Left;
                    //    offset = new Vector3Int(0, 0, 0);
                    //    break;
                    //case Rotation.Right:
                    //    currentFloorEdge = ObjectsOnFloorPlacement.Edge.Right;
                    //    offset = new Vector3Int(2, 0, 2);
                    //    break;
                    case Rotation.Horizontal:
                        offset1 = new Vector3Int(0, 0, 0);
                        offset2 = new Vector3Int(0, 0, 2);
                        break;
                    case Rotation.Vertical:
                        offset1 = new Vector3Int(0, 0, 2);
                        offset2 = new Vector3Int(2, 0, 2);
                        break;
                }
                wallPos = /*floorPos*/ hitTransform.position + offset;
                wallPosOffset1 = hitTransform.position + offset1;
                wallPosOffset2 = hitTransform.position + offset2;

                bool isPlaceable = currentFloorData.isEdgePlaceable(currentFloorEdge);


                //Debug.Log("floor pos : " + floorPos + "Wall Pos : " + wallPos);
                //if(isPlaceable)
                //{
                //    Debug.Log("Object can be placed");
                //    //floorData.SetFloorEdge(ObjectsOnFloorPlacement.Edge.Up, currentSelectedObjectData.objectPrefab);
                //}
                //else
                //{
                //    Debug.Log("Not Possible");
                //}
            }
            else
            {
                isRayhitFloor = false;
            }
        }
    }

    //void SetRotationType(Vector3 _rotation)
    //{
    //    // Normalize the rotation to 0-360 range
    //    float normalizedY = _rotation.y % 360;
    //    if (normalizedY < 0) normalizedY += 360;

    //    // Use thresholds to determine rotation type
    //    if (normalizedY >= 315 || normalizedY < 45)
    //        ObjectRotationType = Rotation.Forward;
    //    else if (normalizedY >= 45 && normalizedY < 135)
    //        ObjectRotationType = Rotation.Right;
    //    else if (normalizedY >= 135 && normalizedY < 225)
    //        ObjectRotationType = Rotation.Backward;
    //    else
    //        ObjectRotationType = Rotation.Left;

    //    //Debug.Log("Rotation Type: " + ObjectRotationType.ToString());
    //}

    void PlaceObject(Transform _objectHolder, Vector3 _position, Quaternion _rotation, Transform _parent)
    {
        Instantiate(_objectHolder, _position /*+ new Vector3(0, halfHeight, 0)*/, _rotation, _parent);
        Vector3Int cellPos = grid.WorldToCell(_position);
        Vector3 cellCenterWorld = grid.GetCellCenterWorld(cellPos);
        gridData.AddData(cellPos, _objectHolder.gameObject, currentSelectedObjectData.size, ObjectRotationType);
    }

    
    

    Vector3 GridToWorldPos(Vector3 _hitPos)
    {
        Vector3Int cellPos = grid.WorldToCell(_hitPos);
        //rayIndicator.gameObject.SetActive(true);
        //rayIndicator.transform.position = grid.CellToWorld(cellPos);
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
            if(scroll > 0)
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
