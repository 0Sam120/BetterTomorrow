using UnityEngine;

public class AttackComponent : MonoBehaviour
{
    // Reference to this object's grid data
    GridObject gridObject;

    // Reference to the character's animation controller
    AnimationControlller characterAnimator;

    private void Awake()
    {
        // Get the GridObject component attached to this GameObject
        gridObject = GetComponent<GridObject>();

        // Get the animation controller from child objects
        characterAnimator = GetComponentInChildren<AnimationControlller>();
    }

    // Called when this character attacks a target on the grid
    public void AttackPosition(GridObject targetGridObject)
    {
        Debug.Log("Target Found");

        // Rotate this character to face the target
        RotateCharacter(targetGridObject.transform.position);

        // Play the attack animation
        characterAnimator.Attack();
    }

    // Rotates the character to face towards a world position
    private void RotateCharacter(Vector3 towards)
    {
        // Calculate direction to the target
        Vector3 direction = (towards - transform.position).normalized;

        // Prevent rotation on the Y axis (keep character upright)
        direction.y = 0;

        // Apply the rotation to face the target
        transform.rotation = Quaternion.LookRotation(direction);
    }
}

