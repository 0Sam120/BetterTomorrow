using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterAttack
{
    CameraHelper helper;

    private List<GridObject> validTargets = new List<GridObject>();
    private int currentTargetIndex = 0;

    // Calculates all grid positions within the character's attack range
    public void CalculateAttackTargets(Vector2Int characterPos, int attackRange, CameraHelper cameraHelper)
    {
        Debug.Log("Start target search");
        
        var helper = cameraHelper;

        validTargets.Clear();

        // Loop through a square area around the character based on attack range
        for (int x = -attackRange; x <= attackRange; x++)
        {
            for (int y = -attackRange; y <= attackRange; y++)
            {
                // Skip positions outside of Manhattan distance range
                if (Mathf.Abs(x) + Mathf.Abs(y) > attackRange) continue; 

                // Skip the character's own tile
                if (x == 0 && y == 0) continue; 

                Vector2Int checkPos = characterPos + new Vector2Int(x, y);

                // Check if this grid position is within the grid boundaries
                if (!GridMap.Instance.CheckBoundry(characterPos.x + x, characterPos.y + y)) continue;

                GridObject gridObject = GridMap.Instance.GetPlacedObject(checkPos);
                if(gridObject != null && IsValidTarget(gridObject) && IsTargetVisible(characterPos, checkPos)) 
                {                     
                    Debug.Log($"Found target at {checkPos} for character at {characterPos}");
                    validTargets.Add(gridObject);
                }
            }
        }

        if(validTargets.Count > 0)
        {
            currentTargetIndex = 0; // Reset target index
            TargetingUI.Instance.SpawnIndicators(validTargets, this);
            TargetingUI.Instance.ShowForTarget(validTargets[currentTargetIndex]);
            helper.ToggleCinematicCamera();
            helper.SetCameraForAttack(validTargets[currentTargetIndex], true);
        }
    }

    private bool IsValidTarget(GridObject target)
    {
        // Check if the target is a character and not the same as the current character
        return target != null && target.GetComponent<Character>() != null;
    }

    private bool IsTargetVisible(Vector2Int from, Vector2Int to)
    {
        Vector3 fromWorld = GridMap.Instance.GetWorldPosition(from.x, from.y);
        fromWorld.y += 1.5f; // Raise the origin point slightly to avoid ground collision issues
        Vector3 toWorld = GridMap.Instance.GetWorldPosition(to.x, to.y);
        toWorld.y += 1.5f; // Raise the target point slightly to avoid ground collision issues

        Vector3 dir = (toWorld - fromWorld).normalized;
        float distance = Vector3.Distance(fromWorld, toWorld);

        if(Physics.Raycast(fromWorld, dir, distance, LayerMask.GetMask("Cover")))
        {
            return false; // There's an obstacle in the way
        }

        return true; // No obstacles, target is visible
    }

    public void SelectTarget(GridObject target)
    {
        int index = validTargets.IndexOf(target);
        if(index >= 0)
        {
            currentTargetIndex = index;
            TargetingUI.Instance.ShowForTarget(validTargets[currentTargetIndex]);
            helper.SetCameraForAttack(validTargets[currentTargetIndex], true);
        }
    }

    public void CancelAttack(CameraHelper cameraHelper)
    {
        helper = cameraHelper;

        TargetingUI.Instance.Hide();
        helper.ResetCameraPosition(true);
        helper.ToggleGameCamera();
    }

    // Returns the grid object located at the given grid position
    public GridObject GetCurrentTarget()
    {
        if (validTargets.Count == 0) return null;
        return validTargets[currentTargetIndex];
    }
}
