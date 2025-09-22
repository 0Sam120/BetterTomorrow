using UnityEngine;
using System.Collections.Generic;

public class SkillComponent : MonoBehaviour
{
    // Skills this unit knows
    public List<SkillsScriptableObject> knownSkills = new List<SkillsScriptableObject>();

    // Runtime state per skill (cooldowns, charges, etc.)
    private Dictionary<SkillsScriptableObject, SkillRuntimeData> skillStates = new Dictionary<SkillsScriptableObject, SkillRuntimeData>();

    private void Awake()
    {
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
        if (data.currentCooldown > 0) return false;
        if (data.limitedCharges && data.chargesLeft <= 0) return false;

        // May also add resource checks here (AP/mana/etc.)
        return true;
    }

    // Use a skill (starts cooldown, reduces charges)
    public void UseSkill(SkillsScriptableObject skill)
    {
        if (!CanUseSkill(skill)) return;

        var data = skillStates[skill];
        data.currentCooldown = skill.cooldown;
        if (skill.limitedCharges && skill.maxCharges > 0)
            data.chargesLeft--;

        // Here’s where you’d hook in the actual effect application
        Debug.Log($"{gameObject.name} used {skill.skillName}!");
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
