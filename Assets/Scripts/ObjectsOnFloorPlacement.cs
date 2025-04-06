using System.Collections.Generic;
using UnityEngine;

public class ObjectsOnFloorPlacement : MonoBehaviour
{
    public Dictionary<Edge, Transform> FloorEdgeDat = new Dictionary<Edge, Transform>();

    public enum Edge
    {
        Up,
        Down, 
        Left, 
        Right
    }

    public void SetFloorEdge(Edge _edge, Transform _PlaceableObject)
    {
        FloorEdgeDat.Add( _edge, _PlaceableObject );
    }

    public bool isEdgePlaceable(Edge _edge)
    {
        if(FloorEdgeDat.ContainsKey(_edge))
        {
            return false;
        }
        return true;
    }
}
