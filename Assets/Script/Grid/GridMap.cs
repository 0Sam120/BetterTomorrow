using System;
using System.Collections.Generic;
using UnityEngine;

public class GridMap : MonoBehaviour, IGridMap
{
    [HideInInspector] public Node[,] grid; // 2D array to hold all grid nodes

    private CoverLogic coverLogic; // Logic for calculating cover

    public LayerMask coverLayer; // Layer for cover props (walls, fences, etc.)
    public LayerMask terrain; // Layer used to detect terrain
    public Material halfCoverMaterial; // Material for half cover
    public Material fullCoverMaterial; // Material for full cover

    public static GridMap Instance { get; private set; } // Singleton instance of GridMap

    public int width = 25; // Number of cells along the width
    public int length = 25; // Number of cells along the length
    public float coverCheckDistance = 0.8f; // Distance to check for cover
    public float cellSize = 1f; // Size of each cell
    public bool showCoverIndicators = true; // Whether to show cover indicators in the scene


    private void Awake()
    {
        coverLogic = new CoverLogic();

        if (Instance == null)
        {
            Instance = this;
            // Initialize your grid here if needed
        }
        else
        {
            Destroy(gameObject);
        }

        GenerateGrid(); // Create the grid when the scene starts
    }

    private void Start()
    {
        coverLogic.CalculateAllCover(); // Calculate cover for all walkable tiles
        CreateCoverVisuals(); // Create visual indicators for cover
    }

    private void GenerateGrid()
    {
        grid = new Node[length, width]; // Initialize the grid array

        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < length; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Node node = new Node(pos);
                grid[x, y] = node; // Create a new Node at each cell

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
                bool passable = !Physics.CheckBox(worldPosition, Vector3.one * (cellSize * 0.3f) / 2, Quaternion.identity, coverLayer);
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

    public void CreateCoverVisuals()
    {
        if (!showCoverIndicators) return;

        GameObject coverParent = new GameObject("CoverVisuals");
        coverParent.transform.SetParent(transform);

        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < length; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                CreateCoverVisualsForTile(pos, coverParent.transform);
            }
        }
    }

    void CreateCoverVisualsForTile(Vector2Int tilePos, Transform parent)
    {
        Node tile = grid[tilePos.x, tilePos.y];
        if (!tile.passable) return;

        Vector3 worldPos = GetWorldPosition(tilePos.x, tilePos.y);

        foreach (var coverData in tile.coverData)
        {
            if (coverData.Value == CoverType.None) continue;

            GameObject coverIndicator = CreateCoverIndicator(coverData.Value, coverData.Key);
            coverIndicator.transform.SetParent(parent);

            // Position the indicator
            Vector3 indicatorPos = worldPos + GetDirectionOffset(coverData.Key) * (cellSize * 0.4f);
            indicatorPos.y = (coverData.Value == CoverType.Half) ? 0.5f : 1f;
            coverIndicator.transform.position = indicatorPos;

            // Rotate to face the correct direction
            coverIndicator.transform.rotation = GetDirectionRotation(coverData.Key);
        }
    }

    GameObject CreateCoverIndicator(CoverType coverType, CoverDirection direction)
    {
        GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Quad);
        indicator.name = $"Cover_{coverType}_{direction}";

        // Make indicators always visible and bright
        Renderer renderer = indicator.GetComponent<Renderer>();

        // Scale based on cover type
        if (coverType == CoverType.Half)
        {
            indicator.transform.localScale = new Vector3(1f, 1f, 0.8f);
            if (halfCoverMaterial != null)
            {
                renderer.material = halfCoverMaterial;
            }
            else
            {
                // Default bright yellow for half cover
                renderer.material.color = Color.yellow;
                renderer.material.SetFloat("_Metallic", 0f);
                renderer.material.SetFloat("_Smoothness", 0.5f);
            }
        }
        else
        {
            indicator.transform.localScale = new Vector3(1f, 1f, 0.8f);
            if (fullCoverMaterial != null)
            {
                renderer.material = fullCoverMaterial;
            }
            else
            {
                // Default bright red for full cover
                renderer.material.color = Color.red;
                renderer.material.SetFloat("_Metallic", 0f);
                renderer.material.SetFloat("_Smoothness", 0.5f);
            }
        }

        // Ensure indicators are always visible by making them emissive
        if (halfCoverMaterial == null || fullCoverMaterial == null)
        {
            Material mat = renderer.material;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", renderer.material.color * 0.3f);
        }

        // Remove collider as this is just visual
        DestroyImmediate(indicator.GetComponent<Collider>());

        return indicator;
    }

    Vector3 GetDirectionOffset(CoverDirection direction)
    {
        switch (direction)
        {
            case CoverDirection.North: return Vector3.forward;
            case CoverDirection.South: return Vector3.back;
            case CoverDirection.East: return Vector3.right;
            case CoverDirection.West: return Vector3.left;
            default: return Vector3.zero;
        }
    }

    Quaternion GetDirectionRotation(CoverDirection direction)
    {
        switch (direction)
        {
            case CoverDirection.North: return Quaternion.identity; ;
            case CoverDirection.South: return Quaternion.identity;
            case CoverDirection.East: return Quaternion.Euler(0, 90, 0);
            case CoverDirection.West: return Quaternion.Euler(0, 90, 0);
            default: return Quaternion.identity;
        }
    }

    public Vector3 GetWorldPosition(int x, int y, bool elevation = false)
    {
        // Get the world position for a grid coordinate
        return new Vector3(x * cellSize, elevation ? grid[x, y].elevation : 0f, y * cellSize);
    }

    internal void RemoveObject(Vector2Int positionOnGrid, GridObject gridObject)
    {
        if (CheckBoundry(positionOnGrid))
        {
            if (grid[positionOnGrid.x, positionOnGrid.y].gridObject != gridObject) { return; }
            grid[positionOnGrid.x, positionOnGrid.y].gridObject = null;
        }
        else
        {
            Debug.Log("Object outside bounds");
        }
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

    public bool CheckWalkable(Vector2Int pos)
    {
        // Return whether a cell is walkable
        return grid[pos.x, pos.y].passable;
    }

    public List<Vector3> ConvertPathToWorldPosition(List<Vector2Int> path)
    {
        List<Vector3> worldPositions = new List<Vector3>();

        if (path == null)
            return worldPositions;

        foreach (var tilePos in path)
        {
            worldPositions.Add(GetWorldPosition(tilePos.x, tilePos.y, true));
        }

        return worldPositions;
    }

}
