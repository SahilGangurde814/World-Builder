using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    [Header("Placement Attributes")]
    [SerializeField] private LayerMask layerToPlaceObjects = 6;
    [SerializeField] private Transform objectToPlace;
    [SerializeField] private Transform objectPlaceHolder;
    [SerializeField] private PlaceableObjectData objectPool;
    [SerializeField] private Vector3 horizontalRotationOffset = new Vector3(0, 30, 0);
    [SerializeField] private Vector3 verticalRotationOffset = new Vector3(0, 0, 30);
    [SerializeField] private Transform rayIndicator;
    [SerializeField] private Transform placedObjectParent;
    [SerializeField] private List<PlaceablePrefabs> preveiwObjectsData;

    [SerializeField] private GridData gridData;

    public Material invalidPosMaterial;
    public Material validPosMaterial;

    public Grid grid;

    [Range(5, 30)] 
    public float maxObjectPlacementDistance = 10;
    private Camera mainCamera;
    private bool hasCancelledPlacement = true;
    private Vector3 previewRotation = Vector3.zero;
    private int selectedObjectIndex;
    private Transform placeableObject;
    private Transform placeableObjectPreview;
    private float halfHeight;
    private PlaceablePrefabs currentSelectedObjectData;

    private Rotation ObjectRotationType = Rotation.Forward;
    private Vector3Int wallPos;
    ObjectsOnFloorPlacement currentFloorData;
    ObjectsOnFloorPlacement.Edge currentFloorEdge;
    public enum Rotation
    {
        Forward,
        Backward,
        Right,
        Left
    }

    private void Start()
    {
        mainCamera = Camera.main;

        placeableObject = objectPool.GetCurrentPrefab(PrefabTypes.Wall).objectPrefab;
        currentSelectedObjectData = objectPool.GetCurrentPrefab(PrefabTypes.Wall);

        //Debug.Log("Rotation Type : " + ObjectRotationType.ToString());

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
                    PreveiwObjectState(hasCancelledPlacement);
                }

                SetObjectRotation();

                SpawnPreviewObject(gridCellToWorldPos, currentHitPos, hitPos, mainCameraPos);
            }
            
            if(Input.GetMouseButtonUp(0))
            {
                if (hasCancelledPlacement == false) return;

                if (currentSelectedObjectData.PrefabType == PrefabTypes.Floor) 
                { 
                    PreveiwObjectState(false);
                    //Vector3 direction = mainCameraPos - objectPlaceHolder.position;
                    //direction.y = 0;    for object to not rotate on x axis
                    Vector3Int cellPos = grid.WorldToCell(gridCellToWorldPos);
                    Vector3 cellCenterWorld = grid.GetCellCenterWorld(cellPos);
                    if(gridData.CanPlaceObject(cellPos, currentSelectedObjectData.size, ObjectRotationType) /*&& currentSelectedObjectData.PrefabType == PrefabTypes.Floor*/)
                    {
                        PlaceObject(placeableObject, cellPos, /*Quaternion.Euler(previewRotation)*/ Quaternion.identity, placedObjectParent);
                    }
                    else
                    {
                        Debug.Log("Position occupied OR Object Type is not Floor");
                    }
                }
                else if(currentSelectedObjectData.PrefabType != PrefabTypes.Floor)
                {
                    PreveiwObjectState(false);
                    //PlaceObject(placeableObject, wallPos, Quaternion.Euler(previewRotation), placedObjectParent);
                    if(currentFloorData == null) return;
                    if (currentFloorData.isEdgePlaceable(currentFloorEdge))
                    {
                        currentFloorData.SetFloorEdge(currentFloorEdge, currentSelectedObjectData.objectPrefab, wallPos, Quaternion.Euler(previewRotation));
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
            PreveiwObjectState(false);
        }
    }

    void SpawnPreviewObject(Vector3 _gridCellToWorldPos, Vector3 _currentHitPos, Vector3 _hitPos, Vector3 _mainCameraPos)
    {
        //// if object's pivot is in center use this
        //MeshRenderer meshRenderer = objectPlaceHolder.GetComponentInChildren<MeshRenderer>();
        //float height = meshRenderer.bounds.size.y;
        //halfHeight = height / 2;

        Vector3Int cellPos = grid.WorldToCell(_gridCellToWorldPos);
        Vector3 cellCenterWorld = grid.GetCellCenterWorld(cellPos);

        if (!gridData.CanPlaceObject(cellPos, currentSelectedObjectData.size, ObjectRotationType))
        {
            PreviewMaterial(invalidPosMaterial);
        }
        else
        {
            PreviewMaterial(validPosMaterial);
        }

        if (hasCancelledPlacement /*&& currentSelectedObjectData.PrefabType != PrefabTypes.Wall*/)
        {

            if (_currentHitPos != _hitPos)
            {
                _currentHitPos = _hitPos;
                if (currentSelectedObjectData.PrefabType != PrefabTypes.Floor)
                {
                    PreviewObjectSetup(cellPos + new Vector3(0.005f, 0, 0.005f) /*+ new Vector3(0, halfHeight, 0)*/, objectPlaceHolder, _mainCameraPos, previewRotation);
                }
                else
                {
                    PreviewObjectSetup(cellPos + new Vector3(0.005f, 0, 0.005f) /*+ new Vector3(0, halfHeight, 0)*/, objectPlaceHolder, _mainCameraPos, new Vector3(0, 0, 0));
                }
            }
            else
            {
                PreviewObjectSetup(cellPos + new Vector3(0.005f, 0, 0.005f) /*+ new Vector3(0, halfHeight, 0)*/, objectPlaceHolder, _mainCameraPos, previewRotation);
            }
        }
    }

    void SetObjectRotation()
    {
        if (currentSelectedObjectData.PrefabType != PrefabTypes.Floor)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                previewRotation = previewRotation + horizontalRotationOffset;
                SetRotationType(previewRotation);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                previewRotation = previewRotation - horizontalRotationOffset;
                SetRotationType(previewRotation);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                previewRotation = previewRotation + verticalRotationOffset;
                SetRotationType(previewRotation);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                previewRotation = previewRotation - verticalRotationOffset;
                SetRotationType(previewRotation);
            }
        }
    }

    void SetFloorEdge(RaycastHit _hitInfo)
    {
        if (currentSelectedObjectData.PrefabType != PrefabTypes.Floor)
        {
            Transform hitTransform = _hitInfo.transform;
            if (hitTransform.tag == "Floor")
            {
                //Debug.Log("Wall Type");
                currentFloorData = hitTransform.GetComponent<ObjectsOnFloorPlacement>();
                currentFloorEdge = new ObjectsOnFloorPlacement.Edge();
                Vector3Int floorPos = grid.WorldToCell(_hitInfo.transform.position);
                Vector3Int offset = Vector3Int.zero;
                switch (ObjectRotationType)
                {
                    case Rotation.Forward:
                        currentFloorEdge = ObjectsOnFloorPlacement.Edge.Up;
                        offset = new Vector3Int(0, 0, 2);
                        break;
                    case Rotation.Backward:
                        currentFloorEdge = ObjectsOnFloorPlacement.Edge.Down;
                        offset = new Vector3Int(2, 0, 0);
                        break;
                    case Rotation.Left:
                        currentFloorEdge = ObjectsOnFloorPlacement.Edge.Left;
                        offset = new Vector3Int(0, 0, 0);
                        break;
                    case Rotation.Right:
                        currentFloorEdge = ObjectsOnFloorPlacement.Edge.Right;
                        offset = new Vector3Int(2, 0, 2);
                        break;
                }
                wallPos = floorPos + offset;
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
        }
    }


    void SetRotationType(Vector3 _rotation)
    {
        // Normalize the rotation to 0-360 range
        float normalizedY = _rotation.y % 360;
        if (normalizedY < 0) normalizedY += 360;

        // Use thresholds to determine rotation type
        if (normalizedY >= 315 || normalizedY < 45)
            ObjectRotationType = Rotation.Forward;
        else if (normalizedY >= 45 && normalizedY < 135)
            ObjectRotationType = Rotation.Right;
        else if (normalizedY >= 135 && normalizedY < 225)
            ObjectRotationType = Rotation.Backward;
        else
            ObjectRotationType = Rotation.Left;

        //Debug.Log("Rotation Type: " + ObjectRotationType.ToString());
    }

    void PlaceObject(Transform _objectHolder, Vector3 _position, Quaternion _rotation, Transform _parent)
    {
        Instantiate(_objectHolder, _position /*+ new Vector3(0, halfHeight, 0)*/, _rotation, _parent);
        Vector3Int cellPos = grid.WorldToCell(_position);
        Vector3 cellCenterWorld = grid.GetCellCenterWorld(cellPos);
        gridData.AddData(cellPos, _objectHolder.gameObject, currentSelectedObjectData.size, ObjectRotationType);
    }

    void PreveiwObjectState(bool _isActive)
    {
        objectPlaceHolder.gameObject.SetActive(_isActive);
    }

    void PreviewObjectSetup(Vector3 _position, Transform _Object, Vector3 _cameraPos, Vector3 _rotation)
    {
        PreveiwObjectState(true);
        _Object.position = Vector3.Lerp(_Object.position, _position, 0.2f);
        //Object.position = position;
        Vector3 direction = _cameraPos - _Object.position;
        direction.y = 0;
        _Object.rotation = Quaternion.Euler(_rotation);
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
                selectedObject = SetPreviewObject(listCount, prefabTypes, previewObject);
            }
            else
            {
                if (objectPlaceHolder.gameObject.activeSelf)
                {
                    PreveiwObjectState(false);
                }
                selectedObjectIndex = (selectedObjectIndex + 1) % listCount;
                selectedObject = SetPreviewObject(listCount, prefabTypes, selectedObject);
            }
        }

        placeableObject = selectedObject;
    }

    void PreviewMaterial(Material _material)
    {
        Transform[] materialHoldingObjectsArr = objectPlaceHolder.GetComponent<MaterialData>().materialHoldingObjects;
        foreach (Transform materialHoldingObject in materialHoldingObjectsArr)
        {
            materialHoldingObject.GetComponent<MeshRenderer>().material = _material;
        }
    }

    Transform SetPreviewObject(int _listCount, PrefabTypes _prefabTypes, Transform _selectedObject)
    {
        _prefabTypes = objectPool.placeablePrefabs[selectedObjectIndex].PrefabType;
        PlaceablePrefabs placeablePrefabs = objectPool.GetCurrentPrefab(_prefabTypes);
        _selectedObject = placeablePrefabs.objectPrefab;
        objectPlaceHolder = preveiwObjectsData.Find((x) => x.PrefabType == _prefabTypes).objectPrefab;
        currentSelectedObjectData = placeablePrefabs;

        return _selectedObject;
    }

    //public void DestroyPlacedObject(Vector3Int key)
    //{
    //    if(placeObjectsData.ContainsKey(key))
    //    {
    //        placeObjectsData.Remove(key);
    //    }
    //}
}
