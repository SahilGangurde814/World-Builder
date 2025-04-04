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

    public bool CanPlaceObject(Vector3Int Position, Vector2Int Size)
    {
        List<Vector3Int> occupiedPositions = CalculateSize(Position, Size);

        foreach (var position in occupiedPositions)
        {
            if(placeObjectsData.ContainsKey(position))
            {
                return false;
            }
        }

        return true;
    }

    public void AddData(Vector3Int Position, GameObject PlacedObject, Vector2Int Size)
    {
        List<Vector3Int> _occupiedPosition =  CalculateSize(Position, Size);
        PlacementData _placedData = new PlacementData();
        _placedData.PlaceableObject = PlacedObject;
        _placedData.occupiedPosition = _occupiedPosition;
        //placeObjectsData.Add(Position, PlacedData);

        foreach (var _position in _occupiedPosition)
        {
            placeObjectsData.Add(_position, _placedData);
        }
    }

    public void DestroyPlacedObjectData(Vector3Int key)  // if using cell's centre alignment for object placement use vector3
    {
        if (placeObjectsData.ContainsKey(key))
        {
            placeObjectsData.Remove(key);
        }
    }

    public List<Vector3Int> CalculateSize(Vector3Int Position, Vector2Int Size)
    {
        List<Vector3Int> returnSize = new();

        for(int x = 0; x < Size.x; x++)
        {
            for(int y = 0; y < Size.y; y++)
            {
                returnSize.Add(Position + new Vector3Int(x, 0, y));
            }
        }

        return returnSize;
    }
}
