using System.Collections.Generic;
using UnityEngine;

public class GridData : MonoBehaviour
{
    private Dictionary<Vector3, GameObject> placeObjectsData = new();

    public bool ContainsKey(Vector3 Position)
    {
        return placeObjectsData.ContainsKey(Position);
    }

    public void AddData(Vector3 Position, GameObject PlacedObject)
    {
        placeObjectsData.Add(Position, PlacedObject);
    }

    public void DestroyPlacedObjectData(Vector3Int key)  // if using cell's centre alignment for object placement use vector3
    {
        if (placeObjectsData.ContainsKey(key))
        {
            placeObjectsData.Remove(key);
        }
    }

    public List<Vector3> CalculateSize(Vector3 Position, Vector2 Size)
    {
        List<Vector3> returnSize = new();

        for(int x = 0; x < Size.x; x++)
        {
            for(int y = 0; y < Size.y; y++)
            {
                returnSize.Add(Position + new Vector3(x, 0, y));
            }
        }

        return returnSize;
    }
}
