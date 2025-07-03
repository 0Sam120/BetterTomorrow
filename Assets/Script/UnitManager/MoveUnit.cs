using System;
using System.Collections.Generic;
using UnityEngine;

public class MoveUnit : MonoBehaviour
{
    [SerializeField] private GridMap grid; // Reference to the GridMap for position calculations
    [SerializeField] private GridRenderer targetRenderer;

    Pathfinding pathfinding; // Reference to the pathfinding system
    Camera cam; // Reference to the camera component

    private void Awake()
    {
        cam = GetComponent<Camera>(); // Get the Camera component attached to the same GameObject
        pathfinding = grid.GetComponent<Pathfinding>(); // Get the Pathfinding component from the grid
    }


    public void CheckWalkableTerrain(Character targetCharacter)
    {
        GridObject gridObject = targetCharacter.GetComponent<GridObject>();
        List<PathNode> walkableNodes = new List<PathNode>();
        pathfinding.ClearNodes();
        pathfinding.CalculateWalkableNodes(
            gridObject.positionOnGrid.x,
            gridObject.positionOnGrid.y,
            targetCharacter.MaxMoveSpeed,
            ref walkableNodes
            );
        targetRenderer.Hide();
        targetRenderer.fieldHighlight(walkableNodes);
    }

    public List<PathNode> GetPath(Vector2Int from)
    {
        List<PathNode> path = pathfinding.TraceBackPatch(from.x, from.y);

        if (path == null || path.Count == 0) { return null; }
        path.Reverse();

        return path;
    }
}
