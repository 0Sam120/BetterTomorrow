using UnityEngine;

[CreateAssetMenu(menuName = "Skill Effects/Spawn Cover")]
public class SpawnCoverEffect : SkillEffect
{
    public GameObject coverPrefab;
    public int duration;

    public override void ApplyEffect(Character user, Character target, SkillsScriptableObject skill)
    {
        var pos = target.transform.position; // or picked tile
        var cover = GameObject.Instantiate(coverPrefab, pos, Quaternion.identity);
        GameObject.Destroy(cover, duration);
        Debug.Log($"{user.name} spawned cover at {pos}!");
    }
}
