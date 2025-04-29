using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Represents a single cell/node in the grid
public class Node
{
    public bool passable; // Whether this node can be walked on or not
    public GridObject gridObject; // Reference to any object placed on this node
    public float elevation; // Height (Y-axis value) of the terrain at this node
}
