using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Skill Effects/Damage")]
public class DamageEffect : SkillEffect
{
    public override void ApplyEffect(GameObject user, GameObject target, SkillsScriptableObject skill)
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

[CreateAssetMenu(menuName = "Skill Effects/Heal")]
public class HealEffect : SkillEffect
{
    public override void ApplyEffect(GameObject user, GameObject target, SkillsScriptableObject skill)
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

[CreateAssetMenu(menuName = "Skill Effects/Spawn Cover")]
public class SpawnCoverEffect : SkillEffect
{
    public GameObject coverPrefab;
    public int duration;

    public override void ApplyEffect(GameObject user, GameObject target, SkillsScriptableObject skill)
    {
        var pos = target.transform.position; // or picked tile
        var cover = GameObject.Instantiate(coverPrefab, pos, Quaternion.identity);
        GameObject.Destroy(cover, duration);
        Debug.Log($"{user.name} spawned cover at {pos}!");
    }
}


