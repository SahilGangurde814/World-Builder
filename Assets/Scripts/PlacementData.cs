using UnityEngine;

public class PlacementData : MonoBehaviour
{
    private ObjectsOnFloorPlacement.Edge placedEdge;
    private ObjectPlacer.Rotation rotation;

    public ObjectsOnFloorPlacement.Edge GetPlacedEdge()
    {
        return placedEdge;
    }

    public void SetPlacedEdge(ObjectsOnFloorPlacement.Edge _placedEdge)
    {
        _placedEdge = placedEdge;
    }

    public ObjectPlacer.Rotation GetRotation()
    {
        return rotation;
    }

    public void SetRotation(ObjectPlacer.Rotation _rotation)
    {
        _rotation = rotation;
    }
}
