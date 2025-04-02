using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    //Width of the grid
    [SerializeField] private int width = 30;
    //Length of the grid
    [SerializeField] private int length = 30;
    //Size of the cells
    [SerializeField] private float cellSize = 1f;
    //Color of the grid lines
    [SerializeField] private Color gridColor = Color.white;

    private void Start()
    {
    // Draws the grid when the script starts
        DrawGrid();
    }

    private void DrawGrid()
    {
    // Creates game object to hold all the grid lines
        GameObject gridLines = new GameObject("GridLines");
        gridLines.transform.SetParent(transform);
   // Draws vertical grid lines along the X-axis
        for (int x = 0; x <= length; x++)
        {
            CreateLine(new Vector3(x * cellSize, 0, 0), new Vector3(x * cellSize, 0, width * cellSize), gridLines);
        }
    // Draws vertical grid lines along the Z-axis
        for (int y = 0; y <= width; y++)
        {
            CreateLine(new Vector3(0, 0, y * cellSize), new Vector3(length * cellSize, 0, y * cellSize), gridLines);
        }
    }
    // Adds a linerender to render the line
    private void CreateLine(Vector3 start, Vector3 end, GameObject parent)
    {
    // Creates game object for the gridline
        GameObject lineObj = new GameObject("GridLine");
        lineObj.transform.SetParent(parent.transform);
    // Adds a LineRenderer component to render the line
        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        // Sets line width
        line.startWidth = 0.05f;
        line.endWidth = 0.05f;
        // Defines the line's start and end positions
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        // Assigns a material to the LineRenderer (default sprite shader)
        line.material = new Material(Shader.Find("Sprites/Default"));
        // Sets the line color
        line.startColor = gridColor;
        line.endColor = gridColor;
    }
}
