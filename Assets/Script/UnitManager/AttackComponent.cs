using UnityEngine;

public class AttackComponent : MonoBehaviour
{
    // Reference to this object's grid data
    GridObject gridObject;

    // Reference to the character's animation controller
    AnimationControlller characterAnimator;
    Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
        // Get the GridObject component attached to this GameObject
        gridObject = GetComponent<GridObject>();

        // Get the animation controller from child objects
        characterAnimator = GetComponentInChildren<AnimationControlller>();
    }

    // Called when this character attacks a target on the grid
    public void AttackPosition(GridObject targetGridObject, int total)
    {
        Character targetCharacter = targetGridObject.GetComponent<Character>();

        // Rotate this character to face the target
        RotateCharacter(targetCharacter.transform.position);

        int effectiveAC = targetCharacter.GetEffectiveAC(transform.position);
        Debug.Log($"{character.Name} attacks {targetCharacter.Name} with a total of {total} against AC {effectiveAC}");

        if (total >= effectiveAC)
        {
            // If the attack hits, deal damage
            int damage = Random.Range(1, character.DMG) + character.DMGMod;
            targetCharacter.TakeDamage(damage);
            Debug.Log($"{character.Name} attacks {targetCharacter.Name} for {damage} damage!");
        }
        else
        {
            Debug.Log($"{character.Name} attacks {targetCharacter.Name} but misses!");
        }

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

