using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static TurnManager;

public enum AIType
{
    Aggressive,
    Defensive,
    Cowardly,
    Opportunistic,
    Support
}

public class AIManager : MonoBehaviour
{
    [SerializeField] GridMap targetGrid;
    Character thisUnit;
    int optimalRange;
    int maxRange;
    int minRange;
    
    public void HandleAITurn(Character unit)
    {
        thisUnit = unit;
        maxRange = thisUnit.atkRange;
        minRange = 1;
        optimalRange = Mathf.CeilToInt(maxRange / 2);
        WellnessCheck();
    }

    private void WellnessCheck()
    {
        if (thisUnit.HP < 20)
        {
            Debug.Log("Unit HP is low, seeking cover.");
            SeekCover();
        }
        else
        {
            Debug.Log("Unit HP is sufficient, proceeding with normal actions.");
            // Add logic for normal actions here
            if (GetEnemiesInRange() != null)
            {
                Debug.Log("Enemies in range, preparing to attack.");
                PerformAttack();
            }
            else
            {
                Debug.Log("No enemies in range, seeking cover or moving.");
                SeekCover(); // Or implement a move action
            }
        }
    }

    private void SeekCover()
    {
        foreach(Character otherUnit in GetEnemiesInRange())
        {
            if (otherUnit != thisUnit)
            {
                Vector2Int unitPos = thisUnit.GetComponent<GridObject>().positionOnGrid;
                Vector2Int targetPos = otherUnit.GetComponent<GridObject>().positionOnGrid;
                Debug.Log("Enemy Unit Found: " + otherUnit.name + " at position: " + targetPos);
                if (IsInRange(unitPos, targetPos, otherUnit.atkRange))
                {
                    Debug.Log("In Range of Enemy: " + otherUnit.name);
                    Debug.Log("Seeking cover from " + otherUnit.name);
                }
            }
        }
    }

    private void MoveToPosition()
    {
        // Implement logic to move the unit to a safe position
        // This could involve pathfinding to a position that is not in range of enemies
        Debug.Log("Moving to a safe position.");
        // Example: thisUnit.GetComponent<CharacterTurn>().MoveTo(safePosition);
        // Ensure the safePosition is calculated based on the grid and enemy positions
    }

    private void PerformAttack()
    {
        List<Character> enemiesInRange = GetEnemiesInRange();
        if (enemiesInRange.Count > 0 && CanIAct(thisUnit, 2))
        {
            Character targetEnemy = enemiesInRange[0]; // For simplicity, target the first enemy in range
            Debug.Log("Attacking enemy: " + targetEnemy.name);
            // Add attack logic here, e.g., targetEnemy.TakeDamage(thisUnit.atkDamage);
            int total = thisUnit.RollToHit();
            thisUnit.GetComponent<AttackComponent>().AttackPosition(targetEnemy.GetComponent<GridObject>(), total);
        }
        else
        {
            Debug.Log("No enemies in range to attack.");
        }
    }

    Vector2Int CalculateBestMovePosition()
    {
        Vector2Int bestPos = thisUnit.GetComponent<GridObject>().positionOnGrid;
        int bestScore = int.MinValue;

        List<Vector2Int> walkableTiles = GetWalkableTilesInRange(thisUnit, (int)thisUnit.MaxMoveSpeed);
        List<PathNode> walkableNodes = new List<PathNode>();
        Character targetEnemy = GetEnemiesInRange()[0];

        foreach(Vector2Int tile in walkableTiles)
        {
            
        }

        return bestPos;
    }

    List<Vector2Int> GetWalkableTilesInRange(Character unit, int moveRange)
    {
        Vector2Int startPos = unit.GetComponent<GridObject>().positionOnGrid;
        List<Vector2Int> walkableTiles = new List<Vector2Int>();

        for (int x = -moveRange; x <= moveRange; x++)
        {
            for (int y = -moveRange; y <= moveRange; y++)
            {
                Vector2Int tilePos = new Vector2Int(startPos.x + x, startPos.y + y);

                if (!targetGrid.CheckBoundry(tilePos)) continue;
                if (!targetGrid.CheckWalkable(tilePos)) continue;

                int distance = Mathf.Abs(x) + Mathf.Abs(y);
                if (distance <= moveRange)
                {
                    walkableTiles.Add(tilePos);
                }
            }
        }

        return walkableTiles;
    }


    public List<Character> GetEnemiesInRange()
    {
        List<Character> inRangeCharacters = new List<Character>();

        foreach (Character otherUnit in UnitRegistry.AllUnits)
        {
            if (otherUnit.team != thisUnit.team && otherUnit != thisUnit)
            {
                Vector2Int unitPos = thisUnit.GetComponent<GridObject>().positionOnGrid;
                Vector2Int targetPos = otherUnit.GetComponent<GridObject>().positionOnGrid;
                Debug.Log(Equals(otherUnit, thisUnit) ? "Same Unit" : "Enemy Unit Found: " + otherUnit.name + " at position: " + targetPos);

                if (IsInRange(unitPos, targetPos, thisUnit.atkRange))
                {
                    inRangeCharacters.Add(otherUnit);
                }

                Debug.Log(IsInRange(unitPos, targetPos, thisUnit.atkRange) ? "In Range" : "Out of Range");
            }
        }

        return inRangeCharacters;
    }

    public bool IsInRange(Vector2Int unitPos, Vector2Int targetPos, int range)
    {
        int distance = Mathf.Abs(unitPos.x - targetPos.x) + Mathf.Abs(unitPos.y - targetPos.y);
        return distance <= range;
    }

    internal bool CanIAct(Character unit, int momentumCost)
    {
        if(unit.GetComponent<CharacterTurn>().Momentum < momentumCost)
        {
            Debug.Log("Unit has no momentum to act.");
            return false;
        }

        return true;
    }

    private int ScoreTile(Vector2Int tilePos, Character targetUnit)
    {
        int score = 0;
        Vector2Int targetPos = targetUnit.GetComponent<GridObject>().positionOnGrid;

        int distance = Mathf.Abs(tilePos.x - targetPos.x) + Mathf.Abs(tilePos.y - targetPos.y);

        if(distance == optimalRange)
        {
            score += 10;
        }
        else
        {
            score -= Mathf.Abs(distance - optimalRange);
        }

        return score;
    }
}
