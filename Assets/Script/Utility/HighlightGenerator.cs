using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public enum HighlightShape { Single, Square, Circle, Line, Cone }

public class HighlightGenerator : MonoBehaviour
{
    private List<GameObject> activeHighlights = new();

    public static HighlightGenerator Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowHighlight(HighlightShape shape, Vector2Int origin, Vector2Int target, GameObject highlightPrefab, Transform highlightParent, int radiusOrRange = 3, List<Vector2Int> validCells = null)
    {
        ClearHighlights();
        List<Vector2Int> cells = GetShapeCells(shape, origin, target, radiusOrRange);

        if(validCells != null)
        {
            cells = cells.Where(cells => validCells.Contains(cells)).ToList();
        }

        foreach (var cell in cells)
            CreateHighlightAt(cell, highlightPrefab, highlightParent);
    }

    private void CreateHighlightAt(Vector2Int cell, GameObject highlightPrefab, Transform highlightParent)
    {
        Vector3 pos = GridMap.Instance.GetWorldPosition(cell.x, cell.y);
        pos.y += 0.05f;
        Quaternion rotation = Quaternion.Euler(90f, 0f, 0f);
        GameObject highlight = Instantiate(highlightPrefab, pos, rotation, highlightParent);
        activeHighlights.Add(highlight);
    }

    public void ClearHighlights()
    {
        foreach (var obj in activeHighlights)
        {
            if (obj != null)
                Destroy(obj);
        }
        activeHighlights.Clear();
    }

    public List<Vector2Int> GetShapeCells(HighlightShape shape, Vector2Int origin, Vector2Int target, int radiusOrRange)
    {
        switch (shape)
        {
            case HighlightShape.Single: return new() { origin };
            case HighlightShape.Square: return GenerateSquare(origin, radiusOrRange);
            case HighlightShape.Circle: return GenerateCircle(origin, radiusOrRange);
            case HighlightShape.Line: return GenerateLine(origin, target, radiusOrRange);
            case HighlightShape.Cone: return GenerateCone(origin, target, radiusOrRange, 60f);
            default: return new();
        }
    }

    private List<Vector2Int> GenerateSquare(Vector2Int center, int radius)
    {
        List<Vector2Int> cells = new();
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                cells.Add(center + new Vector2Int(x, y));
            }
        }
        return cells;
    }

    private List<Vector2Int> GenerateCircle(Vector2Int center, int radius)
    {
        List<Vector2Int> cells = new();
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                float distance = Mathf.Sqrt(x * x + y * y);
                if (distance <= radius + 0.5f) // small fudge for visual fullness
                {
                    cells.Add(center + new Vector2Int(x, y));
                }
            }
        }
        return cells;
    }

    private List<Vector2Int> GenerateLine(Vector2Int start, Vector2Int target, int maxRange)
    {
        List<Vector2Int> cells = new();

        Vector2Int dir = target - start;
        Vector2 floatDir = new Vector2(dir.x, dir.y);
        Vector2 normalizedDir = floatDir.normalized;
        float dist = dir.magnitude;

        // Clamp line length
        Vector2Int end = start + Vector2Int.RoundToInt(normalizedDir * Mathf.Min(dist, maxRange));

        int x0 = start.x, y0 = start.y;
        int x1 = end.x, y1 = end.y;
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            cells.Add(new Vector2Int(x0, y0));
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }

        return cells;
    }

    private List<Vector2Int> GenerateCone(Vector2Int origin, Vector2Int target, int range, float coneAngle)
    {
        List<Vector2Int> cells = new();

        Vector2 dir = (target - origin);
        if (dir == Vector2.zero) dir = Vector2.up; // fallback so you don't divide by zero
        dir.Normalize();

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                Vector2Int cell = origin + new Vector2Int(x, y);
                Vector2 toCell = (Vector2)cell - (Vector2)origin;
                float dist = toCell.magnitude;
                if (dist > range || dist < 0.5f) continue;

                float angle = Vector2.Angle(dir, toCell.normalized);
                if (angle <= coneAngle * 0.5f)
                {
                    cells.Add(cell);
                }
            }
        }

        return cells;
    }

    public List<Vector2Int> CalculateSkillArea(Character owner, int range)
    {
        List<Vector2Int> rangeArea = new List<Vector2Int>();
        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                if (Mathf.Abs(x) + Mathf.Abs(y) <= range)
                {
                    Vector2Int cell = owner.GetComponent<GridObject>().positionOnGrid + new Vector2Int(x, y);
                    if (GridMap.Instance.CheckBoundry(cell))
                    {
                        rangeArea.Add(cell);
                    }
                }
            }
        }

        return rangeArea;

    }
}
