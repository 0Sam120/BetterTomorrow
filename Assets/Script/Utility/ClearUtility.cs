using System;
using System.Collections.Generic;
using UnityEngine;

public class ClearUtility
{
    GridRenderer highlight = CommandInput.Instance.GetRenderer();

    private static ClearUtility instance = null;
    private static readonly object lockObj = new object();

    public static ClearUtility Instance
    {
        get
        {
            lock (lockObj)
            {
                if (instance == null)
                {
                    instance = new ClearUtility();
                }
                return instance;
            }
        }
    }

    // Hides the grid highlight for movement range
    public void ClearGridHighlight()
    {
        highlight.Hide();
        Debug.Log("Move highlight cleared.");
    }

    // Fully clears all pathfinding and grid highlights (attack and move)
    public void FullClear()
    {
        ClearGridHighlight();
        Debug.Log("Full clear executed. All highlights cleared.");
    }
}
