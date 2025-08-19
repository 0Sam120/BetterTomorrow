using UnityEngine;
using UnityEngine.UIElements;

public class CoverLogic
{
    public void CalculateAllCover()
    {
        var grid = GridMap.Instance;
        var gridSize = grid.grid;

        for (int y = 0; y < grid.width; y++)
        {
            for (int x = 0; x < grid.length; x++)
            {
                if (y < 0 || x >= grid.length || y < 0 || y >= grid.width)
                {
                    Debug.LogError($"Grid out of range at ({x},{y}) with grid {grid.length}x{grid.width}");
                    continue;
                }

                if (gridSize[x, y].passable)
                {
                    CalculateCoverForTile(new Vector2Int(x, y));
                }
            }
        }
    }

    void CalculateCoverForTile(Vector2Int tilePos)
    {
        Vector3 worldPos = GridMap.Instance.GetWorldPosition(tilePos.x, tilePos.y);

        // Check each direction for cover
        CheckCoverInDirection(tilePos, worldPos, Vector3.forward, CoverDirection.North);
        CheckCoverInDirection(tilePos, worldPos, Vector3.back, CoverDirection.South);
        CheckCoverInDirection(tilePos, worldPos, Vector3.right, CoverDirection.East);
        CheckCoverInDirection(tilePos, worldPos, Vector3.left, CoverDirection.West);
    }

    void CheckCoverInDirection(Vector2Int tilePos, Vector3 worldPos, Vector3 direction, CoverDirection coverDir)
    {
        var grid = GridMap.Instance;
        var gridSize = grid.grid;

        if (!grid.CheckWalkable(tilePos))
        {
            // If the tile is not walkable, no cover can be checked
            gridSize[tilePos.x, tilePos.y].coverData[coverDir] = CoverType.None;
            return;
        }

        // Cast ray from tile center in the specified direction
        Vector3 rayStart = worldPos + Vector3.up * 0.1f; // Slightly above ground
        Vector3 rayEnd = rayStart + direction * grid.coverCheckDistance;

        // Check at multiple heights for full vs half cover
        bool hasLowCover = Physics.Linecast(rayStart, rayEnd, grid.coverLayer);
        bool hasHighCover = Physics.Linecast(rayStart + Vector3.up * 1.5f, rayEnd + Vector3.up * 1.5f, grid.coverLayer);

        CoverType coverType = CoverType.None;
        if (hasHighCover)
        {
            coverType = CoverType.Full;
        }
        else if (hasLowCover)
        {
            coverType = CoverType.Half;
        }

        gridSize[tilePos.x, tilePos.y].coverData[coverDir] = coverType;
    }

}
