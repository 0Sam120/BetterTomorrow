using System;
using System.Collections.Generic;
using UnityEngine;

public class GridMap : MonoBehaviour
{
    Node[,] grid;
    public int width = 25;
    public int length = 25;
    [SerializeField] float cellSize = 1f;
    [SerializeField] LayerMask obstacle;
    [SerializeField] LayerMask terrain;

    private void Awake()
    {
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        grid = new Node[length, width];

        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < length; x++)
            {
                grid[x, y] = new Node();
            }
        }

        CalculateElevation();
        CheckPassableGrid();
    }

    private void CalculateElevation()
    {
        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < length; x++)
            {
                Ray ray = new Ray(GetWorldPosition(x, y) + Vector3.up * 100f, Vector3.down);
                RaycastHit hit;
                if(Physics.Raycast(ray, out hit, float.MaxValue, terrain))
                {
                    grid[x, y].elevation = hit.point.y;
                }
            }
        }
    }

    public bool CheckBoundry(Vector2Int positionOnGrid)
    {
        if(positionOnGrid.x < 0 || positionOnGrid.x >= length)
        {
            return false;
        }
        if(positionOnGrid.y < 0  || positionOnGrid.y >= width)
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
                Vector3 worldPosition = GetWorldPosition(x, y);
                bool passable = !Physics.CheckBox(worldPosition, Vector3.one/2 *  cellSize, Quaternion.identity, obstacle);
                grid[x, y].passable = passable;
            }
        }
    }

    public Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        worldPosition.x += cellSize / 2;
        worldPosition.y += cellSize / 2;
        Vector2Int positionOfGrid = new Vector2Int((int)(worldPosition.x / cellSize), (int)(worldPosition.z / cellSize));
        return positionOfGrid;
    }

    private void OnDrawGizmos()
    {
        if (grid == null) 
        {
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
        return new Vector3(x * cellSize, elevation == true ? grid[x, y].elevation : 0f, y * cellSize);
    }

    public void PlaceObject(Vector2Int positionOnGrid, GridObject gridObject)
    {
        
        if(CheckBoundry(positionOnGrid) == true)
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
        if (CheckBoundry(gridPosition) == true)
        {
            GridObject gridObject = grid[gridPosition.x, gridPosition.y].gridObject;
            return gridObject;
        }
        return null;
    }

    internal bool CheckBoundry(int posX, int posY)
    {
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
        return grid[pos_x, pos_y].passable;
    }

    public List<Vector3> ConvertPathNodesToWorldPosition(List<PathNode> path)
    {
        List<Vector3> worldPositions = new List<Vector3>();

        for(int i = 0; i < path.Count; i++)
        {
            worldPositions.Add(GetWorldPosition(path[i].pos_x, path[i].pos_y, true));
        }

        return worldPositions;
    }
}
