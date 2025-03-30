using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    [SerializeField] private int width = 30;
    [SerializeField] private int length = 30;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Color gridColor = Color.white;

    private void Start()
    {
        DrawGrid();
    }

    private void DrawGrid()
    {
        GameObject gridLines = new GameObject("GridLines");
        gridLines.transform.SetParent(transform);

        for (int x = 0; x <= length; x++)
        {
            CreateLine(new Vector3(x * cellSize, 0, 0), new Vector3(x * cellSize, 0, width * cellSize), gridLines);
        }
        for (int y = 0; y <= width; y++)
        {
            CreateLine(new Vector3(0, 0, y * cellSize), new Vector3(length * cellSize, 0, y * cellSize), gridLines);
        }
    }

    private void CreateLine(Vector3 start, Vector3 end, GameObject parent)
    {
        GameObject lineObj = new GameObject("GridLine");
        lineObj.transform.SetParent(parent.transform);
        LineRenderer line = lineObj.AddComponent<LineRenderer>();

        line.startWidth = 0.05f;
        line.endWidth = 0.05f;
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);

        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = gridColor;
        line.endColor = gridColor;
    }
}
