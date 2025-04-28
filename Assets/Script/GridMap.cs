using System;
using System.Collections.Generic;
using UnityEngine;

public class GridMap : MonoBehaviour
{
    Node[,] grid; // 2D array to hold all grid nodes
    public int width = 25; // Number of cells along the width
    public int length = 25; // Number of cells along the length
    [SerializeField] float cellSize = 1f; // Size of each cell
    [SerializeField] LayerMask obstacle; // Layer used to detect obstacles
    [SerializeField] LayerMask terrain; // Layer used to detect terrain

    private void Awake()
    {
        GenerateGrid(); // Create the grid when the scene starts
    }

    private void GenerateGrid()
    {
        grid = new Node[length, width]; // Initialize the grid array

        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < length; x++)
            {
                grid[x, y] = new Node(); // Create a new Node at each cell
            }
        }

        CalculateElevation(); // Assign elevation to each cell
        CheckPassableGrid(); // Check for obstacles in each cell
    }

    private void CalculateElevation()
    {
        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < length; x++)
            {
                // Raycast downward to find terrain surface
                Ray ray = new Ray(GetWorldPosition(x, y) + Vector3.up * 100f, Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, float.MaxValue, terrain))
                {
                    grid[x, y].elevation = hit.point.y; // Set node's elevation
                }
            }
        }
    }

    public bool CheckBoundry(Vector2Int positionOnGrid)
    {
        // Check if the position is within the grid bounds
        if (positionOnGrid.x < 0 || positionOnGrid.x >= length)
        {
            return false;
        }
        if (positionOnGrid.y < 0 || positionOnGrid.y >= width)
        {
            return false;
        }

        return true;
    }

    private void CheckPassableGrid()
    {
        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < length; x++)
            {
                // Check if there's an obstacle at the node's world position
                Vector3 worldPosition = GetWorldPosition(x, y);
                bool passable = !Physics.CheckBox(worldPosition, Vector3.one / 2 * cellSize, Quaternion.identity, obstacle);
                grid[x, y].passable = passable; // Mark node as passable or not
            }
        }
    }

    public Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        // Convert a world position into a grid coordinate
        worldPosition.x += cellSize / 2;
        worldPosition.y += cellSize / 2;
        Vector2Int positionOfGrid = new Vector2Int((int)(worldPosition.x / cellSize), (int)(worldPosition.z / cellSize));
        return positionOfGrid;
    }

    private void OnDrawGizmos()
    {
        if (grid == null)
        {
            // If no grid exists yet, draw basic placeholders
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    Vector3 pos = GetWorldPosition(x, y);
                    Gizmos.DrawCube(pos, Vector3.one / 4);
                }
            }
        }
        else
        {
            // Draw cells with color depending on passability
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    Vector3 pos = GetWorldPosition(x, y, true);
                    Gizmos.color = grid[x, y].passable ? Color.white : Color.red;
                    Gizmos.DrawCube(pos, Vector3.one / 4);
                }
            }
        }
    }

    public Vector3 GetWorldPosition(int x, int y, bool elevation = false)
    {
        // Get the world position for a grid coordinate
        return new Vector3(x * cellSize, elevation ? grid[x, y].elevation : 0f, y * cellSize);
    }

    public void PlaceObject(Vector2Int positionOnGrid, GridObject gridObject)
    {
        // Place a GridObject at a specific cell
        if (CheckBoundry(positionOnGrid))
        {
            grid[positionOnGrid.x, positionOnGrid.y].gridObject = gridObject;
        }
        else
        {
            Debug.Log("Character object out of bounds");
        }
    }

    internal GridObject GetPlacedObject(Vector2Int gridPosition)
    {
        // Get the GridObject at a given position
        if (CheckBoundry(gridPosition))
        {
            GridObject gridObject = grid[gridPosition.x, gridPosition.y].gridObject;
            return gridObject;
        }
        return null;
    }

    internal bool CheckBoundry(int posX, int posY)
    {
        // Alternative overload to check bounds with ints instead of Vector2Int
        if (posX < 0 || posX >= length)
        {
            return false;
        }
        if (posY < 0 || posY >= width)
        {
            return false;
        }

        return true;
    }

    public bool CheckWalkable(int pos_x, int pos_y)
    {
        // Return whether a cell is walkable
        return grid[pos_x, pos_y].passable;
    }

    public List<Vector3> ConvertPathNodesToWorldPosition(List<PathNode> path)
    {
        // Convert a list of PathNodes into a list of world positions
        List<Vector3> worldPositions = new List<Vector3>();

        for (int i = 0; i < path.Count; i++)
        {
            worldPositions.Add(GetWorldPosition(path[i].pos_x, path[i].pos_y, true));
        }

        return worldPositions;
    }
}
