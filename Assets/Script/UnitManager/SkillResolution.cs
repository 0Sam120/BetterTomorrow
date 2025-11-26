using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillResolution : MonoBehaviour
{
    private List<Character> validTargets = new List<Character>();

    public GridRenderer skillTargetRenderer;

    public void SkillTargeting(SkillsScriptableObject skill)
    {
        GetComponent<MoveUnit>().CheckWalkableTerrain(
            TurnManager.Instance.currentUnit.GetComponent<Character>(),
            skill.range
        );
        skillTargetRenderer.SetHighlightParameters(skill.areaOfEffect, skill.areaOfEffect);
        skillTargetRenderer.currentTargetType = skill.targeting;
        TurnManager.Instance.currentSkill = skill;
    }

    public List<Character> GetValidSkillTargets()
    {
        List<Vector2Int> cells = skillTargetRenderer.GetTargets();

        foreach (var cell in cells)
        {
            GridObject gridObject = GridMap.Instance.GetPlacedObject(cell);
            if (gridObject != null)
            {
                Debug.Log(gridObject.name);

                Character character = gridObject.GetComponent<Character>();
                if (character != null)
                {
                    validTargets.Add(character);
                }
            }
        }


        Debug.Log(validTargets.Count);
        return validTargets;
    }
}
