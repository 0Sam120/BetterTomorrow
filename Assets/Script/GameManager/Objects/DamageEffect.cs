using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Skill Effects/Damage")]
public class DamageEffect : SkillEffect
{
    public override void ApplyEffect(Character user, Character target, SkillsScriptableObject skill)
    {
        var stats = target.GetComponent<Character>();
        if (stats != null)
        {
            int damage = Mathf.RoundToInt(Random.Range(1, skill.effectPower)); // pull from skill values
            stats.TakeDamage(damage);
            Debug.Log($"{user.name} dealt {damage} damage with {skill.skillName}!");
        }
    }
}


