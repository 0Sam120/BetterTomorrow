using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public enum CoverType
{
    None,
    Half,
    Full
}

public enum CoverDirection
{
    North,
    South,
    East,
    West
}

// Represents a single cell/node in the grid
public class Node
{
    public Vector2Int position; // Position of this node in the grid (X, Y coordinates)
    public GridObject gridObject; // Reference to any object placed on this node
    public Dictionary<CoverDirection, CoverType> coverData; // Cover data for each direction
    public float elevation; // Height (Y-axis value) of the terrain at this node
    public bool passable; // Whether this node can be walked on or not

    public Node(Vector2Int pos)
    {
        position = pos;
        coverData = new Dictionary<CoverDirection, CoverType>();
        // Initialize all directions with no cover
        foreach (CoverDirection dir in System.Enum.GetValues(typeof(CoverDirection)))
        {
            coverData[dir] = CoverType.None;
        }
    }
}

