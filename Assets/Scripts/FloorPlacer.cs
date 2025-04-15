using System.Collections.Generic;
using UnityEngine;

public class FloorPlacer : MonoBehaviour
{
    private float snapRange = 3.1f;
    private float floorSize = 3f;
    private List<Transform> placedFloors = new List<Transform>();
    Vector3 snapPos;
    public void PlaceFloor(Vector3 _rayHitPos, Transform _floorTransform)
    {
        snapPos = _rayHitPos;

        if (placedFloors.Count == 0)
        {
            GameObject newFloor1 = Instantiate(_floorTransform.gameObject, snapPos, Quaternion.identity);
            placedFloors.Add(newFloor1.transform);

            return;
        }

        snapPos = CalculateSnapPos(_rayHitPos);

        GameObject newFloor = Instantiate(_floorTransform.gameObject, snapPos, Quaternion.identity);
        placedFloors.Add(newFloor.transform);
    }


    public List<Transform> GetPlacedFloorsData() { return placedFloors; }
    public void SetPlacedFloorsData(Transform _floor)
    {
        placedFloors.Add(_floor);
    }

    public Vector3 GetSnapPos()
    {
        return snapPos;
    }

    public Vector3 CalculateSnapPos(Vector3 _rayHitPos)
    {
        Vector3 positionToSnap = _rayHitPos;

        foreach (Transform _floor in placedFloors)
        {
            Vector3 distance = (_rayHitPos - _floor.position);

            if (Mathf.Abs(distance.x) < snapRange && Mathf.Abs(distance.z) < snapRange)
            {
                if (Mathf.Abs(distance.x) > Mathf.Abs(distance.z))
                {
                    if (distance.x > 0)
                    {
                        Debug.Log("Right");
                        positionToSnap = _floor.position + new Vector3(floorSize, 0, 0);
                    }
                    else
                    {
                        Debug.Log("left");
                        positionToSnap = _floor.position + new Vector3(-floorSize, 0, 0);
                    }
                }
                else
                {
                    if (distance.z > 0)
                    {
                        Debug.Log("Top");
                        positionToSnap = _floor.position + new Vector3(0, 0, floorSize);
                    }
                    else
                    {
                        Debug.Log("Bottom");
                        positionToSnap = _floor.position + new Vector3(0, 0, -floorSize);
                    }
                }
            }

        }

        return positionToSnap;
    }
}
