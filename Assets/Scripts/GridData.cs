using System.Collections.Generic;
using UnityEngine;

public class GridData : MonoBehaviour
{
    private Dictionary<Vector3Int, PlacementData> placeObjectsData = new();

    public class PlacementData
    {
        public GameObject PlaceableObject;
        public List<Vector3Int> occupiedPosition;
    }

    public bool CanPlaceObject(Vector3Int _Position, Vector2Int _Size, ObjectPlacer.Rotation _rotation)
    {
        List<Vector3Int> occupiedPositions = CalculateSize(_Position, _Size, _rotation);

        foreach (var position in occupiedPositions)
        {
            if(placeObjectsData.ContainsKey(position))
            {
                return false;
            }
        }

        return true;
    }

    public void AddData(Vector3Int _Position, GameObject _PlacedObject, Vector2Int _Size, ObjectPlacer.Rotation _rotation)
    {
        List<Vector3Int> _occupiedPosition =  CalculateSize(_Position, _Size, _rotation);
        PlacementData _placedData = new PlacementData();
        _placedData.PlaceableObject = _PlacedObject;
        _placedData.occupiedPosition = _occupiedPosition;
        //placeObjectsData.Add(Position, PlacedData);

        foreach (var _position in _occupiedPosition)
        {
            placeObjectsData.Add(_position, _placedData);
        }
    }

    public void DestroyPlacedObjectData(Vector3Int _key)  // if using cell's centre alignment for object placement use vector3
    {
        if (placeObjectsData.ContainsKey(_key))
        {
            placeObjectsData.Remove(_key);
        }
    }

    public List<Vector3Int> CalculateSize(Vector3Int _Position, Vector2Int _Size, ObjectPlacer.Rotation _rotationType)
    {
        List<Vector3Int> returnSize = new();

        switch (_rotationType)
        {
            case ObjectPlacer.Rotation.Forward:

                for(int x = 0; x < _Size.x; x++)
                {
                    for(int y = 0; y < _Size.y; y++)
                    {
                        returnSize.Add(_Position + new Vector3Int(x, 0, y));
                    }
                }
                break;
                //return returnSize;

            case ObjectPlacer.Rotation.Left:

                for (int x = 0; x < _Size.y; x++)
                {
                    for (int y = 0; y < _Size.x; y++)
                    {
                        returnSize.Add(_Position + new Vector3Int(x, 0, y));
                    }
                }
                break;
                //return returnSize;

            case ObjectPlacer.Rotation.Right:

                for (int x = 0; x < _Size.y; x++)
                {
                    for (int y = 0; y < _Size.x; y++)
                    {
                        returnSize.Add(_Position - new Vector3Int(x, 0, y));
                    }
                }
                break;

            case ObjectPlacer.Rotation.Backward:

                for (int x = 0; x < _Size.x; x++)
                {
                    for (int y = 0; y < _Size.y; y++)
                    {
                        returnSize.Add(_Position - new Vector3Int(x, 0, y));
                    }
                }
                break;
        }

        foreach(Vector3 position in returnSize)
        {
            Debug.Log("Occuping Position : " + position);
        }
        return returnSize;
    }
}
