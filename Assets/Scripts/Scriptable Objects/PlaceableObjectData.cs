using UnityEngine;

[CreateAssetMenu(fileName = "NewPlaceableObject", menuName = "Placement/Placeable Object") ]
public class PlaceableObjectData : ScriptableObject
{
    public Transform objectPrefab;
    public Transform objectPreview;
}
