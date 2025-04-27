using System.Collections.Generic;
using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    GridMap grid;
    [SerializeField] GameObject movePoint;
    List<GameObject> movePointGo;

    private void Start()
    {
        grid = GetComponent<GridMap>();
        movePointGo = new List<GameObject>();
    }
}
