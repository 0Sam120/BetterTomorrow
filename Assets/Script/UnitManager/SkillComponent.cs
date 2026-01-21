using UnityEngine;
using System.Collections.Generic;

public class SkillComponent : MonoBehaviour
{
    // Skills this unit knows
    public List<SkillsScriptableObject> knownSkills = new List<SkillsScriptableObject>();

    // Runtime state per skill (cooldowns, charges, etc.)
    private Dictionary<SkillsScriptableObject, SkillRuntimeData> skillStates = new Dictionary<SkillsScriptableObject, SkillRuntimeData>();
    private Character character;

    private void Awake()
    {
        
        character = GetComponent<Character>();
        
        // Initialize runtime data for each known skill
        foreach (var skill in knownSkills)
        {
            skillStates[skill] = new SkillRuntimeData(skill);
        }
    }

    // Tick cooldowns down by 1
    public void TickRound()
    {
        foreach (var data in skillStates.Values)
        {
            if (data.currentCooldown > 0)
                data.currentCooldown--;
        }
    }

    // Check if a skill can currently be used
    public bool CanUseSkill(SkillsScriptableObject skill)
    {
        if (!skillStates.ContainsKey(skill)) return false;

        var data = skillStates[skill];
        if (data.currentCooldown > 0) 
        {
            Debug.Log("Skill is on cooldown");
            return false;
        }
        
        if (data.limitedCharges && data.chargesLeft <= 0)
        {
            Debug.Log("Can't use a charge");
            return false;
        }

        if(character.AP < skill.cost)
        {
            Debug.Log("Not enough AP to use the skill");
            return false;
        }

        // May also add resource checks here (AP/mana/etc.)
        Debug.Log("You may now use your skill");
        return true;
    }

    // Use a skill (starts cooldown, reduces charges)
    public void UseSkill(SkillsScriptableObject skill, List<Character> targets)
    {
        if (!CanUseSkill(skill)) return;
        
        switch (skill.rollType)
        {
            case RollType.None:
                Debug.Log("No roll needed");
                ApplyEffect(skill, targets);
                break;
            case RollType.Save:
                Debug.Log("Making saves");
                foreach (var target in targets)
                {
                    if(target.MakeASave() >= GetComponent<Character>().save)
                    {
                        target.resistance = 0.5f;
                        Debug.Log($"{target.name} succeeded on a save, and takes only half damage!");
                    }
                    else
                        Debug.Log($"{target.name} failed its save and takes full damage.");
                }
                ApplyEffect(skill, targets);
                foreach (var target in targets)
                {
                    if (target.MakeASave() >= GetComponent<Character>().save)
                    {
                        target.resistance = 1f;
                    }
                }
                break;
        }

        var data = skillStates[skill];
        data.currentCooldown = skill.cooldown;
        if (skill.limitedCharges && skill.maxCharges > 0)
            data.chargesLeft--;
        character.AP -= skill.cost;
        if(TurnManager.Instance.currentUnit == character)
        {
            CharacterMenu.instance.UpdateBarValue(character.AP, character.maxAP, false);
        }

        // Here’s where you’d hook in the actual effect application
        Debug.Log($"{gameObject.name} used {skill.skillName}!");
        TurnManager.Instance.ShowMovementOutline();
    }

    private void ApplyEffect(SkillsScriptableObject skill, List<Character> targets)
    {
        Debug.Log("Apllying effects");
        
        Character user = GetComponent<Character>();

        Debug.Log($"Appplying effects to {targets.Count} targets");
        foreach (var target in targets)
        {
            Debug.Log($"Applying effects to {target.name}");
            foreach (var effect in skill.effects)
            {
                effect.ApplyEffect(user, target, skill);
                Debug.Log($"Applied {effect.name}");
            }
        }
    }

    // Helper struct to track runtime values
    private class SkillRuntimeData
    {
        public int currentCooldown;
        public int chargesLeft;
        public bool limitedCharges;

        public SkillRuntimeData(SkillsScriptableObject skill)
        {
            currentCooldown = 0;
            limitedCharges = skill.limitedCharges;
            chargesLeft = skill.limitedCharges ? skill.maxCharges : -1;
        }
    }
}
