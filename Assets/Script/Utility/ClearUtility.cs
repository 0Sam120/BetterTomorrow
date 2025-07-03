using UnityEngine;

public class ClearUtility : MonoBehaviour
{
    // Reference to the pathfinding system
    [SerializeField] Pathfinding targetPF;

    // Reference to the grid renderer used for attack highlights
    [SerializeField] GridRenderer attackHighlight;

    // Reference to the grid renderer used for movement highlights
    [SerializeField] GridRenderer moveHighlight;

    // Clears the pathfinding nodes (used to reset movement paths)
    public void ClearPathfinding()
    {
        targetPF.ClearNodes();
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
        ClearPathfinding();
        ClearGridHighlightMove();
        ClearGridHighlightAttack();
    }
}
