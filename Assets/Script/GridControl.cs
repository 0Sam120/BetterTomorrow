using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles player interaction with the grid-based map.
/// Calculates pathfinding when clicking on the terrain.
/// Draws the current path using Gizmos.
/// </summary>
public class GridControl : MonoBehaviour
{
    [SerializeField] GridMap grid;         // Reference to the grid map
    [SerializeField] LayerMask terrain;    // LayerMask to define valid terrain for clicks

    private Pathfinding pathfinding;        // Reference to the Pathfinding system
    private Vector2Int currentPosition = new Vector2Int(); // Current position on the grid
    private List<PathNode> path;             // Current calculated path
    private Camera cam;                      // Camera used to raycast from mouse

    private void Awake()
    {
        // Cache the camera component (assumed to be on the same GameObject)
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        // Get the Pathfinding component from the grid
        pathfinding = grid.GetComponent<Pathfinding>();
    }

    private void Update()
    {
        // On left mouse button click
        if (Input.GetMouseButtonDown(0))
        {
            // Create a ray from the camera through the mouse position
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Check if the ray hits the terrain layer
            if (Physics.Raycast(ray, out hit, float.MaxValue, terrain))
            {
                // Convert hit position to grid coordinates
                Vector2Int gridPosition = grid.GetGridPosition(hit.point);

                // Find a path from current position to the clicked position
                path = pathfinding.FindPath(currentPosition.x, currentPosition.y, gridPosition.x, gridPosition.y);

                // Update current position to the destination
                currentPosition = gridPosition;
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Don't draw if no path exists
        if (path == null) { return; }
        if (path.Count == 0) { return; }

        // Draw lines between each node in the path
        for (int i = 0; i < path.Count - 1; i++)
        {
            Gizmos.DrawLine(
                grid.GetWorldPosition(path[i].pos_x, path[i].pos_y, true),
                grid.GetWorldPosition(path[i + 1].pos_x, path[i + 1].pos_y, true)
            );
        }
    }
}
