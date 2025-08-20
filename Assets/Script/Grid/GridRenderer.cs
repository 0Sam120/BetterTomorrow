using System.Collections.Generic;
using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    GridMap grid;
    [SerializeField] GameObject highlightPoint;
    [SerializeField] GameObject container;

    // Cache of all spawned highlights
    Dictionary<Vector2Int, GameObject> highlights;

    // Colors
    [SerializeField] Color normalColor = Color.white;
    [SerializeField] Color hoverColor = Color.yellow;

    private void OnEnable() => CursorData.OnCursorMoved += UpdateHover;
    private void OnDisable() => CursorData.OnCursorMoved -= UpdateHover;

    private void Awake()
    {
<<<<<<< Updated upstream
        grid = GetComponentInParent<GridMap>(); // Get the GridMap component attached to the same GameObject
        highlightPointGO = new List<GameObject>(); // Initialize the list of move points
=======
        grid = GridMap.Instance;
        highlights = new Dictionary<Vector2Int, GameObject>();
>>>>>>> Stashed changes
    }

    private GameObject CreatePointHighlightObject(Vector2Int gridPos)
    {
        GameObject go = Instantiate(highlightPoint, container.transform);
        highlights[gridPos] = go; // store in dictionary
        return go;
    }

    public void FieldHighlight(List<Vector2Int> positions)
    {
        foreach (var pos in positions)
        {
            GameObject highlight = GetOrCreateHighlight(pos);
            PositionHighlight(highlight, pos);
            SetHighlightColor(highlight, normalColor);
            highlight.SetActive(true);
        }
    }

    internal void Hide()
    {
        foreach (var kvp in highlights)
        {
            kvp.Value.SetActive(false);
        }
    }

    private GameObject GetOrCreateHighlight(Vector2Int pos)
    {
        if (highlights.TryGetValue(pos, out var existing))
            return existing;

        return CreatePointHighlightObject(pos);
    }

    private void PositionHighlight(GameObject highlightObject, Vector2Int pos)
    {
        Vector3 position = grid.GetWorldPosition(pos.x, pos.y, true);
        position += Vector3.up * 0.2f;
        highlightObject.transform.position = position;
    }

    public void UpdateHover(Vector2Int cursorGridPos)
    {
        // Reset all to normal
        foreach (var kvp in highlights)
        {
            SetHighlightColor(kvp.Value, normalColor);
        }

        // Color only the hovered one
        if (highlights.TryGetValue(cursorGridPos, out var hoveredHighlight))
        {
            SetHighlightColor(hoveredHighlight, hoverColor);
        }
    }

    private void SetHighlightColor(GameObject go, Color color)
    {
        // Works for either MeshRenderer or SpriteRenderer
        if (go.TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material.color = color;
        }
        else if (go.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
        {
            spriteRenderer.color = color;
        }
    }
}
