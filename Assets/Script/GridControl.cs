using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class GridControl : MonoBehaviour
{
    [SerializeField] GridMap grid;
    [SerializeField] LayerMask terrain;

    Pathfinding pathfinding;
    Vector2Int currentPosition = new Vector2Int();
    List<PathNode> path;
    Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        pathfinding = grid.GetComponent<Pathfinding>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit, float.MaxValue, terrain))
            {
                Vector2Int gridPosition = grid.GetGridPosition(hit.point);

                path = pathfinding.FindPath(currentPosition.x, currentPosition.y, gridPosition.x, gridPosition.y);

                currentPosition = gridPosition;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if(path == null) { return; }
        if (path.Count == 0) { return; }

        for(int i = 0; i < path.Count - 1; i++)
        {
            Gizmos.DrawLine(grid.GetWorldPosition(path[i].pos_x, path[i].pos_y, true), grid.GetWorldPosition(path[i + 1].pos_x, path[i + 1].pos_y, true));
        }
    }
}
