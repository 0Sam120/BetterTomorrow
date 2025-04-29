using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

// Represents a node used for pathfinding (separate from the world Node class)
public class PathNode
{
    public int pos_x; // Grid X coordinate
    public int pos_y; // Grid Y coordinate

    public float gValue; // Cost from start node
    public float hValue; // Heuristic cost to end node
    public PathNode parentNode; // Link to parent node for path retracing

    public float fValue { get { return gValue + hValue; } } // Total cost

    public PathNode(int xPos, int yPos)
    {
        pos_x = xPos;
        pos_y = yPos;
    }
}

[RequireComponent(typeof(GridMap))]
public class Pathfinding : MonoBehaviour
{
    GridMap gridMap; // Reference to the grid system
    PathNode[,] pathNodes; // 2D array of PathNodes

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        if (gridMap == null) { gridMap = GetComponent<GridMap>(); }

        pathNodes = new PathNode[gridMap.length, gridMap.width];

        // Initialize PathNode array with corresponding grid coordinates
        for (int x = 0; x < gridMap.length; x++)
        {
            for (int y = 0; y < gridMap.width; y++)
            {
                pathNodes[x, y] = new PathNode(x, y);
            }
        }
    }

    // Finds a path from start position to end position
    public List<PathNode> FindPath(int startX, int startY, int endX, int endY)
    {
        PathNode startNode = pathNodes[startX, startY];
        PathNode endNode = pathNodes[endX, endY];

        List<PathNode> openList = new List<PathNode>(); // Nodes to be evaluated
        List<PathNode> closedList = new List<PathNode>(); // Nodes already evaluated

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            PathNode currentList = openList[0];

            // Find the node with the lowest f-cost
            for (int i = 0; i < openList.Count; i++)
            {
                if (currentList.fValue > openList[i].fValue)
                {
                    currentList = openList[i];
                }
                if (currentList.fValue == openList[i].fValue && currentList.hValue > openList[i].hValue)
                {
                    currentList = openList[i];
                }
            }

            openList.Remove(currentList);
            closedList.Add(currentList);

            // If the end node is reached, retrace and return the path
            if (currentList == endNode)
            {
                return RetracePath(startNode, endNode);
            }

            List<PathNode> neighbourNodes = new List<PathNode>();
            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    if (x == 0 && y == 0) { continue; } // Skip self
                    if (gridMap.CheckBoundry(currentList.pos_x + x, currentList.pos_y + y) == false) { continue; }

                    neighbourNodes.Add(pathNodes[currentList.pos_x + x, currentList.pos_y + y]);
                }
            }

            for (int i = 0; i < neighbourNodes.Count; i++)
            {
                if (closedList.Contains(neighbourNodes[i])) { continue; }
                if (gridMap.CheckWalkable(neighbourNodes[i].pos_x, neighbourNodes[i].pos_y) == false) { continue; }

                float movmentCost = currentList.gValue + CalculateDistance(currentList, neighbourNodes[i]);

                if (!openList.Contains(neighbourNodes[i]) || movmentCost < neighbourNodes[i].gValue)
                {
                    neighbourNodes[i].gValue = movmentCost;
                    neighbourNodes[i].hValue = CalculateDistance(neighbourNodes[i], endNode);
                    neighbourNodes[i].parentNode = currentList;

                    if (!openList.Contains(neighbourNodes[i]))
                    {
                        openList.Add(neighbourNodes[i]);
                    }
                }
            }
        }

        // If no path is found
        return null;
    }

    // Calculates the movement cost between two nodes (diagonal costs more)
    private int CalculateDistance(PathNode currentList, PathNode target)
    {
        int distX = Mathf.Abs(currentList.pos_x - target.pos_x);
        int distY = Mathf.Abs(currentList.pos_y - target.pos_y);

        if (distX > distY) { return 14 * distY + 10 * (distX - distY); }
        return 14 * distX + 10 * (distY - distX);
    }

    // Retraces the path by following parent nodes backwards
    private List<PathNode> RetracePath(PathNode startNode, PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();

        PathNode currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parentNode;
        }
        path.Reverse(); // So that it starts at startNode and ends at endNode

        return path;
    }
}
