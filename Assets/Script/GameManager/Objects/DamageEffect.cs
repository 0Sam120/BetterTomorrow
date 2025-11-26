using NUnit.Framework;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Skill Effects/Damage")]
public class DamageEffect : SkillEffect
{
    public DamageTypes damageType;
    
    public override void ApplyEffect(Character user, Character target, SkillsScriptableObject skill)
    {
        var stats = target.GetComponent<Character>();
        if (stats != null)
        {
            int damage = Mathf.RoundToInt(Random.Range(1, skill.effectPower)); // pull from skill values
            damage += user.GetComponent<CharacterTurn>().CheckMomentum(damage, damageType, user);
            stats.TakeDamage(damage, damageType);
            Debug.Log($"{user.name} dealt {damage} damage with {skill.skillName}!");
        }
    }
}


