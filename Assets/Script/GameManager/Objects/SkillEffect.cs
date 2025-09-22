using UnityEngine;

public abstract class SkillEffect : ScriptableObject
{
    public abstract void ApplyEffect(GameObject user, GameObject target, SkillsScriptableObject skill);

}
