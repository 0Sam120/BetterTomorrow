using UnityEngine;

[CreateAssetMenu(menuName = "Skill Effects/Heal")]
public class HealEffect : SkillEffect
{
    public override void ApplyEffect(Character user, Character target, SkillsScriptableObject skill)
    {
        var stats = target.GetComponent<Character>();
        if (stats != null)
        {
            int healing = Mathf.RoundToInt(Random.Range(1, skill.effectPower)); // pull from skill values
            stats.Heal(healing);
            Debug.Log($"{user.name} healed {healing} hit points with {skill.skillName}!");
        }
    }
}
