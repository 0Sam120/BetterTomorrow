using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GridRenderer : MonoBehaviour
{
    [SerializeField] private GameObject highlightPrefab;
    [SerializeField] private Transform highlightParent;
    private GameObject selectedCellHighlight;
    private GameObject coverParent;
    private LineRenderer lineRenderer;

    private List<Vector2Int> currentMovementArea = new List<Vector2Int>();

    private int currentHighlightRadius = 2;
    private int currentHighlightRange = 5;
    private bool isInitialized = false;

    private void OnEnable() => CursorData.OnCursorMoved += HighlightSelectedCell;
    private void OnDisable() => CursorData.OnCursorMoved -= HighlightSelectedCell;

    public TargetType currentTargetType;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        lineRenderer.loop = true;
        lineRenderer.widthMultiplier = 0.05f;
        currentTargetType = TargetType.Ally;
    }

    public void Hide()
    {
        isInitialized = false;

        // Clear selected cell highlight
        HighlightGenerator.Instance.ClearHighlights();

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

    public void SetHighlightParameters(int radius, int range)
    {
        currentHighlightRadius = radius;
        currentHighlightRange = range;
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
            switch (currentTargetType)
            {
                case TargetType.Ally:
                    HighlightGenerator.Instance.ShowHighlight(HighlightShape.Single, selectedCell, selectedCell, highlightPrefab, highlightParent);
                    GridMap.Instance.CreateCoverVisualsForTile(selectedCell, coverParent.transform);
                    break;
                case TargetType.Area:
                    HighlightGenerator.Instance.ShowHighlight(HighlightShape.Square, selectedCell, selectedCell, highlightPrefab, highlightParent, currentHighlightRadius, currentMovementArea);
                    break;
                case TargetType.Circle:
                    HighlightGenerator.Instance.ShowHighlight(HighlightShape.Circle, selectedCell, selectedCell, highlightPrefab, highlightParent, currentHighlightRadius, currentMovementArea);
                    break;
                case TargetType.Line:
                    HighlightGenerator.Instance.ShowHighlight(HighlightShape.Line, currentMovementArea[0], selectedCell, highlightPrefab, highlightParent, currentHighlightRange, currentMovementArea);
                    break;
                case TargetType.Cone:
                    HighlightGenerator.Instance.ShowHighlight(HighlightShape.Cone, currentMovementArea[0], selectedCell, highlightPrefab, highlightParent, currentHighlightRange, currentMovementArea);
                    break;
                case TargetType.Self:
                    HighlightGenerator.Instance.ClearHighlights();
                    break;
                case TargetType.None:
                    HighlightGenerator.Instance.ClearHighlights();
                    break;
                default:
                    HighlightGenerator.Instance.ClearHighlights();
                    break;
            }
        }
        else
        {
            // Clear highlights when cursor is outside the movement area
            HighlightGenerator.Instance.ClearHighlights();
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

    public List<Vector2Int> GetTargets()
    {
        List<Vector2Int> targets = new List<Vector2Int>();
        GridMap map = GridMap.Instance;

        if(TurnManager.Instance.currentSkill.targeting == TargetType.Self)
        {
            targets.Add(map.GetGridPosition(TurnManager.Instance.currentUnit.transform.position));
            Debug.Log($"Added self target at {targets[0]}");
        }
        else
        {
            foreach (Transform t in highlightParent)
            {
                Vector3 pos = t.position;
                if (map.CheckBoundry((int)pos.x, (int)pos.y))
                {
                    Vector2Int tile = map.GetGridPosition(pos);
                    targets.Add(tile);
                }
            }
        }

        Debug.Log(targets.Count);

        return targets;
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