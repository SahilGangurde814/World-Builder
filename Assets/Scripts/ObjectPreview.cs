using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ObjectPreview : MonoBehaviour
{
    [SerializeField] private ObjectPlacer objectPlacer;
    [SerializeField] private GridData gridData;
    [SerializeField] private List<PlaceablePrefabs> preveiwObjectsData;

    public Transform objectPlaceHolder;
    public Material invalidPosMaterial;
    public Material validPosMaterial;
    [HideInInspector] public Vector3 previewRotation = Vector3.zero;


    public void SpawnPreviewObject(Vector3 _gridCellToWorldPos, Vector3 _currentHitPos, Vector3 _hitPos, Vector3 _mainCameraPos)
    {

        Vector3Int cellPos = objectPlacer.grid.WorldToCell(_gridCellToWorldPos);
        Vector3 cellCenterWorld = objectPlacer.grid.GetCellCenterWorld(cellPos);

        PreviewMaterial(gridData.CanPlaceObject(cellPos, objectPlacer.currentSelectedObjectData.size, objectPlacer.ObjectRotationType) ? validPosMaterial : invalidPosMaterial);

        float dist1 = (_hitPos - objectPlacer.wallPosOffset1).magnitude;
        float dist2 = (_hitPos - objectPlacer.wallPosOffset2).magnitude;

        if (objectPlacer.hasCancelledPlacement)
        {

            if (_currentHitPos != _hitPos)
            {
                _currentHitPos = _hitPos;
                if (objectPlacer.currentSelectedObjectData.PrefabType != PrefabTypes.Floor)
                {
                    if (dist1 < dist2) PreviewObjectSetup(objectPlacer.wallPosOffset1 + new Vector3(0.005f, 0, 0.005f), objectPlaceHolder, _mainCameraPos, previewRotation);
                    else PreviewObjectSetup(objectPlacer.wallPosOffset2 + new Vector3(0.005f, 0, 0.005f), objectPlaceHolder, _mainCameraPos, previewRotation);
                }
                else
                {
                    PreviewObjectSetup(_hitPos + new Vector3(0.005f, 0, 0.005f), objectPlaceHolder, _mainCameraPos, new Vector3(0, 0, 0));
                }
            }

            if (objectPlacer.isRayhitFloor == false)
            {
                PreviewObjectSetup(_hitPos + new Vector3(0.005f, 0, 0.005f), objectPlaceHolder, _mainCameraPos, previewRotation);
            }
        }
    }

    public void PreviewMaterial(Material _material)
    {
        Transform[] materialHoldingObjectsArr = objectPlaceHolder.GetComponent<MaterialData>().materialHoldingObjects;
        foreach (Transform materialHoldingObject in materialHoldingObjectsArr)
        {
            materialHoldingObject.GetComponent<MeshRenderer>().material = _material;
        }
    }

    public void PreviewObjectSetup(Vector3 _position, Transform _Object, Vector3 _cameraPos, Vector3 _rotation)
    {
        PreveiwObjectState(true);
        _Object.position = Vector3.Lerp(_Object.position, _position, 0.2f);
        //Object.position = position;
        Vector3 direction = _cameraPos - _Object.position;
        direction.y = 0;
        _Object.rotation = Quaternion.Euler(_rotation);
    }

    public void PreveiwObjectState(bool _isActive)
    {
        objectPlaceHolder.gameObject.SetActive(_isActive);
    }

    public Transform SetPreviewObject(int _listCount, PrefabTypes _prefabTypes, Transform _selectedObject)
    {
        _prefabTypes = objectPlacer.objectPool.placeablePrefabs[objectPlacer.selectedObjectIndex].PrefabType;
        PlaceablePrefabs placeablePrefabs = objectPlacer.objectPool.GetCurrentPrefab(_prefabTypes);
        _selectedObject = placeablePrefabs.objectPrefab;
        objectPlaceHolder = preveiwObjectsData.Find((x) => x.PrefabType == _prefabTypes).objectPrefab;
        objectPlacer.currentSelectedObjectData = placeablePrefabs;

        return _selectedObject;
    }
}
