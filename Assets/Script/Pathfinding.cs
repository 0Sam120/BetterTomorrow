using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    public int pos_x;
    public int pos_y;

    public float gValue;
    public float hValue;
    public PathNode parentNode;

    public float fValue { get { return gValue +  hValue; } }

    public PathNode(int xPos, int yPos)
    {
        pos_x = xPos;
        pos_y = yPos;
    }
}

[RequireComponent (typeof(GridMap))]
public class Pathfinding : MonoBehaviour
{
    GridMap gridMap;
    PathNode[,] pathNodes;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        if (gridMap == null) { gridMap = GetComponent<GridMap>(); }

        pathNodes = new PathNode[gridMap.length, gridMap.width];

        for (int x = 0; x < gridMap.length; x++)
        {
            for (int y = 0; y < gridMap.width; y++)
            {
                pathNodes[x, y] = new PathNode(x, y);
            }
        }
    }

    public List<PathNode> FindPath(int startX, int startY, int endX, int endY)
    {
        PathNode startNode = pathNodes[startX, startY];
        PathNode endNode = pathNodes[endX, endY];

        List<PathNode> openList = new List<PathNode>();
        List<PathNode> closedList = new List<PathNode>();

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            PathNode currentList = openList[0];

            for(int i = 0; i < openList.Count; i++)
            {
                if(currentList.fValue > openList[i].fValue)
                {
                    currentList = openList[i];
                }

                if(currentList.fValue == openList[i].fValue && currentList.hValue > openList[i].hValue)
                {
                    currentList = openList[i];
                }
            }

            openList.Remove(currentList);
            closedList.Add(currentList);

            if (currentList == endNode)
            {
                return RetracePath(startNode, endNode);    
            }

            List<PathNode> neighbourNodes = new List<PathNode>();
            for(int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    if(x == 0 && y == 0) { continue; }
                    if (gridMap.CheckBoundry(currentList.pos_x + x, currentList.pos_y + y) == false) { continue; }

                    neighbourNodes.Add(pathNodes[currentList.pos_x + x, currentList.pos_y + y]);
                }    
            }

            for (int i = 0; i < neighbourNodes.Count; i++)
            { 
                if(closedList.Contains(neighbourNodes[i])) { continue; }
                if (gridMap.CheckWalkable(neighbourNodes[i].pos_x, neighbourNodes[i].pos_y) == false) { continue; }

                float movmentCost = currentList.gValue + CalculateDistance(currentList, neighbourNodes[i]);

                if(openList.Contains(neighbourNodes[i]) == false || movmentCost < neighbourNodes[i].gValue)
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

        return null;
    }

    private int CalculateDistance(PathNode currentList, PathNode target)
    {
        int distX = Mathf.Abs(currentList.pos_x - target.pos_x);
        int distY = Mathf.Abs(currentList.pos_y - target.pos_y);

        if (distX > distY) { return 14 * distY + 10 * (distX - distY); }
        return 14 * distX + 10 * (distY - distX);
    }

    private List<PathNode> RetracePath(PathNode startNode, PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();

        PathNode currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parentNode;
        }
        path.Reverse();

        return path;
    }
}
