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

    public void SetFloorEdge(Edge _edge, Transform _PlaceableObject, Vector3 _position, Quaternion _rotation)
    {
        if (_edge == Edge.Down)
        {
            _rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (_edge == Edge.Left) 
        {
            _rotation = Quaternion.Euler(0, 270, 0);
            Debug.Log("Rotation : " + _rotation);
        }
        
        Instantiate(_PlaceableObject, _position, _rotation, this.transform);
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
