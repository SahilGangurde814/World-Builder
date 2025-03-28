using UnityEngine;

public class DestroyPlacedObject : MonoBehaviour
{
    [SerializeField] private ObjectPlacer objectPlacer;
    [SerializeField] private LayerMask placedObjectLayer;

    private Camera _camera;
    private float maxDestroyDis;

    private void Start()
    {
        _camera = Camera.main;
        maxDestroyDis = objectPlacer.maxObjectPlacementDistance;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.B))
        {
            Ray ray = new Ray(_camera.transform.position, _camera.transform.forward);
            bool isHit = Physics.Raycast(ray, out RaycastHit hit, maxDestroyDis, placedObjectLayer);

            Debug.Log("Hit Object name : " + isHit);
            if (isHit)
            {
                Vector3 placedObjectPos = hit.transform.position;
                Vector3Int cellPos = objectPlacer.grid.WorldToCell(placedObjectPos);
                objectPlacer.DestroyPlacedObject(cellPos);
                Destroy(hit.transform.gameObject);
            }
        }
    }
}
