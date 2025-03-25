using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlaceableObject", menuName = "Placement/Placeable Object") ]
public class PlaceableObjectData : ScriptableObject
{
    public List<PlaceablePrefabs> placeablePrefabs;

    public PlaceablePrefabs CurrentPrefab(PrefabTypes type)
    {
        return placeablePrefabs.Find((x) => x.PrefabType == type);
    }

    public PlaceablePrefabs GetObjectByIndex(int index) 
    {
        return placeablePrefabs[index];
    }
}

[Serializable]
public class PlaceablePrefabs
{
    public PrefabTypes PrefabType;
    public Transform objectPrefab;
    public Transform objectPreview;
}
[Serializable]
public enum PrefabTypes
{
    Wall,
    Window
}