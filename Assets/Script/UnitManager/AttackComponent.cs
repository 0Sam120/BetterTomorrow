using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AttackComponent : MonoBehaviour
{
    // Reference to the character's animation controller
    AnimationControlller characterAnimator;
    Character character;


    private void Awake()
    {
        character = GetComponent<Character>();
        characterAnimator = GetComponentInChildren<AnimationControlller>();
    }

    // Called when this character attacks a target on the grid
    public void AttackPosition(GridObject targetGridObject, int total)
    {
        Character targetCharacter = targetGridObject.GetComponent<Character>();

        var weapon = character.GetComponent<GearComponent>().weaponData;

        // Rotate this character to face the target
        RotateCharacter(targetCharacter.transform.position);

        int effectiveAC = targetCharacter.GetEffectiveAC(character.transform.position) - weapon.penetration;
        Debug.Log($"{character.Name} attacks {targetCharacter.Name} with a total of {total} against AC {effectiveAC}");

        if (total >= effectiveAC)
        {
            // If the attack hits, deal damage
            int totalDamage = 0;

            for (int i = 0; i < weapon.dieCount; i++)
            {
                int damage = Random.Range(1, weapon.damageDie) + character.DMGMod;
                totalDamage += damage;
            }

            totalDamage = character.GetComponent<CharacterTurn>().CheckMomentum(totalDamage, weapon.damageType, character);

            targetCharacter.TakeDamage(totalDamage, weapon.damageType);
            Debug.Log($"{character.Name} attacks {targetCharacter.Name} for {totalDamage} damage!");
        }
        else
        {
            Debug.Log($"{character.Name} attacks {targetCharacter.Name} but misses!");
        }

        // Play the attack animation
        characterAnimator.Attack();

        ReturnToStartPosition();
    }

    // Rotates the character to face towards a world position
    private void RotateCharacter(Vector3 towards)
    {
        // Calculate direction to the target
        Vector3 direction = (towards - character.transform.position).normalized;

        // Prevent rotation on the Y axis (keep character upright)
        direction.y = 0;

        // Apply the rotation to face the target
        character.transform.rotation = Quaternion.LookRotation(direction);
    }

    public float CalculateHitChance(GridObject target)
    {
        int effectiveAC = target.GetComponent<Character>().GetEffectiveAC(character.transform.position);
        float hitChance;
        
        if(target == null || target.GetComponent<Character>() == null)
        {
            Debug.LogError("Target is null or does not have a Character component.");
            return 0f; // No hit chance if target is invalid
        }

        if(character == null)
        {
            Debug.LogError("Character is not assigned.");
            return 0f; // No hit chance if character is invalid
        }

        hitChance = Mathf.Max(0, Mathf.Min(95, (21+ character.atkMod - effectiveAC) * 5));

        return hitChance;
    }

    public string CalculateExpectedDamage()
    {
        int minDamage = 1 + character.DMGMod; // Minimum damage is 1 + modifier
        int maxDamage = character.GetComponent<GearComponent>().weaponData.damageDie + character.DMGMod; // Maximum damage is DMG + modifier

        string expectedDamage = $"{minDamage}-{maxDamage};";

        return expectedDamage;
    }

    public float CalculateCritChance()
    {
        // Critical hit chance is 5% for a roll of 20
        return 5f; // Return as percentage
    }

    public void ReturnToStartPosition()
    {
        TargetingUI.Instance.Hide();
        CameraHelper.Instance.ToggleGameCamera();
    }
}

