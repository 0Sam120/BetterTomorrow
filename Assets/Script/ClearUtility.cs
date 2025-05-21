using UnityEngine;

public class ClearUtility : MonoBehaviour
{
    [SerializeField] Pathfinding targetPF;
    [SerializeField] GridRenderer attackHighlight;
    [SerializeField] GridRenderer moveHighlight;

    public void ClearPathfinding()
    {
        targetPF.ClearNodes();
    }

    public void ClearGridHighlightAttack()
    {
        attackHighlight.Hide();
    }

    public void ClearGridHighlightMove()
    {
        moveHighlight.Hide();
    }

    public void FullClear()
    {
        ClearPathfinding();
        ClearGridHighlightMove();
        ClearGridHighlightAttack();
    }
}
