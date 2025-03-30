using UnityEngine;

public class GridObject : MonoBehaviour
{
    [SerializeField] GridMap targetGrid;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        Vector2Int positionOnGrid = targetGrid.GetGridPosition(transform.position);
        targetGrid.PlaceObject(positionOnGrid, this);
    }
}
