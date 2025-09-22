using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GridRenderer : MonoBehaviour
{
    [SerializeField] private GameObject highlightPrefab;
    private GameObject selectedCellHighlight;
    private GameObject coverParent;
    private LineRenderer lineRenderer;

    private List<Vector2Int> currentMovementArea = new List<Vector2Int>();

    private bool isInitialized = false;

    private void OnEnable() => CursorData.OnCursorMoved += HighlightSelectedCell;
    private void OnDisable() => CursorData.OnCursorMoved -= HighlightSelectedCell;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        lineRenderer.loop = true;
        lineRenderer.widthMultiplier = 0.05f;
    }

    public void Hide()
    {
        isInitialized = false;

        // Clear selected cell highlight
        ClearSelectedCell();
        
        // Clear outline
        lineRenderer.positionCount = 0;
    }

    public void FieldHighlight(List<Vector2Int> cells)
    {
        // Clear old stuff
        Hide();

        // Draw the smooth outline around all cells
        DrawMovementOutline(cells);
    }

    private void HighlightSelectedCell(Vector2Int selectedCell)
    {
        if (!isInitialized) return;
        
        // Create cover parent if it doesn’t exist yet
        if (coverParent == null)
        {
            coverParent = new GameObject("CoverVisuals");
            coverParent.transform.SetParent(transform);
        }

        // Destroy old highlight
        if (selectedCellHighlight != null)
        {
            Destroy(selectedCellHighlight);
        }

        // Destroy old cover indicators too
        foreach (Transform child in coverParent.transform)
        {
            Destroy(child.gameObject);
        }

        // Only highlight if inside the current movement area
        if (currentMovementArea != null && currentMovementArea.Contains(selectedCell) && isInitialized)
        {
            var pos = GridMap.Instance.GetWorldPosition(selectedCell.x, selectedCell.y);
            pos.y += 0.05f; // Slightly above ground to avoid z-fighting
            var rotation = Quaternion.Euler(90f, 0f, 0f);
            selectedCellHighlight = Instantiate(highlightPrefab, pos, rotation, transform);

            GridMap.Instance.CreateCoverVisualsForTile(selectedCell, coverParent.transform);
        }
    }

    public void ClearSelectedCell()
    {
        if (selectedCellHighlight != null)
        {
            Destroy(selectedCellHighlight);
            selectedCellHighlight = null;
        }
    }

    private void DrawMovementOutline(List<Vector2Int> cells)
    {
        if (cells == null || cells.Count == 0)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        // Convert to HashSet for fast lookup
        HashSet<Vector2Int> cellSet = new HashSet<Vector2Int>(cells);

        // Find all edge segments that form the perimeter
        List<EdgeSegment> perimeter = new List<EdgeSegment>();

        foreach (var cell in cells)
        {
            // Check each of the 4 edges of this cell
            AddEdgeIfPerimeter(cell, Vector2Int.up, cellSet, perimeter);      // Top edge
            AddEdgeIfPerimeter(cell, Vector2Int.right, cellSet, perimeter);   // Right edge  
            AddEdgeIfPerimeter(cell, Vector2Int.down, cellSet, perimeter);    // Bottom edge
            AddEdgeIfPerimeter(cell, Vector2Int.left, cellSet, perimeter);    // Left edge
        }

        // Convert edge segments to world positions
        List<Vector3> outlinePoints = new List<Vector3>();
        float cellSize = 1f; // Adjust based on your grid cell size

        foreach (var edge in perimeter)
        {
            Vector3 worldPos = GridMap.Instance.GetWorldPosition(edge.cell.x, edge.cell.y);
            Vector3 point1, point2;

            switch (edge.direction)
            {
                case 0: // Top edge (up)
                    point1 = worldPos + new Vector3(-cellSize / 2, 0.05f, cellSize / 2);
                    point2 = worldPos + new Vector3(cellSize / 2, 0.05f, cellSize / 2);
                    break;
                case 1: // Right edge
                    point1 = worldPos + new Vector3(cellSize / 2, 0.05f, cellSize / 2);
                    point2 = worldPos + new Vector3(cellSize / 2, 0.05f, -cellSize / 2);
                    break;
                case 2: // Bottom edge (down)
                    point1 = worldPos + new Vector3(cellSize / 2, 0.05f, -cellSize / 2);
                    point2 = worldPos + new Vector3(-cellSize / 2, 0.05f, -cellSize / 2);
                    break;
                case 3: // Left edge
                    point1 = worldPos + new Vector3(-cellSize / 2, 0.05f, -cellSize / 2);
                    point2 = worldPos + new Vector3(-cellSize / 2, 0.05f, cellSize / 2);
                    break;
                default:
                    continue;
            }

            outlinePoints.Add(point1);
            outlinePoints.Add(point2);
        }

        if (outlinePoints.Count == 0)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        // Remove duplicate points and sort for proper outline
        outlinePoints = ConnectEdgeSegments(outlinePoints);

        lineRenderer.positionCount = outlinePoints.Count;
        lineRenderer.SetPositions(outlinePoints.ToArray());
        isInitialized = true;
        SetMovementArea(cells);
    }

    public void SetMovementArea(List<Vector2Int> movementArea)
    {
        currentMovementArea = movementArea;
    }

    private void AddEdgeIfPerimeter(Vector2Int cell, Vector2Int direction, HashSet<Vector2Int> cellSet, List<EdgeSegment> perimeter)
    {
        Vector2Int neighbor = cell + direction;

        // If the neighbor is not in our movement area, this edge is part of the perimeter
        if (!cellSet.Contains(neighbor))
        {
            int dirIndex = GetDirectionIndex(direction);
            perimeter.Add(new EdgeSegment(cell, dirIndex));
        }
    }

    private int GetDirectionIndex(Vector2Int direction)
    {
        if (direction == Vector2Int.up) return 0;
        if (direction == Vector2Int.right) return 1;
        if (direction == Vector2Int.down) return 2;
        if (direction == Vector2Int.left) return 3;
        return -1;
    }

    private List<Vector3> ConnectEdgeSegments(List<Vector3> points)
    {
        // Build adjacency map: every point knows which points it's connected to
        Dictionary<Vector3, List<Vector3>> adjacency = new Dictionary<Vector3, List<Vector3>>();
        for (int i = 0; i < points.Count; i += 2)
        {
            Vector3 a = points[i];
            Vector3 b = points[i + 1];

            if (!adjacency.ContainsKey(a)) adjacency[a] = new List<Vector3>();
            if (!adjacency.ContainsKey(b)) adjacency[b] = new List<Vector3>();

            adjacency[a].Add(b);
            adjacency[b].Add(a);
        }

        // Pick a starting point: lowest X, then lowest Z, just to be consistent
        Vector3 start = points[0];
        foreach (var p in adjacency.Keys)
        {
            if (p.x < start.x || (Mathf.Approximately(p.x, start.x) && p.z < start.z))
                start = p;
        }

        List<Vector3> ordered = new List<Vector3>();
        HashSet<Vector3> visited = new HashSet<Vector3>();

        Vector3 current = start;
        Vector3 previous = Vector3.positiveInfinity; // something invalid

        // Walk edges until we return to the start
        do
        {
            ordered.Add(current);
            visited.Add(current);

            // pick the next neighbor that's not the one we just came from
            Vector3 next = adjacency[current][0];
            if (adjacency[current].Count > 1)
            {
                if (next == previous)
                    next = adjacency[current][1];
            }

            previous = current;
            current = next;

        } while (current != start && !visited.Contains(current));

        // close the loop
        if (ordered.Count > 0 && ordered[0] != ordered[ordered.Count - 1])
            ordered.Add(ordered[0]);

        return ordered;
    }


    private struct EdgeSegment
    {
        public Vector2Int cell;
        public int direction; // 0=up, 1=right, 2=down, 3=left

        public EdgeSegment(Vector2Int cell, int direction)
        {
            this.cell = cell;
            this.direction = direction;
        }
    }
}