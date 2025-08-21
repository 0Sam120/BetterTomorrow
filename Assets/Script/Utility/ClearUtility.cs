using System;
using System.Collections.Generic;
using UnityEngine;

public class ClearUtility
{

    // Reference to the grid renderer used for attack highlights
    GridRenderer attackHighlight = CommandInput.Instance.GetAttackPointRenderer();

    // Reference to the grid renderer used for movement highlights
    GridRenderer moveHighlight = CommandInput.Instance.GetMovePointRenderer();

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

    // Hides the grid highlight for attack range
    public void ClearGridHighlightAttack()
    {
        attackHighlight.Hide();
    }

    // Hides the grid highlight for movement range
    public void ClearGridHighlightMove()
    {
        moveHighlight.Hide();
    }

    // Fully clears all pathfinding and grid highlights (attack and move)
    public void FullClear()
    {
        ClearGridHighlightMove();
        ClearGridHighlightAttack();
    }
}
