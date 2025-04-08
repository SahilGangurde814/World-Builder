using UnityEngine;

public class DestroyPlacedObject : MonoBehaviour
{
    [SerializeField] private ObjectPlacer objectPlacer;
    [SerializeField] private ObjectPreview objectPreview;
    [SerializeField] private LayerMask placedObjectLayer;
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private GridData gridData;

    private Camera _camera;
    private float maxDestroyDis;
    private Transform selectedObject;

    private void Start()
    {
        _camera = Camera.main;
        maxDestroyDis = objectPlacer.maxObjectPlacementDistance;
    }

    private void Update()
    {
        Ray ray = new Ray(_camera.transform.position, _camera.transform.forward);

        if (Input.GetKey(KeyCode.B))
        {
            bool isHit = Physics.Raycast(ray, out RaycastHit hit, maxDestroyDis, placedObjectLayer);

            if (isHit)
            {
                if(selectedObject != hit.transform && selectedObject != null)
                {
                    selectedObject.GetComponentInChildren<MeshRenderer>().material = defaultMaterial;
                }
                selectedObject = hit.transform;
                hit.transform.GetComponentInChildren<MeshRenderer>().material = objectPreview.invalidPosMaterial;
            }
            else if(!isHit && selectedObject != null)
            {
                selectedObject.GetComponentInChildren<MeshRenderer>().material = defaultMaterial;  
            }
        }

        if (Input.GetKeyUp(KeyCode.B))
        { 
            bool isHit = Physics.Raycast(ray, out RaycastHit hit, maxDestroyDis, placedObjectLayer);

            if (isHit && selectedObject == hit.transform) 
            {
                Vector3 placedObjectPos = hit.transform.position;
                Vector3Int cellPos = objectPlacer.grid.WorldToCell(placedObjectPos);
                gridData.DestroyPlacedObjectData(cellPos);
                Destroy(selectedObject.gameObject);
            }
        }
    }
}
