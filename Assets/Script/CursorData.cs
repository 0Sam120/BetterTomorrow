using UnityEngine;
using UnityEngine.EventSystems;

public class CursorData : MonoBehaviour
{
    [SerializeField] Camera mainCamera;

    [SerializeField] GridMap targetGrid;
    [SerializeField] LayerMask terrainMask;

    public Vector2Int positionOnGrid;


    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject()) { return; }
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, float.MaxValue, terrainMask))
        {
            Vector2Int hitPosition = targetGrid.GetGridPosition(hit.point);
            if(hitPosition != positionOnGrid)
            {
                positionOnGrid = hitPosition;
            }
        }
    }
}
