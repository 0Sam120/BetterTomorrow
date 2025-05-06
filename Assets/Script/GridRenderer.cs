using System.Collections.Generic;
using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    GridMap grid; // Reference to the GridMap component
    [SerializeField] GameObject movePoint; // Prefab for move point visualization
    [SerializeField] GameObject movePointContainer;
    List<GameObject> movePointGo; // List to hold spawned move point GameObjects


    private void Awake()
    {
        grid = GetComponent<GridMap>(); // Get the GridMap component attached to the same GameObject
        movePointGo = new List<GameObject>(); // Initialize the list of move points
    }

    private GameObject CreateMovePointHighlightObject()
    {
        GameObject go = Instantiate(movePoint);
        movePointGo.Add(go);
        go.transform.SetParent(movePointContainer.transform);
        return go;
    }

    public void fieldHighlight(List<Vector2Int> position)
    {
        for (int i = 0; i < position.Count; i++)
        {
            Highlight(position[i].x, position[i].y, GetMovePointGO(i));
        }
    }

    public void fieldHighlight(List<PathNode> position)
    {
        for (int i = 0; i < position.Count; i++)
        {
            Highlight(position[i].pos_x, position[i].pos_y, GetMovePointGO(i));
        }
    }

    private GameObject GetMovePointGO(int i)
    {
        if(movePointGo.Count < i)
        {
            return movePointGo[i];
        }

        GameObject newHighlightObject = CreateMovePointHighlightObject();
        return newHighlightObject;
    }

    private void Highlight(int posX, int posY, GameObject highlightObject)
    {
        Vector3 position = grid.GetWorldPosition(posX, posY, true);
        position += Vector3.up * 0.2f;
        highlightObject.transform.position = position;
    }
}
