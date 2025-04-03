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
            Debug.Log("Occupied Positions : " + position);
            if(placeObjectsData.ContainsKey(position))
            {
                Debug.Log("Can Not Place Object");
                return false;
            }
        }

        Debug.Log("Can Not Place Object");
        return true;
    }

    public void AddData(Vector3Int Position, GameObject PlacedObject, Vector2Int Size)
    {
        List<Vector3Int> occupiedPosition =  CalculateSize(Position, Size);
        PlacementData PlacedData = new PlacementData();
        PlacedData.PlaceableObject = PlacedObject;
        PlacedData.occupiedPosition = occupiedPosition;
        placeObjectsData.Add(Position, PlacedData);
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
