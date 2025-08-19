using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterAttack
{
    List<Vector2Int> attackPosition;
    
    // Calculates all grid positions within the character's attack range
    public void CalculateAttackArea(Vector2Int characterPositionOnGrid, int attackRange)
    {

        var highlight = CommandInput.Instance.GetAttackPointRenderer();
        
        // Initialize or clear the attackPosition list
        if (attackPosition == null)
        {
            attackPosition = new List<Vector2Int>();
        }
        else
        {
            attackPosition.Clear();
        }

        // Loop through a square area around the character based on attack range
        for (int x = -attackRange; x <= attackRange; x++)
        {
            for (int y = -attackRange; y <= attackRange; y++)
            {
                // Skip positions outside of Manhattan distance range
                if (Mathf.Abs(x) + Mathf.Abs(y) > attackRange) { continue; }

                // Skip the character's own tile
                if (x == 0 && y == 0) { continue; }

                // Check if this grid position is within the grid boundaries
                if (GridMap.Instance.CheckBoundry(characterPositionOnGrid.x + x, characterPositionOnGrid.y + y))
                {
                    // Add valid attack position
                    attackPosition.Add(new Vector2Int(characterPositionOnGrid.x + x, characterPositionOnGrid.y + y));
                }
            }
        }

        // Highlight all valid attack tiles
        highlight.fieldHighlight(attackPosition);
    }

    // Returns the grid object located at the given grid position
    internal GridObject GetAttackTarget(Vector2Int positionOnGrid)
    {
        GridObject target = GridMap.Instance.GetPlacedObject(positionOnGrid);
        return target;
    }

    // Checks if the given grid position is within the calculated attack area
    internal bool Check(Vector2Int positionOnGrid)
    {
        return attackPosition.Contains(positionOnGrid);
    }

}
