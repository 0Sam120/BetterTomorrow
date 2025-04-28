using System.Collections.Generic;
using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    GridMap grid; // Reference to the GridMap component
    [SerializeField] GameObject movePoint; // Prefab for move point visualization
    List<GameObject> movePointGo; // List to hold spawned move point GameObjects

    private void Start()
    {
        grid = GetComponent<GridMap>(); // Get the GridMap component attached to the same GameObject
        movePointGo = new List<GameObject>(); // Initialize the list of move points
    }
}
