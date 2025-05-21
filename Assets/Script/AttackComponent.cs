using UnityEngine;

public class AttackComponent : MonoBehaviour
{
    GridObject gridObject;
    AnimationControlller characterAnimator;

    private void Awake()
    {
        gridObject = GetComponent<GridObject>();
        characterAnimator = GetComponentInChildren<AnimationControlller>();
    }

    public void AttackPosition(GridObject targetGridObject)
    {
        Debug.Log("Target Found");
        RotateCharacter(targetGridObject.transform.position);
        characterAnimator.Attack();
    }

    private void RotateCharacter(Vector3 towards)
    {
        Vector3 direction = (towards - transform.position).normalized;
        direction.y = 0;
        transform.rotation = Quaternion.LookRotation(direction);
    }
}
