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
        // Simple approach: remove duplicates and sort by position
        // For a more robust solution, you'd want to properly connect the edge segments
        HashSet<Vector3> uniquePoints = new HashSet<Vector3>();
        foreach (var point in points)
        {
            // Round to avoid floating point precision issues
            Vector3 rounded = new Vector3(
                Mathf.Round(point.x * 100f) / 100f,
                point.y,
                Mathf.Round(point.z * 100f) / 100f
            );
            uniquePoints.Add(rounded);
        }

        List<Vector3> result = new List<Vector3>(uniquePoints);

        // Sort points to form outline (simplified approach)
        if (result.Count > 2)
        {
            Vector3 centroid = Vector3.zero;
            foreach (var point in result)
                centroid += point;
            centroid /= result.Count;

            result.Sort((a, b) => {
                float angleA = Mathf.Atan2(a.z - centroid.z, a.x - centroid.x);
                float angleB = Mathf.Atan2(b.z - centroid.z, b.x - centroid.x);
                return angleA.CompareTo(angleB);
            });
        }

        return result;
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